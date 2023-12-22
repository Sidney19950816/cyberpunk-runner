using Assets.Scripts.DailyRewards;
using Assets.Scripts.Managers;

public class MainMenuState : BaseState
{
    public MainMenuState()
    {
    }

    public override void OnStateEnter()
    {
        // Code to enter main menu state
        AudioManager.Instance.PlayMainMenuMusic();
    }

    public override void OnStateExit()
    {
        // Code to exit main menu state
    }

    public void OnStartButtonPressed()
    {
        //StateManager.SetState(new GameState());
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
        GameSceneManager.Instance.MotorbikeBattery.OnStartButtonPressed();
    }

    public bool GetStartButtonState()
    {
        return GameSceneManager.Instance.MotorbikeBattery.GetBatteryCount() > 0;
    }

    public void OnWeaponShopButtonPressed()
    {
        StateManager.SetState(new ItemShopState(ShopCategory.WEAPON));
    }

    public void OnBikeShopButtonPressed()
    {
        StateManager.SetState(new ItemShopState(ShopCategory.BIKE));
    }

    public void OnShopButtonPressed()
    {
        StateManager.SetState(new ShopState());
    }

    public void OnSettingsButtonPressed()
    {
        StateManager.SetState(new SettingsState());
    }

    public void OnDailyRewardsButtonPressed()
    {
        DailyRewardsManager.Instance.OnOpenEventButtonPressed();
    }

    private void SetDefaultBike()
    {
        Util.SetSelectedItemId("BIKE", "BIKE_0");
    }
}
