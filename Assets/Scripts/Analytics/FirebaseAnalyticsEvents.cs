using System.Collections;
using System.Collections.Generic;
using Firebase.Analytics;

public static class FirebaseAnalyticsEvents
{
    public static void LogUserSignUp(string userSignUp)
    {
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventSignUp, new Parameter(
                FirebaseAnalytics.UserPropertySignUpMethod, userSignUp));
    }

    public static void LogUserSetting(string userSetting)
    {
        FirebaseAnalytics.LogEvent("user_setting", "setting", userSetting);
    }

    public static void LogUserShop(string userShop, string price)
    {
        FirebaseAnalytics.LogEvent("user_shop", userShop, price);
    }

    public static void LogUserGarage(string userMotorbike, string price)
    {
        FirebaseAnalytics.LogEvent("user_garage", userMotorbike, price);
    }

    public static void LogUpgradeMotorbike(string userMotorbike, string userMotorbikeUpgrade)
    {
        FirebaseAnalytics.LogEvent("upgrade_motorbike",
                new Parameter("motorbike_name", userMotorbike),
                new Parameter("motorbike_upgrade", userMotorbikeUpgrade));
    }

    public static void LogUserWeapon(string userWeapon, string price)
    {
        FirebaseAnalytics.LogEvent("user_weapons", userWeapon, price);
    }

    public static void LogUpgradeWeapon(string userWeapon, string userWeaponUpgrade)
    {
        FirebaseAnalytics.LogEvent("upgrade_weapon",
                new Parameter("weapon_name", userWeapon),
                new Parameter("weapon_upgrade", userWeaponUpgrade));
    }

    public static void LogEnemyType(string enemyLevel)
    {
        Parameter enemyLevelParameter = new Parameter("level", enemyLevel);
        FirebaseAnalytics.LogEvent("enemy_level", enemyLevelParameter);
    }

    public static void LogDailyReward(string dailyReward)
    {
        FirebaseAnalytics.LogEvent("user_daily_rewards", "day_reward", dailyReward);
    }

    public static void LogChargeMotorbikeEvent()
    {
        FirebaseAnalytics.LogEvent("charge_motorbike");
    }

    public static void LogGameOverMultiplierEvent()
    {
        FirebaseAnalytics.LogEvent("gameplay_x2");
    }
}
