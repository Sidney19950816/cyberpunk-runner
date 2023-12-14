using UnityEngine;
using TMPro;

public class CurrencyItemView : MonoBehaviour
{
    [SerializeField] private CurrencyType currencyType;
    [SerializeField] private TextMeshProUGUI balanceField;

    public CurrencyType CurrencyType { get { return currencyType; } }

    public void SetBalance(long balance)
    {
        balanceField.text = balance.ToString();
    }
}
