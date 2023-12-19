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

public class ShopView : BaseView
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

    private IAPManager _iapManager;

    private void Start()
    {
        float scale = (float)Screen.width / Screen.height;
        if (scale > 2)
            contentPanel.localScale = Vector3.one * (Screen.width / Screen.height + 1 - scale);

        _iapManager = GetComponent<IAPManager>()
            .With(i => i.InitializeIAP())
            .With(i => i.OnInitializeCompleted += HandleIAPInitialization)
            .With(i => i.OnPurchaseCompleted += 
            (bool completed) => StartCoroutine(ForceRebuildLayoutImmediate()));
    }

    private void HandleIAPInitialization()
    {
        Debug.Log("HandleIAPInitialization");
        foreach (Product nonConsumable in _iapManager.SortedNonConsumables)
        {
            bool shouldSkip = nonConsumable.definition.payouts
                .Where(p => p.type == PayoutType.Item)
                    .Any(item => EconomyManager.Instance.PlayersInventoryItems
                        .Any(i => i.InventoryItemId == item.subtype));

            if (shouldSkip)
                continue;

            ShopPackView shopPackView = Instantiate(shopPackPrefab, nonConsumablePanel.transform);
            shopPackView.OnPurchase += _iapManager.HandlePurchase;
            shopPackView.Init(nonConsumable);
        }

        foreach (Product consumable in _iapManager.SortedConsumables)
        {
            ShopCurrencyView shopPackView = Instantiate(shopCurrencyPrefab, consumablePanel.transform);
            shopPackView.OnPurchase += _iapManager.HandlePurchase;
            shopPackView.Init(consumable);
        }

        StartCoroutine(ForceRebuildLayoutImmediate());
    }

    IEnumerator ForceRebuildLayoutImmediate()
    {
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel);
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
}
