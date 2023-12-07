using Assets.Scripts.DailyRewards;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class ServiceManager
{
    public static async Task InitializeServices()
    {
        try
        {
            await Task.WhenAll(
                RemoteConfigManager.Instance.FetchConfigs(),
                EconomyManager.Instance.RefreshEconomyConfiguration(),
                EconomyManager.Instance.RefreshCurrencyBalances(),
                EconomyManager.Instance.RefreshInventory(),
                DailyRewardsManager.Instance.RefreshStatus()
            );
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error initializing services: {ex}");
        }
    }
}