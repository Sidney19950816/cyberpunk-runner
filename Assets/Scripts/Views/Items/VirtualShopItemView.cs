using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Unity.Services.Economy.Model;
using System;
using TMPro;
using Assets.Scripts.Managers;

public class VirtualShopItemView : BaseView
{
    [Header("BUTTONS")]
    [SerializeField] private Button previousItemButton;
    [SerializeField] private Button nextItemButton;
    [SerializeField] private Button buyItemButton;
    [SerializeField] private Button selectItemButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button buyIAPButton;

    [Space, Header("UPGRADES")]
    [SerializeField] private Transform attributesParent;
    [SerializeField] private AttributeItemView attributeItemView;
    [SerializeField] private Image lockIcon;
    [SerializeField] private Transform upgradeButtonsLayout;
    [SerializeField] private Button upgradeButton;
    //TODO: Add Full Upgrade Button

    [Space, Header("CURRENCY HUD")]
    [SerializeField] private CurrencyHudView currencyHudView;

    [Space, Header("Sprite Renderers")]
    [SerializeField] private Transform itemsParent;
    [SerializeField] private Transform mainBackground;

    [Space, Header("TEXTS")]
    [SerializeField] private Text shopCategoryText;
    [SerializeField] private Text itemNameText;
    [SerializeField] private TextMeshProUGUI buyButtonPriceText;
    [SerializeField] private TextMeshProUGUI buyIAPButtonPriceText;
    [SerializeField] private Text selectItemButtonText;

    private List<InventoryItemDefinition> inventoryItems = new List<InventoryItemDefinition>();
    private Dictionary<Enum, AttributeItemView> upgradeItems = new Dictionary<Enum, AttributeItemView>();
    private List<GameObject> itemObjects = new List<GameObject>();

    private int currentIndex;

    public void Initialize(ShopCategory shopCategory)
    {
        inventoryItems = EconomyManager.Instance.InventoryItemDefinitions.FindAll(i => i.Id.Contains(shopCategory.ToString()));

        if (inventoryItems == null) return;

        InitializeItemObjects(shopCategory.ToString());
        UpdateInventoryItem(inventoryItems.FindIndex(i => i.Id == Util.GetSelectedItemId(shopCategory.ToString())), shopCategory);
        shopCategoryText.text = $"{shopCategory}S";
        mainBackground.position = Camera.main.ScreenToWorldPoint(transform.position);
        mainBackground.localPosition = new Vector3(mainBackground.localPosition.x, mainBackground.localPosition.y - 3, 7);

        previousItemButton.onClick.AddListener(() => UpdateInventoryItem(-1, shopCategory));
        previousItemButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);

        nextItemButton.onClick.AddListener(() => UpdateInventoryItem(1, shopCategory));
        nextItemButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);
    }

    public void UpdateInventoryItem(int index, ShopCategory shopCategory)
    {
        itemObjects[currentIndex].SetActive(false);
        Quaternion rotation = itemObjects[currentIndex].transform.rotation;
    
        currentIndex = (currentIndex + index + inventoryItems.Count) % inventoryItems.Count;

        itemObjects[currentIndex].SetActive(true);
        itemObjects[currentIndex].transform.position = Camera.main.ScreenToWorldPoint(itemsParent.position);
        itemObjects[currentIndex].transform.rotation = rotation;

        InventoryItemDefinition inventoryItem = inventoryItems[currentIndex];

        var playersInventoryItem = EconomyManager.Instance.PlayersInventoryItems.Find(i => i.InventoryItemId == inventoryItem.Id);

        SetButtonState(buyItemButton, () => EconomyManager.Instance.OnPurchaseClicked(inventoryItem, () => OnPurchaseButtonClick(inventoryItem, shopCategory)), playersInventoryItem == null);
        SetSelectItemButtonState(() => OnSelectButtonClick(inventoryItem, shopCategory), playersInventoryItem != null, Util.GetSelectedItemId(shopCategory.ToString()) != inventoryItem.Id);

        lockIcon.SetActive(playersInventoryItem == null);

        string jsonData = playersInventoryItem?.InstanceData.GetAsString() ?? inventoryItem.CustomDataDeserializable.GetAsString();

        InitializeItemUpgrades(jsonData, playersInventoryItem?.PlayersInventoryItemId ?? string.Empty, shopCategory);
    }

    private void OnSelectButtonClick(InventoryItemDefinition inventoryItem, ShopCategory shopCategory)
    {
        Util.SetSelectedItemId(shopCategory.ToString(), inventoryItem.Id);
        UpdateSelectItemState(false);
    }

    private void OnPurchaseButtonClick(InventoryItemDefinition inventoryItem, ShopCategory shopCategory)
    {
        OnSelectButtonClick(inventoryItem, shopCategory);

        string jsonData = inventoryItem.CustomDataDeserializable.GetAsString();

        switch (shopCategory)
        {
            case ShopCategory.BIKE:
                MotorbikeData motorbikeData = JsonUtility.FromJson<MotorbikeData>(jsonData);
                EventsService.UserGarageAsync(motorbikeData.enumId, motorbikeData.price.ToInt());
                break;
            case ShopCategory.WEAPON:
                WeaponData weaponData = JsonUtility.FromJson<WeaponData>(jsonData);
                EventsService.UserWeaponAsync(weaponData.enumId, weaponData.price.ToInt());
                break;
        }

        UpdateInventoryItem(0, shopCategory);
    }

    private void SetButtonState(Button button, UnityEngine.Events.UnityAction action, bool state)
    {
        button.gameObject.SetActive(state);
        button.interactable = state;
        button.onClick.RemoveAllListeners();

        if (state)
        {
            button.onClick.AddListener(action);
            button.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);
        }
    }

    private void SetSelectItemButtonState(UnityEngine.Events.UnityAction action, bool showState, bool interactableState)
    {
        SetButtonState(selectItemButton, action, showState);
        UpdateSelectItemState(interactableState);
    }

    private void UpdateSelectItemState(bool state)
    {
        selectItemButton.interactable = state;
        selectItemButton.GetComponent<Image>().color = state ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.35f);
        selectItemButtonText.text = state ? $"Select" : $"Selected";
        selectItemButtonText.color = state ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.3f);
    }

    private void InitializeItemObjects(string path)
    {
        foreach (InventoryItemDefinition inventoryItem in inventoryItems)
        {
            var itemObject = Instantiate(Resources.Load($"Shop/{path}/{inventoryItem.Id}"), itemsParent) as GameObject;
            itemObject.SetActive(false);
            itemObjects.Add(itemObject);
        }
    }

    public void Hide()
    {
        ClearAll();
        gameObject.SetActive(false);
    }

    private void ClearAll()
    {
        previousItemButton.onClick.RemoveAllListeners();
        nextItemButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveAllListeners();

        currentIndex = 0;
        inventoryItems.Clear();

        if (upgradeItems.Count != 0)
        {
            foreach(AttributeItemView item in upgradeItems.Values)
            {
                Destroy(item.gameObject);
            }
            upgradeItems.Clear();
        }

        if(itemObjects.Count != 0)
        {
            foreach(GameObject item in itemObjects)
            {
                Destroy(item.gameObject);
            }
            itemObjects.Clear();
        }
    }

    public override void UpdateView(BaseState state)
    {
        // Update the UI based on the current game state
        if (state is ItemShopState)
        {
            Show(state);
        }
        else
        {
            Hide();
        }
    }

    protected override void Show(BaseState state)
    {
        base.Show(state);

        ItemShopState itemShopState = state as ItemShopState;

        currencyHudView.gameObject.SetActive(true);
        Initialize(itemShopState.ShopCategory);
        backButton.onClick.AddListener(itemShopState.OnBackButtonPressed);
        backButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);
    }

    private void InitializeWeaponUpgrades(WeaponData weaponData, string playersInventoryItemId)
    {
        foreach (WeaponUpgradeData upgrade in weaponData.upgrades)
        {
            if (upgradeItems.ContainsKey(upgrade.enumId))
            {
                upgradeItems[upgrade.enumId].Initialize(upgrade, weaponData, playersInventoryItemId, upgradeButtonsLayout);
            }
            else
            {
                AttributeItemView upgradeItem = Instantiate(attributeItemView.gameObject, attributesParent).GetComponent<AttributeItemView>();
                upgradeItem.Initialize(upgrade, weaponData, playersInventoryItemId, upgradeButtonsLayout);
                upgradeItems[upgrade.enumId] = upgradeItem;
            }
            itemNameText.text = weaponData.name;
            bool isItemIAP = weaponData.enumId == Assets.Scripts.Events.UserWeapon.Ak800; //TODO: Remove this after implementing Item IAP
            buyButtonPriceText.text = weaponData.price.ToString();
            buyIAPButtonPriceText.text = $"${weaponData.price}";
            if(isItemIAP)
            {
                buyItemButton.SetActive(false);
            }
            buyIAPButton.SetActive(isItemIAP);
        }
    }

    private void InitializeMotorbikeUpgrades(MotorbikeData motorbikeData, string playersInventoryItemId)
    {
        foreach (MotorbikeUpgradeData upgrade in motorbikeData.upgrades)
        {
            if (upgradeItems.ContainsKey(upgrade.enumId))
            {
                upgradeItems[upgrade.enumId].Initialize(upgrade, motorbikeData, playersInventoryItemId, upgradeButtonsLayout);
            }
            else
            {
                AttributeItemView upgradeItem = Instantiate(attributeItemView.gameObject, attributesParent).GetComponent<AttributeItemView>();
                upgradeItem.Initialize(upgrade, motorbikeData, playersInventoryItemId, upgradeButtonsLayout);
                upgradeItems[upgrade.enumId] = upgradeItem;
            }
            itemNameText.text = motorbikeData.name;
            bool isItemIAP = motorbikeData.enumId == Assets.Scripts.Events.UserMotorbike.Infinitron; //TODO: Remove this after implementing Item IAP
            buyButtonPriceText.text = motorbikeData.price.ToString();
            buyIAPButtonPriceText.text = $"${motorbikeData.price}";
            if (isItemIAP)
            {
                buyItemButton.SetActive(false);
            }
            buyIAPButton.SetActive(isItemIAP);
        }
    }

    private void InitializeItemUpgrades(string jsonData, string playersInventoryItemId, ShopCategory shopCategory)
    {
        switch (shopCategory)
        {
            case ShopCategory.BIKE:
                InitializeMotorbikeUpgrades(JsonUtility.FromJson<MotorbikeData>(jsonData), playersInventoryItemId);
                break;
            case ShopCategory.WEAPON:
                InitializeWeaponUpgrades(JsonUtility.FromJson<WeaponData>(jsonData), playersInventoryItemId);
                break;
        }
    }
}
