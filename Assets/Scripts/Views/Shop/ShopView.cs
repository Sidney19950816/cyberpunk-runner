using Assets.Scripts.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.Services.Economy.Model;
using System.Collections;
using Assets.Scripts.Events;
using AppsFlyerSDK;

public class ShopView : BaseView, IDetailedStoreListener
{
    [Header("BUTTONS")]
    [SerializeField] private Button backButton;

    [Space, Header("SCROLL RECT")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private RectTransform nonConsumablePanel;
    [SerializeField] private RectTransform consumablePanel;

    [Space, Header("PREFABS")]
    [SerializeField] private ShopPackView shopPackPrefab;
    [SerializeField] private ShopCurrencyView shopCurrencyPrefab;

    public UnityAction OnDisable;

    private IStoreController controller;
    private IExtensionProvider extensions;

    private Action<bool> OnPurchaseCompleted;

    private void Awake()
    {
        float scale = (float)Screen.width / Screen.height;
        if (scale > 2)
            contentPanel.localScale = Vector3.one * (Screen.width / Screen.height + 1 - scale);

        ResourceRequest operation = Resources.LoadAsync<TextAsset>("IAPProductCatalog");
        operation.completed += HandleIAPCatalogLoaded;
    }

    /// <summary>
    /// Called when Unity IAP is ready to make purchases.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.controller = controller;
        this.extensions = extensions;

        List<Product> sortedNonConsumables = controller.products.all
            .Where(i => i.definition.type == ProductType.NonConsumable)
            .OrderBy(item => item.metadata.localizedPrice).ToList();
        List<Product> sortedConsumables = controller.products.all
            .Where(i => i.definition.type == ProductType.Consumable)
            .OrderBy(item => item.metadata.localizedPrice).ToList();

        foreach (Product nonConsumable in sortedNonConsumables)
        {
            bool shouldSkip = nonConsumable.definition.payouts
                .Where(p => p.type == PayoutType.Item)
                    .Any(item => EconomyManager.Instance.PlayersInventoryItems
                        .Any(i => i.InventoryItemId == item.subtype));

            if (shouldSkip)
                continue;

            ShopPackView shopPackView = Instantiate(shopPackPrefab, nonConsumablePanel.transform);
            shopPackView.OnPurchase += HandlePurchase;
            shopPackView.Init(nonConsumable);
        }

        foreach (Product consumable in sortedConsumables)
        {
            ShopCurrencyView shopPackView = Instantiate(shopCurrencyPrefab, consumablePanel.transform);
            shopPackView.OnPurchase += HandlePurchase;
            shopPackView.Init(consumable);
        }

        StartCoroutine(ForceRebuildLayoutImmediate());
    }

    /// <summary>
    /// Called when Unity IAP encounters an unrecoverable initialization error.
    ///
    /// Note that this will not be called if Internet is unavailable; Unity IAP
    /// will attempt initialization until it becomes available.
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"Error initializing IAP because of {error}." +
            $"\r\nShow a message to the player depending on the error.");
    }

    private void HandlePurchase(Product product, Action<bool> OnPurchaseCompleted)
    {
        this.OnPurchaseCompleted = OnPurchaseCompleted;
        controller.InitiatePurchase(product);
    }

    private void HandleIAPCatalogLoaded(AsyncOperation operation)
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
    /// Called when a purchase completes.
    ///
    /// May be called at any time after OnInitialized().
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        Debug.Log($"Successfully purchased {e.purchasedProduct.definition. id}");
        OnPurchaseCompleted?.Invoke(true);
        OnPurchaseCompleted = null;

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
                if(payout.type == PayoutType.Currency)
                {
                    EconomyManager.Instance.IncrementCurrencyBalance(payout.subtype, (int)payout.quantity);
                }
                else if(payout.type == PayoutType.Item)
                {
                    InventoryItemDefinition inventoryItem = EconomyManager.Instance.InventoryItemDefinitions.Find(i => i.Id == payout.subtype);
                    if (inventoryItem != null)
                        EconomyManager.Instance.OnPurchaseClicked(inventoryItem);
                }
            }
        }

        ShopData shopData = JsonUtility.FromJson<ShopData>(e.purchasedProduct.definition.payout.data);
        UserShop userShop = shopData.enumId;
        EventsService.UserShopAsync(userShop, e.purchasedProduct.metadata.localizedPrice);
        StartCoroutine(ForceRebuildLayoutImmediate());
        return PurchaseProcessingResult.Complete;
    }

    IEnumerator ForceRebuildLayoutImmediate()
    {
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel);
    }

    /// <summary>
    /// Called when a purchase fails.
    /// IStoreListener.OnPurchaseFailed is deprecated,
    /// use IDetailedStoreListener.OnPurchaseFailed instead.
    /// </summary>
    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        Debug.Log($"Failed to purchase {i.definition.id} because {p}");
        OnPurchaseCompleted?.Invoke(false);
        OnPurchaseCompleted = null;
    }

    /// <summary>
    /// Called when a purchase fails.
    /// </summary>
    public void OnPurchaseFailed(Product i, PurchaseFailureDescription p)
    {
        Debug.Log($"Failed to purchase {i.definition.id} because {p}");
        OnPurchaseCompleted?.Invoke(false);
        OnPurchaseCompleted = null;
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        throw new System.NotImplementedException();
    }

    private void OnEnable()
    {
        backButton.onClick.AddListener(() => Hide());
        backButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);
    }

    public override void UpdateView(BaseState state)
    {
        if (state is ShopState)
        {
            Show(state);
        }
        else
        {
            Hide();
        }
    }

    protected override void Show(BaseState state = null)
    {
        base.Show(state);

        ShopState shopState = state as ShopState;

        if (shopState != null)
        {
            contentPanel.anchoredPosition = Vector2.zero;
            backButton.onClick.AddListener(shopState.OnBackButtonPressed);
        }
    }

    protected override void Hide(BaseState state = null)
    {
        base.Hide(state);

        backButton.onClick.RemoveAllListeners();
        OnDisable?.Invoke();
    }

    public void SnapToCurrency()
    {
        Show();
        Canvas.ForceUpdateCanvases();

        contentPanel.anchoredPosition =
                (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
                - (Vector2)scrollRect.transform.InverseTransformPoint(consumablePanel.position) + new Vector2(200, 0);
    }

    public class ShopData
    {
        public UserShop enumId;
    }    
}
