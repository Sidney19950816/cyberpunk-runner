using UnityEngine;
using UnityEngine.Purchasing;
using System;
using UnityEngine.Purchasing.Extension;
using System.Collections.Generic;
using System.Linq;
using AppsFlyerSDK;
using Unity.Services.Economy.Model;
using Assets.Scripts;

public class IAPManager : BaseBehaviour, IDetailedStoreListener
{
    private const string IAP_PRODUCT_CATALOG = "IAPProductCatalog";

    private IStoreController _controller;
    private IExtensionProvider _extensions;

    private Product[] _sortedNonConsumables;
    private Product[] _sortedConsumables;

    public Product[] SortedNonConsumables => _sortedNonConsumables;
    public Product[] SortedConsumables => _sortedConsumables;

    public event Action<bool> OnPurchaseCompleted;
    public event Action OnInitializeCompleted;

    public void InitializeIAP()
    {
        Debug.Log("InitializeIAP");
        if (IsInitialized()) return;

        ResourceRequest operation = Resources.LoadAsync<TextAsset>(IAP_PRODUCT_CATALOG);
        operation.completed += InitializePurchasing;
    }

    private void OnDestroy()
    {
        OnPurchaseCompleted = null;
    }

    private void InitializePurchasing(AsyncOperation operation)
    {
        ResourceRequest request = operation as ResourceRequest;

        Debug.Log($"Loaded Asset: {request.asset}");
        ProductCatalog catalog = JsonUtility.FromJson<ProductCatalog>((request.asset as TextAsset).text);
        Debug.Log($"Loaded Catalog with {catalog.allProducts.Count} items");

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
        StandardPurchasingModule.Instance().useFakeStoreAlways = true;
#endif

#if UNITY_ANDROID
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(
            StandardPurchasingModule.Instance(AppStore.GooglePlay)
        );
#elif UNITY_IOS
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(
            StandardPurchasingModule.Instance(AppStore.AppleAppStore)
        );
#else
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(
            StandardPurchasingModule.Instance(AppStore.NotSpecified)
        );
#endif

        foreach (ProductCatalogItem item in catalog.allProducts)
        {
            IEnumerable<PayoutDefinition> payoutDefinitions = item.Payouts.Select(p => new PayoutDefinition(
                (PayoutType)p.type,
                p.subtype,
                p.quantity,
                p.data
            ));

            builder.AddProduct(item.id, item.type, new IDs
            {
                {item.id, GooglePlay.Name},
                {item.id, MacAppStore.Name}
            }, payoutDefinitions);
        }

        UnityPurchasing.Initialize(this, builder);
    }

    /// <summary>
    /// Called when Unity IAP is ready to make purchases.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _controller = controller;
        _extensions = extensions;

        _sortedNonConsumables = controller.products.all
            .Where(i => i.definition.type == ProductType.NonConsumable)
            .OrderBy(item => item.metadata.localizedPrice).ToArray();

        _sortedConsumables = controller.products.all
            .Where(i => i.definition.type == ProductType.Consumable)
            .OrderBy(item => item.metadata.localizedPrice).ToArray();

        OnInitializeCompleted?.Invoke();
    }

    private bool IsInitialized()
    {
        return _controller != null && _extensions != null;
    }

    public void HandlePurchase(Product product, Action<bool> OnPurchaseCompleted)
    {
        this.OnPurchaseCompleted = OnPurchaseCompleted;
        _controller.InitiatePurchase(product);
    }

    /// <summary>
    /// Called when a purchase completes.
    ///
    /// May be called at any time after OnInitialized().
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        Debug.Log($"Successfully purchased: {e.purchasedProduct.definition.id}");
        OnPurchaseCompleted?.Invoke(true);

        Product product = e.purchasedProduct;
        GameAnalyticsSDK.GameAnalytics.NewBusinessEvent(product.metadata.isoCurrencyCode,
            (int)Math.Ceiling(product.metadata.localizedPrice) * 100, product.definition.type.ToString(),
            product.definition.id, product.metadata.localizedTitle);
        // localizedPrice is multiplied by 100, because NewBusinessEvent amount is in Cents

        Dictionary<string, string> eventValues = new Dictionary<string, string>();
        eventValues.Add(AFInAppEvents.CURRENCY, product.metadata.isoCurrencyCode);
        eventValues.Add(AFInAppEvents.REVENUE, product.metadata.localizedPrice.ToString());
        eventValues.Add(AFInAppEvents.CONTENT_ID, product.definition.id);
        eventValues.Add(AFInAppEvents.CONTENT_TYPE, product.definition.type.ToString());
        eventValues.Add(AFInAppEvents.QUANTITY, product.definition.payouts.Count().ToString());
        AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, eventValues);

        if (e.purchasedProduct.definition.payouts != null)
        {
            Debug.Log("Purchase complete, paying out based on defined payouts");
            foreach (var payout in e.purchasedProduct.definition.payouts)
            {
                Debug.Log(string.Format("Granting {0} {1} {2} {3}", payout.quantity, payout.typeString, payout.subtype, payout.data));
                if (payout.type == PayoutType.Currency)
                {
                    EconomyManager.Instance.IncrementCurrencyBalance(payout.subtype, (int)payout.quantity);
                }
                else if (payout.type == PayoutType.Item)
                {
                    InventoryItemDefinition inventoryItem = EconomyManager.Instance.InventoryItemDefinitions.Find(i => i.Id == payout.subtype);
                    if (inventoryItem != null)
                        EconomyManager.Instance.OnPurchaseClicked(inventoryItem);
                }
            }
        }

        ShopData shopData = JsonUtility.FromJson<ShopData>(e.purchasedProduct.definition.payout.data);
        EventsService.UserShopAsync(shopData.enumId, e.purchasedProduct.metadata.localizedPrice);
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        Debug.LogError($"Purchase of {i.definition.id} failed due to: {p}");
        OnPurchaseCompleted?.Invoke(false);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogError($"Purchase of {product.definition.id} failed due to: {failureDescription.message}");
        OnPurchaseCompleted?.Invoke(false);
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"Error initializing IAP because of {error}." +
            $"\r\nShow a message to the player depending on the error.");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogError($"Error initializing IAP because of {error}." +
            $"\r\nShow a message to the player depending on the error.");
    }
}
