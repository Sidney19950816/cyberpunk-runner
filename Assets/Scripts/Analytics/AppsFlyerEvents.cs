using System.Collections.Generic;
using AppsFlyerSDK;

public static class AppsFlyerEvents
{
    public static void LogUserSignUp(string userSignUp)
    {
        Dictionary<string, string> eventValues = new Dictionary<string, string>();
        eventValues.Add(AFInAppEvents.REGSITRATION_METHOD, userSignUp);
        AppsFlyer.sendEvent(AFInAppEvents.COMPLETE_REGISTRATION, eventValues);
    }

    public static void LogUserSetting(string userSetting)
    {
        Dictionary<string, string> eventValues = new Dictionary<string, string>();
        eventValues.Add("setting", userSetting);
        AppsFlyer.sendEvent("user_setting", eventValues);
    }

    public static void LogUserShop(string userShop, string price)
    {
        Dictionary<string, string> eventValues = new Dictionary<string, string>();
        eventValues.Add(AFInAppEvents.CONTENT_ID, userShop);
        eventValues.Add(AFInAppEvents.PRICE, price);
        AppsFlyer.sendEvent("user_shop", eventValues);
    }

    public static void LogUserGarage(string userMotorbike, string price)
    {
        Dictionary<string, string> eventValues = new Dictionary<string, string>();
        eventValues.Add(userMotorbike, price);
        AppsFlyer.sendEvent("user_garage", eventValues);
    }

    public static void LogUpgradeMotorbike(string userMotorbike, string userMotorbikeUpgrade)
    {
        Dictionary<string, string> eventValues = new Dictionary<string, string>();
        eventValues.Add("motorbike_name", userMotorbike);
        eventValues.Add("motorbike_upgrade", userMotorbikeUpgrade);
        AppsFlyer.sendEvent("upgrade_motorbike", eventValues);
    }

    public static void LogUserWeapon(string userWeapon, string price)
    {
        Dictionary<string, string> eventValues = new Dictionary<string, string>();
        eventValues.Add(userWeapon, price);
        AppsFlyer.sendEvent("user_weapons", eventValues);
    }

    public static void LogUpgradeWeapon(string userWeapon, string userWeaponUpgrade)
    {
        Dictionary<string, string> eventValues = new Dictionary<string, string>();
        eventValues.Add("weapon_name", userWeapon);
        eventValues.Add("weapon_upgrade", userWeaponUpgrade);
        AppsFlyer.sendEvent("upgrade_weapon", eventValues);
    }

    public static void LogEnemyType(string enemyLevel)
    {
        Dictionary<string, string> eventValues = new Dictionary<string, string>();
        eventValues.Add("level", enemyLevel);
        AppsFlyer.sendEvent("enemy_level", eventValues);
    }

    public static void LogDailyReward(string dailyReward)
    {
        Dictionary<string, string> eventValues = new Dictionary<string, string>();
        eventValues.Add("day_reward", dailyReward);
        AppsFlyer.sendEvent("user_daily_rewards", eventValues);
    }

    public static void LogChargeMotorbikeEvent()
    {
        AppsFlyer.sendEvent("charge_motorbike", null);
    }

    public static void LogGameOverMultiplierEvent()
    {
        AppsFlyer.sendEvent("gameplay_x2", null);
    }
}
