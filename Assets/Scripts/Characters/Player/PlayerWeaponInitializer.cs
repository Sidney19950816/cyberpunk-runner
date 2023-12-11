using Assets.Scripts;
using Unity.Services.Economy.Model;
using UnityEngine;

public class PlayerWeaponInitializer : MonoBehaviour
{
    private const string WEAPON = "WEAPON";
    private const string DEFAULT_WEAPON = "WEAPON_0";
    private const string WEAPON_PATH = "Game/Weapon/";

    public PlayerWeapon Initialize()
    {
/*        EconomyManager.Instance.DeleteInventoryItemAsync(EconomyManager.Instance.PlayersInventoryItems.Find(i => i.InventoryItemId == Util.GetSelectedItemId("WEAPON")).PlayersInventoryItemId);
        return null; // TEST*/

        if (!EconomyManager.Instance.PlayersInventoryItems
            .Exists(i => i.InventoryItemId.Contains(WEAPON)))
        {
            EconomyManager.Instance.OnPurchaseClicked
                (EconomyManager.Instance.InventoryItemDefinitions
                .Find(i => i.Id == DEFAULT_WEAPON), SetDefaultWeapon);
        }

        return GetWeapon();
    }

    private PlayerWeapon GetWeapon()
    {
        var playersInventoryItem = GetPlayersInventoryItem();

        WeaponData weaponData = playersInventoryItem != null 
            ? GetWeaponData(playersInventoryItem) 
            : GetWeaponData(GetInventoryItemDefinition());

        return GetWeaponObject()
            .GetComponent<PlayerWeapon>()
            .With(w => w.Initialize(weaponData));
    }

    private void SetDefaultWeapon()
    {
        Util.SetSelectedItemId(WEAPON, DEFAULT_WEAPON);
    }

    private PlayersInventoryItem GetPlayersInventoryItem()
        => EconomyManager.Instance.PlayersInventoryItems
        .Find(i => i.InventoryItemId == Util.GetSelectedItemId(WEAPON));

    private InventoryItemDefinition GetInventoryItemDefinition()
        => EconomyManager.Instance.InventoryItemDefinitions
        .Find(i => i.Id == Util.GetSelectedItemId(WEAPON));

    private WeaponData GetWeaponData(PlayersInventoryItem playersInventoryItem)
        => JsonUtility.FromJson<WeaponData>(
            playersInventoryItem?.InstanceData.GetAsString());

    private WeaponData GetWeaponData(InventoryItemDefinition inventoryItem)
    => JsonUtility.FromJson<WeaponData>(
        inventoryItem?.CustomDataDeserializable.GetAsString());

    private GameObject GetWeaponObject()
        => (GameObject)Instantiate(Resources
            .Load($"{WEAPON_PATH}{Util.GetSelectedItemId(WEAPON)}"), transform);
}
