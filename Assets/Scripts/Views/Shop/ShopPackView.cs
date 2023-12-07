using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using System.Linq;

public class ShopPackView : MonoBehaviour
{
    [Header("TEXT")]
    [SerializeField] private Text priceText;
    [SerializeField] private Text bikeNameText;
    [SerializeField] private Text weaponNameText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI rokenText;
    [SerializeField] private TextMeshProUGUI biochipText;
    [SerializeField] private TextMeshProUGUI xValueText;

    [Space, Header("IMAGES")]
    [SerializeField] private Image bikeImage;
    [SerializeField] private Image weaponImage;

    [Space, Header("BUTTONS")]
    [SerializeField] private Button purchaseButton;

    [Space, Header("GAMEOBJECTS")]
    [SerializeField] private GameObject bestChoiceObject;

    public delegate void PurchaseEvent(Product Model, Action<bool> OnComplete);
    public event PurchaseEvent OnPurchase;

    private Product model;

    public void Init(Product product)
    {
        model = product;
        nameText.text = product.metadata.localizedTitle;
        priceText.text = $"{product.metadata.localizedPriceString} {product.metadata.isoCurrencyCode}";

        foreach(PayoutDefinition payout in product.definition.payouts)
        {
            if(payout.data != string.Empty)
            {
                PackData packData = JsonUtility.FromJson<PackData>(payout.data);

                if(packData != null)
                {
                    bestChoiceObject.SetActive(packData.showBestChoice);
                    xValueText.text = $"{packData.xValue} Value";
                }
            }

            if(payout.type == PayoutType.Item)
            {
                if(payout.subtype.Contains(UtilConstants.BIKE))
                {
                    bikeNameText.text = EconomyManager.Instance.InventoryItemDefinitions.Find(i => i.Id == payout.subtype).Name;
                    bikeImage.sprite = Resources.Load<Sprite>($"UI/Shop/{payout.subtype}");
                }
                else if (payout.subtype.Contains(UtilConstants.WEAPON))
                {
                    weaponNameText.text = EconomyManager.Instance.InventoryItemDefinitions.Find(i => i.Id == payout.subtype).Name;
                }
            }
            else if(payout.type == PayoutType.Currency)
            {
                if (payout.subtype.Equals(UtilConstants.CURRENCY_ROKEN_ID))
                {
                    rokenText.text = $"{payout.quantity}";
                }
                else if (payout.subtype.Equals(UtilConstants.CURRENCY_BIOCHIP_ID))
                {
                    biochipText.text = $"{payout.quantity}";
                }
            }
        }

        purchaseButton.onClick.AddListener(Purchase);
    }

    public void Purchase()
    {
        purchaseButton.enabled = false;
        OnPurchase?.Invoke(model, HandlePurchaseComplete);
    }

    private void HandlePurchaseComplete(bool OnSuccess)
    {
        purchaseButton.enabled = true;
        if (OnSuccess)
            Destroy(gameObject);
    }

    public class PackData
    {
        public bool showBestChoice;
        public string xValue;
    }
}
