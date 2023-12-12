using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts;

public class AttributeItemView : BaseBehaviour
{
    [SerializeField] private Text upgradeNameText;
    [SerializeField] private Text upgradeValueText;
    [SerializeField] private Slider upgradeSliderValue;
    [SerializeField] private GameObject upgradeButtonGameObject;

    private UpgradeButtonView upgradeButtonView;

    public void Initialize(UpgradeData attribute, ItemData itemData, string playersInventoryItemId, Transform upgradesLayout)
    {
        upgradeNameText.text = attribute.name;
        upgradeValueText.text = $"<color=#BCF2FB>{attribute.value}</color><color=#7C7AA6>/{attribute.maxUpgradeableValue}</color>";

        float maxValue = attribute.value <= attribute.maxValue ? attribute.maxValue : -attribute.maxValue;
        float maxUpgradeableValue = maxValue >= 0 ? attribute.maxUpgradeableValue : -attribute.maxUpgradeableValue;
        float value = maxValue >= 0 ? attribute.value : -attribute.value;

        if (attribute.value > attribute.maxValue)
            upgradeSliderValue.minValue = -attribute.baseValue;
         
        upgradeSliderValue.maxValue = maxValue;
        upgradeSliderValue.value = value;

        if(upgradeButtonView == null)
            upgradeButtonView = Instantiate(upgradeButtonGameObject, upgradesLayout).GetComponent<UpgradeButtonView>();

        upgradeButtonView.gameObject.SetActive(!string.IsNullOrEmpty(playersInventoryItemId) && value < maxUpgradeableValue);
        upgradeButtonView.Initialize(attribute, itemData, playersInventoryItemId, () => OnUpgradeComplete(attribute, itemData, playersInventoryItemId, upgradesLayout));
    }

    private void OnUpgradeComplete(UpgradeData upgradeData, ItemData itemData, string playersInventoryItemId, Transform upgradesLayout)
    {
        if (itemData is MotorbikeData)
        {
            MotorbikeData motorbikeData = itemData as MotorbikeData;
            MotorbikeUpgradeData motorbikeUpgradeData = upgradeData as MotorbikeUpgradeData;
            EventsService.UpgradeMotorbikeAsync(motorbikeData.enumId, motorbikeUpgradeData.enumId);
        }
        if (itemData is WeaponData)
        {
            WeaponData weaponData = itemData as WeaponData;
            WeaponUpgradeData weaponUpgradeData = upgradeData as WeaponUpgradeData;
            EventsService.UpgradeWeaponAsync(weaponData.enumId, weaponUpgradeData.enumId);
        }

        Initialize(upgradeData, itemData, playersInventoryItemId, upgradesLayout);
    }

    private void OnDestroy()
    {
        if (upgradeButtonView != null)
            Destroy(upgradeButtonView.gameObject);
    }
}