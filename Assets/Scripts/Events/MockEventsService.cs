using System.Threading.Tasks;
using UnityEngine;
using Firebase.Analytics;
using AppsFlyerSDK;
using GameAnalyticsSDK;

namespace Assets.Scripts.Events
{
    public sealed class MockEventsService : MonoBehaviour, IEventsService
    {
        public Task UserUidAsync(string userUid)
        {
            Debug.Log($"{nameof(UserUidAsync)}: {userUid}");

            FirebaseAnalytics.SetUserId(userUid);
            AppsFlyer.setCustomerUserId(userUid);
            GameAnalytics.SetCustomId(userUid);
            FacebookAppEvents.LogUserIDEvent(userUid);
            UnityAnalytics.LogUserIDEvent(userUid);

            return Task.CompletedTask;
        }

        public Task UserSignUpAsync(UserSignUp userSignUp)
        {
            Debug.Log($"{nameof(UserSignUpAsync)}: {userSignUp}");

            FacebookAppEvents.LogSignUpEvent(userSignUp.ToString());
            FirebaseAnalyticsEvents.LogUserSignUp(userSignUp.ToString());
            AppsFlyerEvents.LogUserSignUp(userSignUp.ToString());
            UnityAnalytics.LogUserSignUp(userSignUp.ToString());

            return Task.CompletedTask;
        }

        public Task UserSettingChangeAsync(UserSetting userSetting)
        {
            Debug.Log($"{nameof(UserSettingChangeAsync)}: {userSetting}");

            FacebookAppEvents.LogSettingEvent(userSetting.ToString());
            FirebaseAnalyticsEvents.LogUserSetting(userSetting.ToString());
            AppsFlyerEvents.LogUserSetting(userSetting.ToString());
            UnityAnalytics.LogUserSetting(userSetting.ToString());

            return Task.CompletedTask;
        }

        public Task UserShopAsync(UserShop userShop, decimal price)
        {
            Debug.Log($"{nameof(UserShopAsync)}: {userShop}, {price}");

            FacebookAppEvents.LogShopEvent(userShop.ToString(), price);
            FirebaseAnalyticsEvents.LogUserShop(userShop.ToString(), price.ToString());
            AppsFlyerEvents.LogUserShop(userShop.ToString(), price.ToString());
            UnityAnalytics.LogUserShop(userShop.ToString(), price.ToString());

            return Task.CompletedTask;
        }

        public Task UserPopupCounterAsync(UserPopup userPopup)
        {
            Debug.Log($"{nameof(UserPopupCounterAsync)}: {userPopup}");
            return Task.CompletedTask;
        }

        public Task UserPopupPurchaseAsync(UserPopup userPopup, decimal price)
        {
            Debug.Log($"{nameof(UserPopupPurchaseAsync)}: {userPopup}, {price}");
            return Task.CompletedTask;
        }

        public Task UserGarageAsync(UserMotorbike userMotorbike, decimal? price = null)
        {
            Debug.Log($"{nameof(UserGarageAsync)}: {userMotorbike}, {price}");

            FacebookAppEvents.LogGarageEvent(userMotorbike.ToString(), price);
            FirebaseAnalyticsEvents.LogUserGarage(userMotorbike.ToString(), price.ToString());
            AppsFlyerEvents.LogUserGarage(userMotorbike.ToString(), price.ToString());
            UnityAnalytics.LogUserGarage(userMotorbike.ToString(), price.ToString());

            return Task.CompletedTask;
        }

        public Task UpgradeMotorbikeAsync(UserMotorbike userMotorbike, UserMotorbikeUpgrade userMotorbikeUpgrade)
        {
            Debug.Log($"{nameof(UpgradeMotorbikeAsync)}: {userMotorbike}, {userMotorbikeUpgrade}");

            FacebookAppEvents.LogUpgradeMotorbikeEvent(userMotorbike.ToString(), userMotorbikeUpgrade.ToString());
            FirebaseAnalyticsEvents.LogUpgradeMotorbike(userMotorbike.ToString(), userMotorbikeUpgrade.ToString());
            AppsFlyerEvents.LogUpgradeMotorbike(userMotorbike.ToString(), userMotorbikeUpgrade.ToString());
            UnityAnalytics.LogUpgradeMotorbike(userMotorbike.ToString(), userMotorbikeUpgrade.ToString());

            return Task.CompletedTask;
        }

        public Task UserWeaponAsync(UserWeapon userWeapon, decimal? price = null)
        {
            Debug.Log($"{nameof(UserWeaponAsync)}: {userWeapon}, {price}");

            FacebookAppEvents.LogWeaponEvent(userWeapon.ToString(), price);
            FirebaseAnalyticsEvents.LogUserWeapon(userWeapon.ToString(), price.ToString());
            AppsFlyerEvents.LogUserWeapon(userWeapon.ToString(), price.ToString());
            UnityAnalytics.LogUserWeapon(userWeapon.ToString(), price.ToString());

            return Task.CompletedTask;
        }

        public Task UpgradeWeaponAsync(UserWeapon userWeapon, UserWeaponUpgrade userWeaponUpgrade)
        {
            Debug.Log($"{nameof(UpgradeWeaponAsync)}: {userWeapon}, {userWeaponUpgrade}");

            FacebookAppEvents.LogUpgradeWeaponEvent(userWeapon.ToString(), userWeaponUpgrade.ToString());
            FirebaseAnalyticsEvents.LogUpgradeWeapon(userWeapon.ToString(), userWeaponUpgrade.ToString());
            AppsFlyerEvents.LogUpgradeWeapon(userWeapon.ToString(), userWeaponUpgrade.ToString());
            UnityAnalytics.LogUpgradeWeapon(userWeapon.ToString(), userWeaponUpgrade.ToString());

            return Task.CompletedTask;
        }

        public Task EnemyTypeAsync(int enemyLevel)
        {
            Debug.Log($"{nameof(EnemyTypeAsync)}: {enemyLevel}");

            FacebookAppEvents.LogEnemyTypeEvent(enemyLevel);
            FirebaseAnalyticsEvents.LogEnemyType(enemyLevel.ToString());
            AppsFlyerEvents.LogEnemyType(enemyLevel.ToString());
            UnityAnalytics.LogEnemyType(enemyLevel.ToString());

            return Task.CompletedTask;
        }

        public Task UserCheckpointAsync(int count)
        {
            Debug.Log($"{nameof(UserCheckpointAsync)}: {count}");
            return Task.CompletedTask;
        }

        public Task UserDailyRewardsAsync(DailyReward dailyReward)
        {
            Debug.Log($"{nameof(UserDailyRewardsAsync)}: {dailyReward}");

            FacebookAppEvents.LogDailyRewardsEvent(dailyReward.ToString());
            FirebaseAnalyticsEvents.LogDailyReward(dailyReward.ToString());
            AppsFlyerEvents.LogDailyReward(dailyReward.ToString());
            UnityAnalytics.LogDailyReward(dailyReward.ToString());

            return Task.CompletedTask;
        }

        public Task ChargeMotorbikeAsync()
        {
            Debug.Log($"{nameof(ChargeMotorbikeAsync)}");

            FacebookAppEvents.LogChargeMotorbikeEvent();
            FirebaseAnalyticsEvents.LogChargeMotorbikeEvent();
            AppsFlyerEvents.LogChargeMotorbikeEvent();
            UnityAnalytics.LogChargeMotorbikeEvent();

            return Task.CompletedTask;
        }

        public Task GameOverMultiplierAsync()
        {
            Debug.Log($"{nameof(GameOverMultiplierAsync)}");

            FacebookAppEvents.LogGameOverMultiplierEvent();
            FirebaseAnalyticsEvents.LogGameOverMultiplierEvent();
            AppsFlyerEvents.LogGameOverMultiplierEvent();
            UnityAnalytics.LogGameOverMultiplierEvent();

            return Task.CompletedTask;
        }
    }
}