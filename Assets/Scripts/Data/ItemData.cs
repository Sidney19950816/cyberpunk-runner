using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string name;
    public string description;
    public float price;

    public void Upgrade(UpgradeData upgradeData, string playersInventoryItemId, Action callback)
    {
        if (upgradeData == null)
        {
            Debug.LogError($"Upgrade not found for {name}");
            return;
        }

        float maxValue = upgradeData.value <= upgradeData.maxValue ? upgradeData.maxValue : -upgradeData.maxValue;
        float maxUpgradeableValue = maxValue >= 0 ? upgradeData.maxUpgradeableValue : -upgradeData.maxUpgradeableValue;
        float value = maxValue >= 0 ? upgradeData.value : -upgradeData.value;

        if (value >= maxUpgradeableValue)
        {
            Debug.LogError($"{upgradeData.name} already at max value for {name}");
            return;
        }

        if (EconomyManager.Instance.GetPlayersRokenBalance() < upgradeData.upgradePrice)
        {
            Debug.LogError($"Not enough currency to upgrade {upgradeData.name} for {name}");
            return;
        }

        EconomyManager.Instance.DecrementCurrencyBalance(upgradeData.currencyType, upgradeData.upgradePrice);

        upgradeData.value += upgradeData.upgradeValue;
        //upgradeData.value = Mathf.Clamp(upgradeData.value, upgradeData.baseValue, upgradeData.maxValue);
        upgradeData.upgradePrice += (upgradeData.upgradePrice * 40 / 100);

        EconomyManager.Instance.UpdateInventoryItemAsync(playersInventoryItemId, this, callback);

        Debug.Log($"{upgradeData.name} upgraded to {upgradeData.value} for {name}");
    }
}
