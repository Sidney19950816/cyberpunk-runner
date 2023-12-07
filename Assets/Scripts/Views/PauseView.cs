using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts;
using Assets.Scripts.Managers;

public class PauseView : BaseBehaviour
{
    [SerializeField] private Button resumeButton;
    //[SerializeField] private Button restartButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;

    private float timeScale;
    //private float fixedDeltaTime;

    public void Show()
    {
        gameObject.SetActive(true);

        timeScale = Time.timeScale;
        //fixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = 0;

        resumeButton.onClick.AddListener(OnResumeClick);
        mainMenuButton.onClick.AddListener(OnRestartClick);
        settingsButton.onClick.AddListener(OnSettingsClick);

        resumeButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);
        mainMenuButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);
        settingsButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);
    }

    private void Hide()
    {
        resumeButton.onClick.RemoveListener(OnResumeClick);
        mainMenuButton.onClick.RemoveListener(OnRestartClick);
        settingsButton.onClick.RemoveListener(OnSettingsClick);

        resumeButton.onClick.RemoveListener(AudioManager.Instance.PlayUIButtonSound);
        mainMenuButton.onClick.RemoveListener(AudioManager.Instance.PlayUIButtonSound);
        settingsButton.onClick.RemoveListener(AudioManager.Instance.PlayUIButtonSound);

        Time.timeScale = timeScale;
        //Time.fixedDeltaTime = fixedDeltaTime;

        gameObject.SetActive(false);
    }

    private void OnResumeClick()
    {
        Hide();
    }

    private void OnRestartClick()
    {
        GameSceneManager.Instance.RestartScene();
    }

    private void OnSettingsClick()
    {
    }

    private void OnQuitGameClick()
    {
        Application.Quit();
    }
}
