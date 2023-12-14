using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Events;
using Assets.Scripts.Managers;

public class SettingsView : BaseView
{
    #region LINKS
    private const string DISCORD_LINK = "https://discord.gg/aCZwZearvF";
    private const string FACEBOOK_LINK = "https://www.facebook.com/rokencity";
    private const string INSTAGRAM_LINK = "https://www.instagram.com/rokencity.game/";
    private const string YOUTUBE_LINK = "https://www.youtube.com/channel/UCZxdaJVVwe8GbGak8EwJXKQ";
    private const string WEBSITE_LINK = "https://rokencity.com";
    private const string PRIVACY_POLICY_LINK = "https://fooplix.com/privacy-policy";
    private const string TERMS_OF_USE_LINK = "https://fooplix.com/terms-of-service";
    #endregion

    [Space, Header("Effects and Quality")]
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle soundsToggle;
    [SerializeField] private Toggle vibrationToggle;
    [SerializeField] private Button qualityPreviousButton;
    [SerializeField] private Button qualityNextButton;

    [Space, Header("Connect")]
    [SerializeField] private Button platformConnectButton;
    [SerializeField] private Button facebookConnectButton;

    [Space, Header("Follow Us")]
    [SerializeField] private Button discordButton;
    [SerializeField] private Button facebookButton;
    [SerializeField] private Button instagramButton;
    [SerializeField] private Button youtubeButton;

    [Space, Header("Links")]
    [SerializeField] private Button websiteButton;
    [SerializeField] private Button privacyButton;
    [SerializeField] private Button termsButton;

    [Space, Header("Back Button")]
    [SerializeField] private Button backButton;

    [Space, Header("Images")]
    [SerializeField] private Image platformConnectImage;

    [Space, Header("Sprites")]
    [SerializeField] private Sprite googlePlaySprite;
    [SerializeField] private Sprite gameCenterSprite;

    [Space, Header("Texts")]
    [SerializeField] private Text appVersionText;

    private void Start()
    {
        musicToggle.isOn = PlayerPrefsUtil.GetMusicEnabled();
        soundsToggle.isOn = PlayerPrefsUtil.GetSoundsEnabled();
        vibrationToggle.isOn = PlayerPrefsUtil.GetVibrationEnabled();

        // TOGGLE LISTENERS
        musicToggle.onValueChanged.AddListener(OnMusicToggle);
        soundsToggle.onValueChanged.AddListener(OnSoundToggle);
        vibrationToggle.onValueChanged.AddListener(OnVibrationToggle);

        // BUTTON LISTENERS
        discordButton.onClick.AddListener(OnDiscordButtonClick);
        facebookButton.onClick.AddListener(OnFacebookButtonClick);
        instagramButton.onClick.AddListener(OnInstagramButtonClick);
        youtubeButton.onClick.AddListener(OnYoutubeButtonClick);
        websiteButton.onClick.AddListener(OnWebsiteButtonClick);
        privacyButton.onClick.AddListener(OnPrivacyPolicyButtonClick);
        termsButton.onClick.AddListener(OnTermsOfUseButtonClick);

        platformConnectButton.onClick.AddListener(OnPlatformConnectButtonClick);
        platformConnectImage.sprite = Application.platform == RuntimePlatform.Android ? googlePlaySprite : gameCenterSprite;

        appVersionText.text = $"App: {Application.version}";
    }

    public override void UpdateView(BaseState state)
    {
        if(state is SettingsState)
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

        musicToggle.isOn = PlayerPrefsUtil.GetMusicEnabled();
        soundsToggle.isOn = PlayerPrefsUtil.GetSoundsEnabled();
        vibrationToggle.isOn = PlayerPrefsUtil.GetVibrationEnabled();

        backButton.onClick.AddListener(OnBackButtonClick);
        backButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);
    }

    #region OnToggle
    private void OnMusicToggle(bool value)
    {
        PlayerPrefsUtil.SetMusicEnabled(value);
        AudioManager.Instance.PlayUIButtonSound();
        AudioManager.Instance.MuteMusic(value);

        UserSetting userSetting = value ? UserSetting.MusicOn : UserSetting.MusicOff;
        EventsService.UserSettingChangeAsync(userSetting);
    }

    private void OnSoundToggle(bool value)
    {
        PlayerPrefsUtil.SetSoundsEnabled(value);

        if (value)
            AudioManager.Instance.PlayUIButtonSound();

        UserSetting userSetting = value ? UserSetting.SoundOn : UserSetting.SoundOff;
        EventsService.UserSettingChangeAsync(userSetting);
    }

    private void OnVibrationToggle(bool value)
    {
        PlayerPrefsUtil.SetVibrationEnabled(value);
        AudioManager.Instance.PlayUIButtonSound();

        UserSetting userSetting = value ? UserSetting.VibrationOn : UserSetting.VibrationOff;
        EventsService.UserSettingChangeAsync(userSetting);
    }
    #endregion

    #region OnClick
    private void OnBackButtonClick()
    {
        StateManager.SetState(new MainMenuState());
        backButton.onClick.RemoveListener(OnBackButtonClick);
    }

    private void OnDiscordButtonClick()
    {
        Application.OpenURL(DISCORD_LINK);
        AudioManager.Instance.PlayUIButtonSound();
    }

    private void OnFacebookButtonClick()
    {
        Application.OpenURL(FACEBOOK_LINK);
        AudioManager.Instance.PlayUIButtonSound();
    }

    private void OnInstagramButtonClick()
    {
        Application.OpenURL(INSTAGRAM_LINK);
        AudioManager.Instance.PlayUIButtonSound();
    }

    private void OnYoutubeButtonClick()
    {
        Application.OpenURL(YOUTUBE_LINK);
        AudioManager.Instance.PlayUIButtonSound();
    }

    private void OnWebsiteButtonClick()
    {
        Application.OpenURL(WEBSITE_LINK);
        AudioManager.Instance.PlayUIButtonSound();
    }

    private void OnPrivacyPolicyButtonClick()
    {
        Application.OpenURL(PRIVACY_POLICY_LINK);
        AudioManager.Instance.PlayUIButtonSound();
    }

    private void OnTermsOfUseButtonClick()
    {
        Application.OpenURL(TERMS_OF_USE_LINK);
        AudioManager.Instance.PlayUIButtonSound();
    }

    private async void OnPlatformConnectButtonClick()
    {
        await GameSceneManager.Instance.Authentication.LinkPlayerAsync();
        await ServiceManager.InitializeServices();
        AudioManager.Instance.PlayUIButtonSound();
    }
    #endregion
}
