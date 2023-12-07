using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Managers;

public class MainMenuView : BaseView
{
    [Header("BUTTONS")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button weaponShopButton;
    [SerializeField] private Button bikeShopButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button dailyRewardsButton;

    [Space, Header("AUDIO CLIPS")]
    [SerializeField] private AudioClip goToGarageAudio;
    [SerializeField] private AudioClip goToWeaponsAudio;

    public override void UpdateView(BaseState state)
    {
        // Update the UI based on the current game state
        if (state is MainMenuState)
        {
            Show(state);
        }
        else
        {
            Hide();
        }
    }

    protected override void Show(BaseState state = null)
    {
        base.Show(state);

        MainMenuState mainMenuState = state as MainMenuState;

        startGameButton.interactable = mainMenuState.GetStartButtonState();
        startGameButton.onClick.AddListener(mainMenuState.OnStartButtonPressed);
        startGameButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);
        startGameButton.onClick.AddListener(AudioManager.Instance.PlayGameplayMusic);

        bikeShopButton.onClick.AddListener(mainMenuState.OnBikeShopButtonPressed);
        bikeShopButton.onClick.AddListener( () => AudioManager.Instance.PlaySound(goToGarageAudio));

        weaponShopButton.onClick.AddListener(mainMenuState.OnWeaponShopButtonPressed);
        weaponShopButton.onClick.AddListener(() => AudioManager.Instance.PlaySound(goToWeaponsAudio));

        shopButton.onClick.AddListener(mainMenuState.OnShopButtonPressed);
        shopButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);

        settingsButton.onClick.AddListener(mainMenuState.OnSettingsButtonPressed);
        settingsButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);

        dailyRewardsButton.onClick.AddListener(mainMenuState.OnDailyRewardsButtonPressed);
        dailyRewardsButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);
    }

    protected override void Hide(BaseState state = null)
    {
        base.Hide(state);

        startGameButton.onClick.RemoveAllListeners();
        bikeShopButton.onClick.RemoveAllListeners();
        weaponShopButton.onClick.RemoveAllListeners();
        shopButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        dailyRewardsButton.onClick.RemoveAllListeners();
    }

    public void UpdateStatus(MotorbikeBatteryEventManager eventManager)
    {
        startGameButton.interactable = eventManager.batteryCount > 0;
    }
}
