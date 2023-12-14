using System;
using UnityEngine;
using GoogleMobileAds.Api;

public class RewardedAdController : MonoBehaviour
{
    // These ad units are configured to always serve test ads.
#if UNITY_ANDROID
    private const string adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
    private const string adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
    private const string adUnitId = "unused";
#endif

    private RewardedAd rewardedAd;

    /// <summary>
    /// Loads the ad.
    /// </summary>
    public void LoadAd()
    {
        // Clean up the old ad before loading a new one.
        if (rewardedAd != null)
        {
            DestroyAd();
        }

        Debug.Log("Loading rewarded ad.");

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // Send the request to load the ad.
        RewardedAd.Load(adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            // If the operation failed with a reason.
            if (error != null)
            {
                Debug.LogError("Rewarded ad failed to load an ad with error : " + error);
                return;
            }
            // If the operation failed for unknown reasons.
            // This is an unexpected error, please report this bug if it happens.
            if (ad == null)
            {
                Debug.LogError("Unexpected error: Rewarded load event fired with null ad and null error.");
                return;
            }

            // The operation completed successfully.
            Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
            rewardedAd = ad;

            // Register to ad events to extend functionality.
            RegisterEventHandlers(ad);
        });
    }

    /// <summary>
    /// Shows the ad.
    /// </summary>
    public void ShowAd(Action OnComplete)
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            RegisterReloadHandler(rewardedAd);
            Debug.Log("Showing rewarded ad.");
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log(String.Format("Rewarded ad granted a reward: {0} {1}",
                                        reward.Amount,
                                        reward.Type));
                OnComplete?.Invoke();
            });
        }
        else
        {
            Debug.LogError("Rewarded ad is not ready yet.");
        }
    }

    /// <summary>
    /// Destroys the ad.
    /// </summary>
    public void DestroyAd()
    {
        if (rewardedAd != null)
        {
            Debug.Log("Destroying rewarded ad.");
            rewardedAd.Destroy();
            rewardedAd = null;
        }
    }

    /// <summary>
    /// Logs the ResponseInfo.
    /// </summary>
    public void LogResponseInfo()
    {
        if (rewardedAd != null)
        {
            var responseInfo = rewardedAd.GetResponseInfo();
            Debug.Log(responseInfo);
        }
    }

    private void Awake()
    {
        if (rewardedAd == null)
        {
            LoadAd();
        }
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when the ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content with error : "
                + error);

        };
    }

    private void RegisterReloadHandler(RewardedAd ad)
    {
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded Ad full screen content closed.");

            // Reload the ad so that we can show another as soon as possible.
            LoadAd();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);

            // Reload the ad so that we can show another as soon as possible.
            LoadAd();
        };
    }
}
