using System;
using Unity.Services.Economy.Model;
using UnityEngine;

public class CurrencyHudView : MonoBehaviour
{
    [SerializeField] private CurrencyItemView[] _currencyItemViews;

    private void Awake()
    {
        if (_currencyItemViews == null)
            _currencyItemViews = GetComponentsInChildren<CurrencyItemView>();

        UpdateCurrencyBalances();

        EconomyManager.Instance.OnCurrencyBalanceRefresh += UpdateCurrencyBalances;
    }

    public void SetBalance(CurrencyType currencyType, long balance)
    {
        foreach (var currencyItemView in _currencyItemViews)
        {
            if (currencyType == currencyItemView.CurrencyType)
            {
                currencyItemView.SetBalance(balance);
            }
        }
    }

    public void ClearBalances()
    {
        foreach (var currencyItemView in _currencyItemViews)
        {
            currencyItemView.SetBalance(0);
        }
    }

    public CurrencyItemView GetCurrencyItemView(CurrencyType currencyType)
    {
        foreach (var view in _currencyItemViews)
        {
            if (currencyType == view.CurrencyType)
            {
                return view;
            }
        }

        return default;
    }

    private async void UpdateCurrencyBalances()
    {
        if (EconomyManager.Instance.PlayerBalances == null)
        {
            await EconomyManager.Instance.RefreshCurrencyBalances();
        }

        foreach (PlayerBalance balance in EconomyManager.Instance.PlayerBalances)
        {
            CurrencyType currencyType;
            if (Enum.TryParse(balance.CurrencyId, out currencyType))
            {
                SetBalance(currencyType, balance.Balance);
            }
            else
            {
                Debug.LogError("Invalid currency type: " + balance.CurrencyId);
            }
        }
    }
}
