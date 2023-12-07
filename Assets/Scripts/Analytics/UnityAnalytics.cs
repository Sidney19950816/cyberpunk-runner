using System.Collections.Generic;
using Unity.Services.Analytics;

public static class UnityAnalytics
{
    public static void LogUserIDEvent(string userId)
    {
        var parameters = new Dictionary<string, object> {
            { "userId", userId }
        };

        AnalyticsService.Instance.CustomData("user_id", parameters);
    }

    public static void LogUserSignUp(string userSignUp)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "userSignUp", userSignUp}
        };

        AnalyticsService.Instance.CustomData("user_signup", parameters);
    }

    public static void LogUserSetting(string userSetting)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "setting", userSetting}
        };

        AnalyticsService.Instance.CustomData("user_setting", parameters);
    }

    public static void LogUserShop(string userShop, string price)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "userShop", userShop},
            { "price", price}
        };

        AnalyticsService.Instance.CustomData("user_shop", parameters);
    }

    public static void LogUserGarage(string userMotorbike, string price)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "userMotorbike", userMotorbike},
            { "price", price}
        };

        AnalyticsService.Instance.CustomData("user_garage", parameters);
    }

    public static void LogUpgradeMotorbike(string userMotorbike, string userMotorbikeUpgrade)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "motorbike_name", userMotorbike},
            { "motorbike_upgrade", userMotorbikeUpgrade}
        };

        AnalyticsService.Instance.CustomData("upgrade_motorbike", parameters);
    }

    public static void LogUserWeapon(string userWeapon, string price)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "userWeapon", userWeapon},
            { "price", price}
        };

        AnalyticsService.Instance.CustomData("user_weapons", parameters);
    }

    public static void LogUpgradeWeapon(string userWeapon, string userWeaponUpgrade)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "weapon_name", userWeapon},
            { "weapon_upgrade", userWeaponUpgrade}
        };

        AnalyticsService.Instance.CustomData("upgrade_weapon", parameters);
    }

    public static void LogEnemyType(string enemyLevel)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "level", enemyLevel}
        };

        AnalyticsService.Instance.CustomData("enemy_level", parameters);
    }

    public static void LogDailyReward(string dailyReward)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "day_reward", dailyReward}
        };

        AnalyticsService.Instance.CustomData("user_daily_rewards", parameters);
    }

    public static void LogChargeMotorbikeEvent()
    {
        AnalyticsService.Instance.CustomData("charge_motorbike");
    }

    public static void LogGameOverMultiplierEvent()
    {
        AnalyticsService.Instance.CustomData("gameplay_x2");
    }
}
