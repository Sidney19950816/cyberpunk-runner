using UnityEngine;
using TMPro;

public class SimulatedCurrencyItemView : MonoBehaviour
{
    [SerializeField] private CurrencyType currencyType;
    [SerializeField] private TextMeshProUGUI balanceField;

    public CurrencyType CurrencyType { get { return currencyType; } }

    private long currentBalance;

    public void SetBalance(long balance)
    {
        currentBalance += balance;
        balanceField.text = currentBalance.ToString();
    }

    public long GetBalance()
    {
        return currentBalance;
    }
}
