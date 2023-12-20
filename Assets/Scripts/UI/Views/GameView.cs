using UnityEngine.UI;
using UnityEngine;
using TMPro;
using Assets.Scripts;

public class GameView : BaseView
{
    [Header("CURRENCY")]
    [SerializeField] private SimulatedCurrencyItemView simulatedCurrencyItem;

    [Space, Header("HEALTH")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image motorbikeFillImage;

    [Space, Header("SPEED")]
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TextMeshProUGUI speedText;

    private Bike bike;

    public override void UpdateView(BaseState state)
    {
        // Update the UI based on the current game state
        if (state is GameState)
        {
            Show(state);
        }
        else
        {
            if(state is FightState)
            {
                simulatedCurrencyItem.transform.parent.SetParent(transform.parent);
            }
            else
            {
                simulatedCurrencyItem.transform.parent.SetParent(transform);
            }
            Hide();
        }
    }

    protected override void Show(BaseState state = null)
    {
        base.Show(state);

        GameState gameState = state as GameState;
        simulatedCurrencyItem.transform.parent.SetParent(transform);

        if (bike != null) return;

        simulatedCurrencyItem.SetBalance(0);

        if (gameState.Bike == null) return;

        gameState.Bike.Player.Score.OnUpdate += simulatedCurrencyItem.SetBalance;
        gameState.Bike.Health.HealthChanged += () => UpdateHealthValue(gameState.Bike.Health.Current);

        healthSlider.value = healthSlider.maxValue = gameState.Bike.Health.Max;
        motorbikeFillImage.fillAmount = gameState.Bike.Health.Current / gameState.Bike.Health.Max;

        speedText.text = $"{(gameState.Bike.Health.Current / gameState.Bike.Health.Max).ToInt() * 100}";
        speedSlider.maxValue = gameState.Bike.TopSpeed;

        bike = gameState.Bike;
    }

    void UpdateHealthValue(float value)
    {
        healthText.text = (value / healthSlider.maxValue * 100).ToString("F0") + "%";
        motorbikeFillImage.fillAmount = value.ToInt() / healthSlider.maxValue;
    }

    private void Update()
    {
        if(bike != null)
        {
            speedSlider.value = bike.Rigidbody.velocity.magnitude * 3.6f;
            speedText.text = $"{(bike.Rigidbody.velocity.magnitude * 3.6f).ToInt()} KM/H";
        }
    }
}
