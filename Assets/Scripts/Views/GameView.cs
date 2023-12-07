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

    private ArcadeBike arcadeBike;

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

        if (arcadeBike == null)
        {
            simulatedCurrencyItem.SetBalance(0);
            if (gameState.Bike != null)
            {
                gameState.Bike.Player.OnEarnedRokens += simulatedCurrencyItem.SetBalance;
                gameState.Bike.OnHealthUpdate += UpdateHealthValue;

                healthSlider.value = healthSlider.maxValue = gameState.Bike.BikeStats.MaxHealth;
                motorbikeFillImage.fillAmount = gameState.Bike.BikeStats.Health / gameState.Bike.BikeStats.MaxHealth;

                speedText.text = $"{(gameState.Bike.BikeStats.Health / gameState.Bike.BikeStats.MaxHealth).ToInt() * 100}";
                speedSlider.maxValue = gameState.Bike.BikeStats.TopSpeed * 3.6f;

                arcadeBike = gameState.Bike;
            }
        }
    }

    void UpdateHealthValue(float value)
    {
        healthText.text = (value / healthSlider.maxValue * 100).ToString("F0") + "%";
        motorbikeFillImage.fillAmount = value.ToInt() / healthSlider.maxValue;
    }

    private void Update()
    {
        if(arcadeBike != null)
        {
            speedSlider.value = arcadeBike.Rigidbody.velocity.magnitude * 3.6f;
            speedText.text = $"{(arcadeBike.Rigidbody.velocity.magnitude * 3.6f).ToInt()} KM/H";
        }
    }
}
