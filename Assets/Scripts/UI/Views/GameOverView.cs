using System.Collections;
using Assets.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameOverView : BaseView
{
    [SerializeField] private TextMeshProUGUI earnedRokensValueText;

    [Space, Header("COUNTDOWN")]
    [SerializeField] private Text countdownText;
    [SerializeField] private Image countdownImage;
    [SerializeField] private float countdownDuration = 5f;

    [Space, Header("BUTTONS")]
    [SerializeField] private Button homeButton;
    [SerializeField] private Button watchAdButton;
    [SerializeField] private Button skipButton;
    [Header("CURRENCY BUTTONS")]
    [SerializeField] private Button addRokenButton;
    [SerializeField] private Button addBiochipButton;

    private Coroutine countdownCoroutine;

    private ShopView shopView;

    private UnityAction startCountdownAction;

    private void Awake()
    {
        shopView = GameSceneManager.Instance.Canvas.GetComponentInChildren<ShopView>(true);

        if (shopView != null)
        {
            addRokenButton.onClick.AddListener(shopView.SnapToCurrency);
            addRokenButton.onClick.AddListener(StopCountdown);
            addRokenButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);

            addBiochipButton.onClick.AddListener(shopView.SnapToCurrency);
            addBiochipButton.onClick.AddListener(StopCountdown);
            addBiochipButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);

            //if (EconomyManager.Instance.GetPlayersBiochipBalance() < 5) // Test value
            //{
            //    biochipButton.onClick.AddListener(shopView.SnapToCurrency);
            //    biochipButton.onClick.AddListener(StopCountdown);
            //    biochipButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);
            //}
        }
    }

    public override void UpdateView(BaseState state)
    {
        // Update the UI based on the current game state
        if (state is GameOverState)
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

        GameOverState gameOverState = state as GameOverState;

        homeButton.onClick.AddListener(gameOverState.SetMainMenuState);
        homeButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);

        skipButton.onClick.AddListener(gameOverState.SetMainMenuState);
        skipButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);

        watchAdButton.onClick.AddListener(gameOverState.OnWatchAdButtonClick);
        watchAdButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);
        watchAdButton.onClick.AddListener(SendOnGameOverEvent);

        earnedRokensValueText.text = gameOverState.Bike.Player.Rokens.ToString();

        if (shopView != null)
        {
            startCountdownAction = () => StartCountdown(gameOverState);
            shopView.OnDisable += startCountdownAction;
        }

        StartCountdown(gameOverState);

        gameOverState.Bike.Rigidbody.velocity = Vector3.zero;
        gameOverState.Bike.enabled = false;
    }

    private void StartCountdown(GameOverState gameOverState)
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        countdownCoroutine = StartCoroutine(CountdownCoroutine(gameOverState));
    }

    private void StopCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        countdownCoroutine = null;
    }

    private IEnumerator CountdownCoroutine(GameOverState gameOverState)
    {
        float remainingTime = countdownDuration;

        while (remainingTime > 0f)
        {
            countdownText.text = remainingTime.ToString("F0");
            countdownImage.fillAmount = remainingTime / 5;

            yield return new WaitForSeconds(0.1f);

            remainingTime -= 0.1f;
        }

        countdownText.text = "0";
        countdownImage.fillAmount = 0;
        gameOverState.SetMainMenuState();

        countdownCoroutine = null;
    }

    protected override void Hide(BaseState state = null)
    {
        base.Hide(state);

        if (shopView != null)
            shopView.OnDisable -= startCountdownAction;
    }

    private async void SendOnGameOverEvent()
    {
        await EventsService.GameOverMultiplierAsync();
    }
}
