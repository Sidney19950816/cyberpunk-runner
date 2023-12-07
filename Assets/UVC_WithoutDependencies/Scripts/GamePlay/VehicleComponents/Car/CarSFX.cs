using System.Collections.Generic;
using UnityEngine;

namespace PG
{
    /// <summary>
    /// Sound effects, using FMOD.
    /// </summary>
    public class CarSFX :VehicleSFX
    {
        [Header("CarSFX")]

#pragma warning disable 0649
        
        [SerializeField] AudioSource EngineSourceRef;
        [SerializeField] AudioClip LowEngineClip;
        [SerializeField] AudioClip MediumEngineClip;
        [SerializeField] AudioClip HighEngineClip;

        [SerializeField] float MinEnginePitch = 0.5f;
        [SerializeField] float MaxEnginePitch = 1.5f;

        [Header("Additional settings")]
        [SerializeField] AudioSource TurboSource;
        [SerializeField] AudioClip TurboBlowOffClip;
        [SerializeField] float MaxBlowOffVolume = 0.5f;
        [SerializeField] float MinTimeBetweenBlowOffSounds = 1;
        [SerializeField] float MaxTurboVolume = 0.5f;
        [SerializeField] float MinTurboPith = 0.5f;
        [SerializeField] float MaxTurboPith = 1.5f;

        [SerializeField] AudioSource BoostSource;

        [SerializeField] List<AudioClip> BackFireClips;

#pragma warning restore 0649

        CarController Car;
        float LastBlowOffTime;
        float[] EngineSourcesRanges = new float[1] { 1f };
        List<AudioSource> EngineSources = new();

        protected override void Start ()
        {
            base.Start ();

            Car = Vehicle as CarController;

            if (Car == null)
            {
                Debug.LogErrorFormat ("[{0}] CarSFX without CarController in parent", name);
                enabled = false;
                return;
            }

            if (!Car.Engine.EnableBoost && BoostSource)
            {
                BoostSource.Stop ();
            }

            if (!Car.Engine.EnableTurbo && TurboSource)
            {
                TurboSource.Stop ();
            }

            Car.BackFireAction += OnBackFire;

            //Create engine sounds list.
            var engineClips = new List<AudioClip>();
            if (LowEngineClip != null)
            {
                engineClips.Add (LowEngineClip);
            }
            if (MediumEngineClip != null)
            {
                engineClips.Add (MediumEngineClip);
            }
            if (HighEngineClip != null)
            {
                engineClips.Add (HighEngineClip);
            }

            if (engineClips.Count == 2)
            {
                //If the engine has 2 sounds, then they will switch at 30% rpm.
                EngineSourcesRanges = new float[2] { 0.3f, 1f };
            }
            else if (engineClips.Count == 3)
            {
                //If the engine has 3 sounds, then they will switch at 30% and 60% rpm.
                EngineSourcesRanges = new float[3] { 0.3f, 0.6f, 1f };
            }

            //Init Engine sounds.
            if (engineClips != null && engineClips.Count > 0)
            {
                if (EngineSourceRef == null)
                {
                    Debug.LogError ("Engine source is NULL");
                    return;
                }

                AudioSource engineSource;

                for (var i = 0; i < engineClips.Count; i++)
                {
                    if (EngineSourceRef.clip == engineClips[i])
                    {
                        engineSource = EngineSourceRef;
                        EngineSourceRef.transform.SetSiblingIndex (EngineSourceRef.transform.parent.childCount);
                    }
                    else
                    {
                        engineSource = Instantiate (EngineSourceRef, EngineSourceRef.transform.parent);
                        engineSource.clip = engineClips[i];
                        engineSource.Play ();
                    }

                    engineSource.name = string.Format ("Engine source ({0})", i);
                    EngineSources.Add (engineSource);
                }

                if (!EngineSources.Contains (EngineSourceRef))
                {
                    Destroy (EngineSourceRef);
                }
            }
        }


        protected override void Update ()
        {
            base.Update ();
            UpdateEngine ();
            UpdateTurbo ();
            UpdateBoost ();
        }

        // Engine sound logic.
        void UpdateEngine ()
        {
            if (EngineSources.Count == 0 && EngineSourceRef)
            {
                EngineSourceRef.pitch = Mathf.Lerp (MinEnginePitch, MaxEnginePitch, (Car.EngineRPM - Car.MinRPM) / (Car.MaxRPM - Car.MinRPM));
            }
            else if (EngineSources.Count > 1)
            {
                var rpmNorm = ((Car.EngineRPM - Car.MinRPM) / (Car.MaxRPM - Car.MinRPM)).Clamp();
                var pith = Mathf.Lerp (MinEnginePitch, MaxEnginePitch, rpmNorm);

                for (var i = 0; i < EngineSources.Count; i++)
                {
                    EngineSources[i].pitch = pith;

                    if (i > 0 && rpmNorm < EngineSourcesRanges[i - 1])
                    {
                        EngineSources[i].volume = Mathf.InverseLerp (0.2f, 0, EngineSourcesRanges[i - 1] - rpmNorm);
                    }
                    else if (rpmNorm > EngineSourcesRanges[i])
                    {
                        EngineSources[i].volume = Mathf.InverseLerp (0.3f, 0, rpmNorm - EngineSourcesRanges[i]);
                    }
                    else
                    {
                        EngineSources[i].volume = 1;
                    }

                    if (Mathf.Approximately(EngineSources[i].volume, 0) && EngineSources[i].isPlaying)
                    {
                        EngineSources[i].Stop ();
                    }

                    if (EngineSources[i].volume > 0 && !EngineSources[i].isPlaying)
                    {
                        EngineSources[i].Play ();
                    }
                }
            }
            
        }

        //Additional turbo sound
        void UpdateTurbo ()
        {
            if (Car.Engine.EnableTurbo && TurboSource)
            {
                TurboSource.volume = Mathf.Lerp (0, MaxTurboVolume, Car.CurrentTurbo);
                TurboSource.pitch = Mathf.Lerp (MinTurboPith, MaxTurboPith, Car.CurrentTurbo);
                if (Car.CurrentTurbo > 0.2f && (Car.CurrentAcceleration < 0.2f || Car.InChangeGear) && ((Time.realtimeSinceStartup - LastBlowOffTime) > MinTimeBetweenBlowOffSounds))
                {
                    OtherEffectsSource.PlayOneShot (TurboBlowOffClip, Car.CurrentTurbo * MaxBlowOffVolume);
                    LastBlowOffTime = Time.realtimeSinceStartup;
                }
            }
        }

        //Additional boost sound
        void UpdateBoost ()
        {
            if (Car.Engine.EnableBoost && BoostSource)
            {
                if (Car.InBoost && !BoostSource.isPlaying)
                {
                    BoostSource.Play ();
                }
                if (!Car.InBoost && BoostSource.isPlaying)
                {
                    BoostSource.Stop ();
                }
            }
        }

        void OnBackFire ()
        {
            if (BackFireClips != null && BackFireClips.Count > 0)
            {
                OtherEffectsSource.PlayOneShot (BackFireClips[Random.Range (0, BackFireClips.Count - 1)]);
            }
        }
    }
}
