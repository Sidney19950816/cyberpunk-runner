using Assets.Scripts.Abstractions;
using JetBrains.Annotations;
using UnityEngine;

public class SlowMotion : BaseBehaviour
{
    [SerializeField] [UsedImplicitly] private AudioClip _slowMotionInAudioClip;
    [SerializeField] [UsedImplicitly] private AudioClip _slowMotionOutAudioClip;
    [SerializeField] [UsedImplicitly] private AudioClip _slowMotionHeartAudioClip;

    [SerializeField] [UsedImplicitly] private float _slowMotionTimeScale = 0.05f;

    private AudioSource _audioSourceInOut;
    private AudioSource _audioSourceHeart;
    
    private bool _inSlowMotion;

    private float _initialFixedDeltaTime;

    [UsedImplicitly]
    private void Awake()
    {
        _audioSourceInOut = gameObject.AddComponent<AudioSource>();
        _audioSourceHeart = gameObject.AddComponent<AudioSource>();
    }

    [UsedImplicitly]
    private void Start()
    {
        _initialFixedDeltaTime = Time.fixedDeltaTime;
    }

    [UsedImplicitly]
    private void FixedUpdate()
    {
        var time = _inSlowMotion ? _slowMotionTimeScale : 1f;

        Time.timeScale = Mathf.Lerp(Time.timeScale, time, 0.1f);
        Time.fixedDeltaTime = _initialFixedDeltaTime * Time.timeScale;
    }

    public void SetSlowMotionState(bool state)
    {
        _inSlowMotion = state;

        if (state)
            StartAudio();
        else
            EndAudio();
    }

    private void StartAudio()
    {
        _audioSourceInOut.PlayOneShot(_slowMotionInAudioClip);
        _audioSourceHeart.PlayOneShot(_slowMotionHeartAudioClip);
    }

    private void EndAudio()
    {
        _audioSourceHeart.Stop();
        _audioSourceInOut.PlayOneShot(_slowMotionOutAudioClip);
    }
}
