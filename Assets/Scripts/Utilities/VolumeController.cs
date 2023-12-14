using Assets.Scripts;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumeController : MonoBehaviour
{
    [SerializeField] private Volume globalVolume;
    private VolumeProfile volumeProfile;
    private MotionBlur motionblur;
    private LensDistortion lensDistortion;
    private Bike arcadeBike;
    private bool slowMotion;

    public void SetBike(Bike bike)
    {
        arcadeBike = bike;
    }

    private void Start()
    {
        volumeProfile = globalVolume.sharedProfile;
        volumeProfile.TryGet(out motionblur);
        volumeProfile.TryGet(out lensDistortion);
    }

    public void ReduceMotionBlur()
    {
        StartCoroutine(ReduceMotionBlurSmooth());
    }
    public void RecoverMotionBlur()
    {
        StartCoroutine(RecoverMotionBlurSmooth());
    }

    private IEnumerator RecoverMotionBlurSmooth()
    {
        while(motionblur.intensity.value <= arcadeBike.CurrentSpeed / 30)
        {
            motionblur.intensity.value += Time.deltaTime;
            lensDistortion.intensity.value -= Time.deltaTime * .25f;
            yield return null;
        }
        slowMotion = false;
    }
    private IEnumerator ReduceMotionBlurSmooth()
    {
        slowMotion = true;
        while (motionblur.intensity.value >= .1f)
        {
            motionblur.intensity.value -= Time.deltaTime;
            lensDistortion.intensity.value += Time.deltaTime *.25f;
            yield return null;
        }
    }
    private void Update()
    {
        if (arcadeBike != null && motionblur != null && !slowMotion)
        {
            motionblur.intensity.value = arcadeBike.CurrentSpeed / 30;
            lensDistortion.intensity.value = -arcadeBike.CurrentSpeed / 64;
        }
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
