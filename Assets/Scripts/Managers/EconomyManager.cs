using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Unity.Services.Economy;
using Unity.Services.Economy.Model;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    #region Instance
    public static EconomyManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #endregion

    public async Task RefreshEconomyConfiguration()
    {
        // Calling GetCurrenciesAsync (or GetInventoryItemsAsync), in addition to returning the appropriate
        // Economy configurations, will update the cached configuration list, including any new Currency, 
        // Inventory Item, or Purchases that have been published since the last time the player's configuration
        // was cached.
        // 
        // This is important to do before hitting the Economy or Remote Config services for any other calls as
        // both use the cached data list.

        var getCurrenciesTask = EconomyService.Instance.Configuration.GetCurrenciesAsync();
        var getInventoryItemsTask = EconomyService.Instance.Configuration.GetInventoryItemsAsync();
        var getVirtualPurchasesTask = EconomyService.Instance.Configuration.GetVirtualPurchasesAsync();

        await Task.WhenAll(getCurrenciesTask, getInventoryItemsTask, getVirtualPurchasesTask);

        if (this == null)
            return;

        InventoryItemDefinitions = getInventoryItemsTask.Result;
        virtualPurchaseDefinitions = getVirtualPurchasesTask.Result;
    }

    #region Currency Balances
    public List<PlayerBalance> PlayerBalances { get; private set; }
    public Action OnCurrencyBalanceRefresh;

    public async Task RefreshCurrencyBalances()
    {
        GetBalancesResult balanceResult = null;

        try
        {
            balanceResult = await GetEconomyBalances();
        }
        catch (EconomyRateLimitedException e)
        {
            balanceResult = await Util.RetryEconomyFunction(GetEconomyBalances, e.RetryAfter);
        }
        catch (Exception e)
        {
            Debug.Log("Problem getting Economy currency balances:");
            Debug.LogException(e);
        }

        // Check that scene has not been unloaded while processing async wait to prevent throw.
        if (this == null)
            return;

        PlayerBalances = balanceResult?.Balances;

        OnCurrencyBalanceRefresh?.Invoke();
    }

    public long GetPlayersBalance(CurrencyType currencyType)
    {
        return PlayerBalances.Find(b => b.CurrencyId == currencyType.ToString())?.Balance ?? 0;
    }

    public long GetPlayersRokenBalance()
    {
        return PlayerBalances.Find(b => b.CurrencyId == UtilConstants.CURRENCY_ROKEN_ID)?.Balance ?? 0;
    }

    public long GetPlayersBiochipBalance()
    {
        return PlayerBalances.Find(b => b.CurrencyId == UtilConstants.CURRENCY_BIOCHIP_ID)?.Balance ?? 0;
    }

    public async void IncrementRokenBalance(int amount)
    {
        await EconomyService.Instance.PlayerBalances.IncrementBalanceAsync(UtilConstants.CURRENCY_ROKEN_ID, amount);
    }

    static Task<GetBalancesResult> GetEconomyBalances()
    {
        var options = new GetBalancesOptions { ItemsPerFetch = 100 };
        return EconomyService.Instance.PlayerBalances.GetBalancesAsync(options);
    }

    private async Task ListAllCurrencyIds()
    {
        try
        {
            List<CurrencyDefinition> currencies = await EconomyService.Instance.Configuration.GetCurrenciesAsync();

            List<string> currenciesIds = new List<string>();
            foreach (var currency in currencies)
            {
                currenciesIds.Add(currency.Id);
            }

            Debug.Log($"Currencies in config: {string.Join(", ", currenciesIds)}");
        }
        catch (EconomyException e)
        {
            Debug.LogError(e);
        }
    }

    private async Task UpdateCurrencyBalance(string currencyId, int newBalance)
    {
        try
        {
            PlayerBalance updatedBalance = await EconomyService.Instance.PlayerBalances.SetBalanceAsync(currencyId, newBalance);
            Debug.Log($"{updatedBalance.CurrencyId} set to {updatedBalance.Balance}");
        }
        catch (EconomyRateLimitedException e)
        {
            Debug.LogError($"{e} - Retry after {e.RetryAfter}");
        }
        catch (EconomyException e)
        {
            Debug.LogError(e);
        }
    }

    public async void IncrementCurrencyBalance(string currencyId, int amount)
    {
        try
        {
            await EconomyService.Instance.PlayerBalances.IncrementBalanceAsync(currencyId, amount);

            if (this == null) return;

            await RefreshCurrencyBalances();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async void DecrementCurrencyBalance(CurrencyType currencyType, int amount)
    {
        try
        {
            await EconomyService.Instance.PlayerBalances.DecrementBalanceAsync(currencyType.ToString(), amount);

            if (this == null) return;

            await RefreshCurrencyBalances();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async void GetCurrencyAmount(string currencyId)
    {
        GetBalancesResult balanceResult = await GetEconomyBalances();
        Debug.LogError(balanceResult.Balances.Where(b => b.CurrencyId == currencyId).FirstOrDefault().Balance);
    }

    #endregion

    #region Virtual Purchases
    const int k_EconomyPurchaseCostsNotMetStatusCode = 10504;

    // Dictionary of all Virtual Purchase transactions ids to lists of costs & rewards.
    public Dictionary<string, (List<ItemAndAmountData> costs, List<ItemAndAmountData> rewards)>
        virtualPurchaseTransactions
    { get; private set; }

    List<VirtualPurchaseDefinition> virtualPurchaseDefinitions;

    public void InitializeVirtualPurchaseLookup()
    {
        if (virtualPurchaseDefinitions == null)
        {
            return;
        }

        virtualPurchaseTransactions = new Dictionary<string,
            (List<ItemAndAmountData> costs, List<ItemAndAmountData> rewards)>();

        foreach (var virtualPurchaseDefinition in virtualPurchaseDefinitions)
        {
            var costs = ParseEconomyItems(virtualPurchaseDefinition.Costs);
            var rewards = ParseEconomyItems(virtualPurchaseDefinition.Rewards);

            virtualPurchaseTransactions[virtualPurchaseDefinition.Id] = (costs, rewards);
        }
    }

    List<ItemAndAmountData> ParseEconomyItems(List<PurchaseItemQuantity> itemQuantities)
    {
        var itemsAndAmountsSpec = new List<ItemAndAmountData>();

        foreach (var itemQuantity in itemQuantities)
        {
            var id = itemQuantity.Item.GetReferencedConfigurationItem().Id;
            var type = itemQuantity.Item.GetReferencedConfigurationItem().Type;
            var customData = itemQuantity.Item.GetReferencedConfigurationItem().CustomDataDeserializable;

            itemsAndAmountsSpec.Add(new ItemAndAmountData(id, type, itemQuantity.Amount, customData));
        }

        return itemsAndAmountsSpec;
    }

    public async Task<MakeVirtualPurchaseResult> MakeVirtualPurchaseAsync(string virtualPurchaseId)
    {
        try
        {
            return await EconomyService.Instance.Purchases.MakeVirtualPurchaseAsync(virtualPurchaseId);
        }
        catch (EconomyException e)
        when (e.ErrorCode == k_EconomyPurchaseCostsNotMetStatusCode)
        {
            // Rethrow purchase-cost-not-met exception to be handled by shops manager.
            throw;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return default;
        }
    }

    #endregion

    #region Inventory Items

    public List<InventoryItemDefinition> InventoryItemDefinitions { get; private set; }
    public List<PlayersInventoryItem> PlayersInventoryItems { get; private set; }

    public async Task RefreshInventory()
    {
        GetInventoryResult inventoryResult = null;

        try
        {
            inventoryResult = await GetEconomyPlayerInventory();
        }
        catch (EconomyRateLimitedException e)
        {
            inventoryResult = await Util.RetryEconomyFunction(GetEconomyPlayerInventory, e.RetryAfter);
        }
        catch (Exception e)
        {
            Debug.Log("Problem getting Economy inventory items:");
            Debug.LogException(e);
        }

        if (this == null)
            return;

        PlayersInventoryItems = inventoryResult.PlayersInventoryItems;
    }

    static Task<GetInventoryResult> GetEconomyPlayerInventory()
    {
        var options = new GetInventoryOptions { ItemsPerFetch = 100 };
        return EconomyService.Instance.PlayerInventory.GetInventoryAsync(options);
    }

    public async void AddInventoryItemAsync(string inventoryItemId) // Test
    {
        await EconomyService.Instance.PlayerInventory.AddInventoryItemAsync(inventoryItemId);
        await RefreshInventory();
    }

    public async void UpdateInventoryItemAsync(string playersInventoryItemId, object instanceData, Action callback = null)
    {
        await EconomyService.Instance.PlayerInventory.UpdatePlayersInventoryItemAsync(playersInventoryItemId, instanceData);
        await RefreshInventory();
        callback?.Invoke();
    }

    public async void DeleteInventoryItemAsync(string playersInventoryItemId) // Test
    {
        await EconomyService.Instance.PlayerInventory.DeletePlayersInventoryItemAsync(playersInventoryItemId);
        await RefreshInventory();
    }

    #endregion

    public async void OnPurchaseClicked(InventoryItemDefinition inventoryItem, Action callback = null)
    {
        await OnPurchaseClickedAsync(inventoryItem, callback);
    }

    public async Task OnPurchaseClickedAsync(InventoryItemDefinition inventoryItem, Action callback = null)
    {
        string inventoryItemId = $"VIRTUAL_SHOP_{inventoryItem.Id}";

        try
        {
            await HandlePurchaseAsync(inventoryItemId);

            if (this == null) return;

            var playersInventoryItem = PlayersInventoryItems.Find(i => i.InventoryItemId == inventoryItem.Id);
            if (playersInventoryItem == null) return;

            UpdateInventoryItemAsync(playersInventoryItem.PlayersInventoryItemId, inventoryItem.CustomDataDeserializable, callback);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private async Task HandlePurchaseAsync(string inventoryItemId)
    {
        if (virtualPurchaseTransactions == null)
            InitializeVirtualPurchaseLookup();

        foreach (ItemAndAmountData reward in virtualPurchaseTransactions[inventoryItemId].rewards)
        {
            if (reward.type == ItemAndAmountData.INVENTORY_ITEM)
            {
                if (PlayersInventoryItems.FirstOrDefault(i => i.InventoryItemId == reward.id) != null)
                    return;
            }
        }

        var result = await MakeVirtualPurchaseAsync(inventoryItemId);
        if (this == null) return;

        await RefreshCurrencyBalances();
        await RefreshInventory();
    }
}
