using System.Collections.Generic;
using UnityEngine;

namespace PG
{
    /// <summary>
    /// Sound effects, using FMOD.
    /// </summary>
    public class VehicleSFX :MonoBehaviour
    {
        [Header("VehicleSFX")]

        [Header("Suspension sounds")]
        [SerializeField] float LowSuspensionForce = 0.2f;
        [SerializeField] AudioClip LowSuspensionClip;
        [SerializeField] float MediumSuspensionForce = 0.4f;
        [SerializeField] AudioClip MediumSuspensionClip;
        [SerializeField] float HighSuspensionForce = 0.6f;
        [SerializeField] AudioClip HighSuspensionClip;
        [SerializeField] float MinTimeBetweenSuspensionSounds = 0.2f;

        [Header("Ground effects")]
        [SerializeField] AudioSource WheelsEffectSourceRef;                                 //Wheel source, for playing slip sounds.

        [Header("Collisions")]
        [SerializeField] float MinTimeBetweenCollisions = 0.1f;
        [SerializeField] float DefaultMagnitudeDivider = 20;                                //default divider to calculate collision volume.
        [SerializeField] AudioClip DefaultCollisionClip;                                    //Event playable if the desired one was not found.
        [SerializeField] List<ColissionEvent> CollisionEvents = new();

        [Header("Frictions")]
        [SerializeField] AudioSource FrictionEffectSourceRef;
        [SerializeField] float PlayFrictionTime = 0.5f;
        [SerializeField] AudioClip DefaultFrictionClip;                                     //Event playable if the desired one was not found.
        [SerializeField] List<ColissionEvent> FrictionEvents = new();

        [Header("Other settings")]
        public AudioSource OtherEffectsSource;                                              //Source for playing other sound effects

#pragma warning restore 0649

        Dictionary<GroundConfig, WheelSoundData> WheelSounds = new();              //Dictionary for playing multiple wheel sounds at the same time.\
        Dictionary<AudioClip, FrictionSoundData> FrictionSounds = new();           //Dictionary for playing multiple friction sounds at the same time.

        protected VehicleController Vehicle;
        AudioClip CurrentFrictionClip;
        float LastCollisionTime;

        Dictionary<AudioClip, float> LastPlaySoundTime = new();

        protected virtual void Start ()
        {
            Vehicle = GetComponentInParent<VehicleController> ();

            if (Vehicle == null)
            {
                Debug.LogErrorFormat ("[{0}] VehicleSFX without VehicleController in parent", name);
                enabled = false;
                return;
            }

            //Subscribe to collisions.
            Vehicle.CollisionAction += PlayCollisionSound;
            Vehicle.CollisionStayAction += PlayCollisionStayAction;

            //Setting default values.
            WheelsEffectSourceRef.volume = 0;
            FrictionEffectSourceRef.volume = 0;

            FrictionSounds.Add (FrictionEffectSourceRef.clip, new FrictionSoundData () { Source = FrictionEffectSourceRef, LastFrictionTime = Time.time });
            FrictionEffectSourceRef.Stop ();
        }

        protected virtual void Update ()
        {
            UpdateWheels ();
            UpdateFrictions ();
            UpdateSuspension ();
        }

        private void OnDestroy ()
        {
            foreach (var soundKV in WheelSounds)
            {
                if (soundKV.Value.Source)
                {
                    soundKV.Value.Source.Stop ();
                }
            }

            foreach (var soundKV in FrictionSounds)
            {
                if (soundKV.Value.Source)
                {
                    soundKV.Value.Source.Stop ();
                }
            }
        }

        void UpdateWheels ()
        {
            //Wheels sounds logic.
            //Find the sound for each wheel.
            foreach (var wheel in Vehicle.Wheels)
            {
                if (wheel.IsDead)
                {
                    continue;
                }

                WheelSoundData sound = null;

                if (!WheelSounds.TryGetValue (wheel.CurrentGroundConfig, out sound))
                {
                    var source = WheelsEffectSourceRef.gameObject.AddComponent<AudioSource>();
                    source.playOnAwake = WheelsEffectSourceRef.playOnAwake;
                    source.spatialBlend = WheelsEffectSourceRef.spatialBlend;
                    source.clip = wheel.CurrentGroundConfig.IdleAudioClip;
                    source.Stop ();
                    source.volume = 0;
                    sound = new WheelSoundData ()
                    {
                        Source = source
                    };
                    WheelSounds.Add (wheel.CurrentGroundConfig, sound);
                }

                sound.WheelsCount++;

                //Find the maximum slip for each sound.
                if (wheel.SlipNormalized > sound.Slip)
                {
                    sound.Slip = wheel.SlipNormalized;
                }
            }

            var speedNormalized = (Vehicle.CurrentSpeed / 30).Clamp();

            foreach (var sound in WheelSounds)
            {
                AudioClip clip;
                float targetVolume;

                if (sound.Value.Slip >= 0.9f)
                {
                    clip = sound.Key.SlipAudioClip;
                    targetVolume = (sound.Value.Slip - 0.5f).Clamp ();
                }
                else
                {
                    clip = sound.Key.IdleAudioClip;
                    targetVolume = (Vehicle.CurrentSpeed / 30).Clamp ();
                }

                if (sound.Value.Source.clip != clip && clip != null)
                {
                    sound.Value.Source.clip = clip;
                }

                if (sound.Value.WheelsCount == 0 || speedNormalized == 0 || clip == null)
                {
                    targetVolume = 0;
                }

                //Passing parameters to sources.
                sound.Value.Source.volume = Mathf.Lerp (sound.Value.Source.volume, targetVolume, 10 * Time.deltaTime);
                sound.Value.Source.pitch = Mathf.Lerp (0.7f, 1.2f, sound.Value.Source.volume);

                sound.Value.Slip = 0;
                sound.Value.WheelsCount = 0;

                if (Mathf.Approximately (0, sound.Value.Source.volume) && sound.Value.Source.isPlaying)
                {
                    sound.Value.Source.Stop ();
                }
                else if (!Mathf.Approximately (0, sound.Value.Source.volume) && !sound.Value.Source.isPlaying)
                {
                    sound.Value.Source.Play ();
                }
            }
        }

        void UpdateFrictions ()
        {
            FrictionSoundData soundData;
            var speedNormalized = (Vehicle.CurrentSpeed / 30).Clamp();

            foreach (var sound in FrictionSounds)
            {
                soundData = sound.Value;
                if (soundData.Source.isPlaying)
                {
                    var time = Time.time - soundData.LastFrictionTime;

                    if (time > PlayFrictionTime)
                    {
                        sound.Value.Source.pitch = 0;
                        sound.Value.Source.volume = 0;
                        soundData.Source.Stop ();
                    }
                    else
                    {
                        sound.Value.Source.pitch = Mathf.Lerp (0.4f, 1.2f, speedNormalized);
                        soundData.Source.volume = speedNormalized * (1 - (time / soundData.LastFrictionTime));
                    }
                }
            }
        }

        void UpdateSuspension ()
        {
            if (Time.timeSinceLevelLoad < 0.5f || !HighSuspensionClip && !MediumSuspensionClip && !LowSuspensionClip)   //Time.timeSinceLevelLoad < 1f to delay logic on startup
            {
                return;
            }

            var maxSuspensionForceWheel = Vehicle.Wheels[0];
            for (var i = 1; i < Vehicle.Wheels.Length; i++)
            {
                if (Vehicle.Wheels[i].SuspensionPosDiff > maxSuspensionForceWheel.SuspensionPosDiff)
                {
                    maxSuspensionForceWheel = Vehicle.Wheels[i];
                }
            }

            var suspensionForce = maxSuspensionForceWheel.SuspensionPosDiff * maxSuspensionForceWheel.WheelCollider.suspensionDistance * 10;
            float lastPlayTime;

            if (suspensionForce >= HighSuspensionForce && HighSuspensionClip)
            {
                if (!LastPlaySoundTime.TryGetValue (HighSuspensionClip, out lastPlayTime) || lastPlayTime < Time.timeSinceLevelLoad - MinTimeBetweenSuspensionSounds)
                {
                    OtherEffectsSource.PlayOneShot (HighSuspensionClip, (suspensionForce + 0.4f).Clamp ());
                    LastPlaySoundTime[HighSuspensionClip] = Time.timeSinceLevelLoad;
                }
            }
            else if (suspensionForce >= MediumSuspensionForce && MediumSuspensionClip)
            {
                if (!LastPlaySoundTime.TryGetValue (MediumSuspensionClip, out lastPlayTime) || lastPlayTime < Time.timeSinceLevelLoad - MinTimeBetweenSuspensionSounds)
                {
                    OtherEffectsSource.PlayOneShot (MediumSuspensionClip, (suspensionForce + 0.4f).Clamp ());
                    LastPlaySoundTime[MediumSuspensionClip] = Time.timeSinceLevelLoad;
                }
            }
            else if (suspensionForce >= LowSuspensionForce && LowSuspensionClip)
            {
                if (!LastPlaySoundTime.TryGetValue (LowSuspensionClip, out lastPlayTime) || lastPlayTime < Time.timeSinceLevelLoad - MinTimeBetweenSuspensionSounds)
                {
                    OtherEffectsSource.PlayOneShot (LowSuspensionClip, (suspensionForce + 0.4f).Clamp ());
                    LastPlaySoundTime[LowSuspensionClip] = Time.timeSinceLevelLoad;
                }
            }

        }

        #region Collisions

        /// <summary>
        /// Play collision stay sound.
        /// </summary>
        public void PlayCollisionStayAction (VehicleController vehicle, Collision collision)
        {
            if (Vehicle.CurrentSpeed >= 1 && (collision.rigidbody == null || (collision.rigidbody.velocity - vehicle.RB.velocity).sqrMagnitude > 25))
            {
                PlayFrictionSound (collision, collision.relativeVelocity.magnitude);
            }
        }

        /// <summary>
        /// Play collision sound.
        /// </summary>
        public void PlayCollisionSound (VehicleController vehicle, Collision collision)
        {
            if (!vehicle.VehicleIsVisible || collision == null)
                return;

            var collisionLayer = collision.gameObject.layer;

            if (Time.time - LastCollisionTime < MinTimeBetweenCollisions)
            {
                return;
            }

            LastCollisionTime = Time.time;
            float collisionMagnitude = 0;
            if (collision.rigidbody)
            {
                collisionMagnitude = (Vehicle.RB.velocity - collision.rigidbody.velocity).magnitude;
            }
            else
            {
                collisionMagnitude = collision.relativeVelocity.magnitude;
            }
            float magnitudeDivider;

            var audioClip = GetClipForCollision (collisionLayer, collisionMagnitude, out magnitudeDivider);

            var volume = Mathf.Clamp01 (collisionMagnitude / magnitudeDivider.Clamp(0, 40));

            OtherEffectsSource.PlayOneShot (audioClip, volume);
        }

        void PlayFrictionSound (Collision collision, float magnitude)
        {
            if (Vehicle.CurrentSpeed >= 1)
            {
                CurrentFrictionClip = GetClipForFriction (collision.collider.gameObject.layer, magnitude);

                FrictionSoundData soundData;
                if (!FrictionSounds.TryGetValue (CurrentFrictionClip, out soundData))
                {
                    var source = FrictionEffectSourceRef.gameObject.AddComponent<AudioSource>();
                    source.clip = CurrentFrictionClip;

                    soundData = new FrictionSoundData () { Source = source };
                    FrictionSounds.Add (CurrentFrictionClip, soundData);
                }

                if (!soundData.Source.isPlaying)
                {
                    soundData.Source.Play ();
                }

                soundData.LastFrictionTime = Time.time;
            }
        }

        /// <summary>
        /// Search for the desired event based on the collision magnitude and the collision layer.
        /// </summary>
        /// <param name="layer">Collision layer.</param>
        /// <param name="collisionMagnitude">Collision magnitude.</param>
        /// <param name="magnitudeDivider">Divider to calculate collision volume.</param>
        AudioClip GetClipForCollision (int layer, float collisionMagnitude, out float magnitudeDivider)
        {
            for (var i = 0; i < CollisionEvents.Count; i++)
            {
                if (CollisionEvents[i].CollisionMask.LayerInMask (layer) && collisionMagnitude >= CollisionEvents[i].MinMagnitudeCollision && collisionMagnitude < CollisionEvents[i].MaxMagnitudeCollision)
                {
                    if (CollisionEvents[i].MaxMagnitudeCollision == float.PositiveInfinity)
                    {
                        magnitudeDivider = DefaultMagnitudeDivider;
                    }
                    else
                    {
                        magnitudeDivider = CollisionEvents[i].MaxMagnitudeCollision;
                    }

                    return CollisionEvents[i].AudioClip;
                }
            }

            magnitudeDivider = DefaultMagnitudeDivider;
            return DefaultCollisionClip;
        }

        /// <summary>
        /// Search for the desired event based on the friction magnitude and the collision layer.
        /// </summary>
        /// <param name="layer">Collision layer.</param>
        /// <param name="collisionMagnitude">Collision magnitude.</param>
        AudioClip GetClipForFriction (int layer, float collisionMagnitude)
        {
            for (var i = 0; i < FrictionEvents.Count; i++)
            {
                if (FrictionEvents[i].CollisionMask.LayerInMask (layer) && collisionMagnitude >= FrictionEvents[i].MinMagnitudeCollision && collisionMagnitude < FrictionEvents[i].MaxMagnitudeCollision)
                {
                    return FrictionEvents[i].AudioClip;
                }
            }

            return DefaultFrictionClip;
        }

        #endregion //Collisions

        [System.Serializable]
        public struct ColissionEvent
        {
            public AudioClip AudioClip;
            public LayerMask CollisionMask;
            public float MinMagnitudeCollision;
            public float MaxMagnitudeCollision;
        }

        public class FrictionSoundData
        {
            public AudioSource Source;
            public float LastFrictionTime;
        }

        public class WheelSoundData
        {
            public AudioSource Source;
            public float Slip;
            public int WheelsCount;
        }
    }
}
