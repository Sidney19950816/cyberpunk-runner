using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using System;

public class ShopCurrencyView : MonoBehaviour
{
    [Header("TEXTS")]
    [SerializeField] private Text priceText;
    [SerializeField] private TextMeshProUGUI nameText;

    [Space, Header("IMAGES")]
    [SerializeField] private Image currencyImage;

    [Space, Header("SPRITES")]
    [SerializeField] private Sprite rokenSprite;
    [SerializeField] private Sprite biochipSprite;

    [Space, Header("BUTTONS")]
    [SerializeField] private Button purchaseButton;



    public delegate void PurchaseEvent(Product Model, Action<bool> OnComplete);
    public event PurchaseEvent OnPurchase;

    private Product model;

    public void Init(Product product)
    {
        model = product;
        nameText.text = product.metadata.localizedTitle;
        priceText.text = $"{product.metadata.localizedPriceString} {product.metadata.isoCurrencyCode}";

        currencyImage.sprite = product.definition.payout.subtype.Equals(UtilConstants.CURRENCY_ROKEN_ID) ? rokenSprite : biochipSprite;

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
    }
}
