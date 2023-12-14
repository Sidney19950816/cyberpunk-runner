using System;
using Assets.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButtonView : MonoBehaviour
{
    [Header("TEXTS")]
    [SerializeField] private Text upgradeNameText;
    [SerializeField] private Text upgradeValueText;
    [SerializeField] private TextMeshProUGUI upgradePriceText;

    [Space, Header("IMAGES")]
    [SerializeField] private Image currencyImage;

    [Space, Header("BUTTONS")]
    [SerializeField] private Button upgradeButton;

    [Space, Header("AUDIO CLIPS")]
    [SerializeField] private AudioClip upgradeAudio;

    public void Initialize(UpgradeData attribute, ItemData itemData, string playersInventoryItemId, Action callback)
    {
        upgradeNameText.text = attribute.name;
        string symbol = attribute.upgradeValue < 0 ? "" : "+";
        upgradeValueText.text = $"{symbol}{attribute.upgradeValue}";
        upgradePriceText.text = attribute.upgradePrice.ToString();
        currencyImage.sprite = Util.GetCurrencySprite(attribute.currencyType);

        if (upgradeButton.gameObject.activeSelf)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(() => itemData.Upgrade(attribute, playersInventoryItemId, callback));
            upgradeButton.onClick.AddListener(() => AudioManager.Instance.PlaySound(upgradeAudio));
        }
    }
}
