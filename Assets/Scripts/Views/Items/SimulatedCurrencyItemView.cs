using UnityEngine;
using TMPro;

public class SimulatedCurrencyItemView : MonoBehaviour
{
    [SerializeField] private CurrencyType currencyType;
    [SerializeField] private TextMeshProUGUI balanceField;

    public CurrencyType CurrencyType { get { return currencyType; } }

    private int currentBalance;

    public void SetBalance(int balance)
    {
        currentBalance = balance;
        balanceField.text = currentBalance.ToString();
    }

    public int GetBalance()
    {
        return currentBalance;
    }
}
