using System.Collections.Generic;
using Facebook.Unity;

public static class FacebookAppEvents
{
    public static void LogUserIDEvent(string userId)
    {
        var parameters = new Dictionary<string, object> {
            { "User ID", userId }
        };
        LogAppEvent("user_id", null, parameters);
    }

    public static void LogSignUpEvent(string signUpMethod)
    {
        var parameters = new Dictionary<string, object> {
            { "Sign-Up", signUpMethod }
        };

        LogAppEvent("user_signup", null, parameters);
    }

    public static void LogSettingEvent(string setting)
    {
        var parameters = new Dictionary<string, object> {
            { "User Setting", setting }
        };

        LogAppEvent("user_setting", null, parameters);
    }

    public static void LogShopEvent(string itemName, decimal? price)
    {
        var parameters = new Dictionary<string, object>
            {
                { AppEventParameterName.ContentID, itemName }
            };

        LogAppEvent("user_shop", (float)price, parameters);
    }

    public static void LogGarageEvent(string itemName, decimal? price)
    {
        var parameters = new Dictionary<string, object>
            {
                { AppEventParameterName.ContentID, itemName }
            };

        LogAppEvent("user_garage", (float)price, parameters);
    }

    public static void LogUpgradeMotorbikeEvent(string itemName, string itemUpgrade)
    {
        var parameters = new Dictionary<string, object>
            {
                { "Motorbike Name", itemName },
                { "Motorbike Upgrade", itemUpgrade }
            };

        LogAppEvent("motorbike_upgrade", null, parameters);
    }

    public static void LogWeaponEvent(string itemName, decimal? price)
    {
        var parameters = new Dictionary<string, object>
            {
                { AppEventParameterName.ContentID, itemName }
            };

        LogAppEvent("user_weapons", (float)price, parameters);
    }

    public static void LogUpgradeWeaponEvent(string itemName, string itemUpgrade)
    {
        var parameters = new Dictionary<string, object>
            {
                { "Weapon Name", itemName },
                { "Weapon Upgrade", itemUpgrade }
            };

        LogAppEvent("weapon_upgrade", null, parameters);
    }

    public static void LogEnemyTypeEvent(int level)
    {
        LogAppEvent("enemy_level", level);
    }

    public static void LogDailyRewardsEvent(string dailyReward)
    {
        var parameters = new Dictionary<string, object>
            {
                { "Daily Reward", dailyReward }
            };

        LogAppEvent("user_daily_rewards", null, parameters);
    }

    private static void LogAppEvent(string logEvent, float? valueToSum = null, Dictionary<string, object> parameters = null)
    {
        if (FB.IsInitialized)
        {
            FB.LogAppEvent(logEvent, valueToSum, parameters);
        }
    }

    public static void LogChargeMotorbikeEvent()
    {
        LogAppEvent("charge_motorbike");
    }

    public static void LogGameOverMultiplierEvent()
    {
        LogAppEvent("gameplay_x2");
    }
}
