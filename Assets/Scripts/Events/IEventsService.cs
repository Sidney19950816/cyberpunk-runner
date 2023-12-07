using System.Threading.Tasks;

namespace Assets.Scripts.Events
{
    public interface IEventsService
    {
        Task UserUidAsync(string userUid);
        Task UserSignUpAsync(UserSignUp userSignUp);

        Task UserSettingChangeAsync(UserSetting userSetting);

        Task UserShopAsync(UserShop userShop, decimal price);

        Task UserPopupCounterAsync(UserPopup userPopup);
        Task UserPopupPurchaseAsync(UserPopup userPopup, decimal price);

        Task UserGarageAsync(UserMotorbike userMotorbike, decimal? price = null);
        Task UpgradeMotorbikeAsync(UserMotorbike userMotorbike, UserMotorbikeUpgrade userMotorbikeUpgrade);

        Task UserWeaponAsync(UserWeapon userWeapon, decimal? price = null);
        Task UpgradeWeaponAsync(UserWeapon userWeapon, UserWeaponUpgrade userWeaponUpgrade);

        Task EnemyTypeAsync(int enemyLevel);

        Task UserCheckpointAsync(int count);

        Task UserDailyRewardsAsync(DailyReward dailyReward);

        Task ChargeMotorbikeAsync();
        Task GameOverMultiplierAsync();
    }
}