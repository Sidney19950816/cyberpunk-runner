using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Util
{
    public static GameObject FindParentWithTag(GameObject childObject, string tag)
    {
        var t = childObject.transform;
        while (t.parent != null)
        {
            if (t.parent.tag == tag)
            {
                return t.parent.gameObject;
            }

            t = t.parent.transform;
        }

        return null; // Could not find a parent with given tag.
    }

    public static bool IsMobilePlatform
    {
        get
        {
#if UNITY_EDITOR

            return UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android ||
                   UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS;
#else
                return Application.isMobilePlatform;
#endif
        }
    }

    public static async Task<T> RetryEconomyFunction<T>(Func<Task<T>> functionToRetry, int retryAfterSeconds)
    {
        if (retryAfterSeconds > 60)
        {
            Debug.Log($"Economy returned a rate limit exception with an extended Retry After time " +
                      $"of {retryAfterSeconds} seconds. Suggest manually retrying at a later time.");
            return default;
        }

        Debug.Log($"Economy returned a rate limit exception. Retrying after {retryAfterSeconds} seconds");

        try
        {
            // Using a CancellationToken allows us to ensure that the Task.Delay gets cancelled if we exit
            // playmode while it's waiting its delay time. Without it, it would continue trying to execute
            // the rest of this code, even outside of playmode.
            using (var cancellationTokenHelper = new Assets.Scripts.Threading.CancellationTokenHelper())
            {
                var cancellationToken = cancellationTokenHelper.cancellationToken;

                await Task.Delay(retryAfterSeconds * 1000, cancellationToken);

                // Call the function that we passed in to this method after the retry after time period has passed.
                var result = await functionToRetry();

                if (cancellationToken.IsCancellationRequested)
                {
                    return default;
                }

                Debug.Log("Economy retry successfully completed");

                return result;
            }
        }
        catch (OperationCanceledException)
        {
            return default;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        return default;
    }

    public static void SetSelectedItemId(string key, string value = null)
    {
        string v = value;

        if (string.IsNullOrEmpty(v))
        {
            var playerInventoryItem = EconomyManager.Instance.PlayersInventoryItems.FirstOrDefault(i => i.InventoryItemId.Contains(key));
            v = playerInventoryItem?.InventoryItemId ?? string.Empty;
        }

        PlayerPrefs.SetString($"SELECTED_{key}", v);
    }

    public static string GetSelectedItemId(string key)
    {
        string k = $"SELECTED_{key}";

        if (!PlayerPrefs.HasKey(k))
        {
            SetSelectedItemId(key);
        }
        return PlayerPrefs.GetString(k);
    }

    public static void Vibrate()
    {
       if(PlayerPrefsUtil.GetVibrationEnabled())
            Handheld.Vibrate();
    }

    public static void PlaySound(AudioClip audioClip, AudioSource audioSource)
    {
        if (PlayerPrefsUtil.GetSoundsEnabled())
            audioSource.PlayOneShot(audioClip);
    }

    public static Sprite GetCurrencySprite(CurrencyType currencyType)
    {
        return Resources.Load<Sprite>($"UI/Currencies/{currencyType}");
    }

    public static float GetCurrentAnimatorLength(Animator animator, int layer = 0)
    {
        return animator.GetCurrentAnimatorStateInfo(layer).length;
    }

    public static float GetCurrentAnimationSpeed(Animator animator, int layer = 0)
    {
        return GetCurrentAnimatorLength(animator, layer) / animator.GetCurrentAnimatorStateInfo(layer).speed;
    }

    public static float GetAnimClipLength(Animator animator, string clipName)
    {
        var clips = animator.runtimeAnimatorController.animationClips;
        foreach (var clip in clips)
        {
            return clip.name.Equals(clipName) ? clip.length : 0;
        }
        return 0;
    }

    public static void CameraShake(MonoBehaviour monoBehaviour, float duration, float amplitude)
    {
        monoBehaviour.StartCoroutine(CameraShakeCoroutine(duration, amplitude));
    }

    private static IEnumerator CameraShakeCoroutine(float duration, float amplitude)
    {
        CinemachineVirtualCamera vCamera = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera as CinemachineVirtualCamera;
        CinemachineBasicMultiChannelPerlin cameraNoise = vCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cameraNoise.m_AmplitudeGain = amplitude;
        yield return new WaitForSeconds(duration);
        cameraNoise.m_AmplitudeGain = 0;
    }
}
