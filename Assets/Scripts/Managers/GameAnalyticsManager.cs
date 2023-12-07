using GameAnalyticsSDK;
using UnityEngine;
using Assets.Scripts;

public class GameAnalyticsManager : BaseBehaviour, IGameAnalyticsATTListener
{
    void Start()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            GameAnalytics.RequestTrackingAuthorization(this);
        }
        else
        {
            GameAnalytics.Initialize();
        }
    }

    public void GameAnalyticsATTListenerNotDetermined()
    {
        GameAnalytics.Initialize();
    }
    public void GameAnalyticsATTListenerRestricted()
    {
        GameAnalytics.Initialize();
    }
    public void GameAnalyticsATTListenerDenied()
    {
        GameAnalytics.Initialize();
    }
    public void GameAnalyticsATTListenerAuthorized()
    {
        GameAnalytics.Initialize();
    }

    public void NewBusinessEvent(string currency, int amount, string itemType, string itemId, string cartType)
    {
        GameAnalytics.NewBusinessEvent(currency, amount, itemType, itemId, cartType);

        //TODO: Request receipt
    }
}
