using System.Linq;
using Assets.Scripts;
using Assets.Scripts.DailyRewards;
using Assets.Scripts.Managers;
using Unity.Services.Economy.Model;
using UnityEngine;

public class MainMenuState : BaseState
{
    private Bike Bike;

    public MainMenuState()
    {
    }

    public override void OnStateEnter()
    {
        // Code to enter main menu state
        if (EconomyManager.Instance.PlayersInventoryItems.Count == 0)
        {
            EconomyManager.Instance.OnPurchaseClicked(EconomyManager.Instance.InventoryItemDefinitions.Find(i => i.Id == "BIKE_0"), SetDefaultBike);
        }
        else
        {
            // TEST
            //foreach (PlayersInventoryItem item in EconomyManager.Instance.PlayersInventoryItems)
            //{
            //    EconomyManager.Instance.DeleteInventoryItemAsync(item.PlayersInventoryItemId);
            //}

            InstantiateBike();
        }

        AudioManager.Instance.PlayMainMenuMusic();
    }

    public override void OnStateExit()
    {
        // Code to exit main menu state
    }

    public void OnStartButtonPressed()
    {
        GameSceneManager.Instance.FollowVirtualCamera.Follow = Bike.FollowCam;
        GameSceneManager.Instance.FollowVirtualCamera.LookAt = Bike.FollowCam;
        GameSceneManager.Instance.AimVirtualCamera.Follow = Bike.AimCam;
        StateManager.SetState(new GameState(Bike));
        GameSceneManager.Instance.MotorbikeBattery.OnStartButtonPressed();

        Bike.Rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionZ;
        Bike.transform.rotation = Quaternion.Euler(Vector3.zero);
        Bike.transform.position = new Vector3(0, 0, 3);
    }

    public bool GetStartButtonState()
    {
        return GameSceneManager.Instance.MotorbikeBattery.GetBatteryCount() > 0;
    }

    public void OnWeaponShopButtonPressed()
    {
        StateManager.SetState(new ItemShopState(ShopCategory.WEAPON));
        GameObject.Destroy(Bike?.gameObject);
    }

    public void OnBikeShopButtonPressed()
    {
        StateManager.SetState(new ItemShopState(ShopCategory.BIKE));
        GameObject.Destroy(Bike?.gameObject);
    }

    public void OnShopButtonPressed()
    {
        StateManager.SetState(new ShopState());
        GameObject.Destroy(Bike?.gameObject);
    }

    public void OnSettingsButtonPressed()
    {
        StateManager.SetState(new SettingsState());
        GameObject.Destroy(Bike?.gameObject);
    }

    public void OnDailyRewardsButtonPressed()
    {
        DailyRewardsManager.Instance.OnOpenEventButtonPressed();
        GameObject.Destroy(Bike?.gameObject);
    }

    private void SetDefaultBike()
    {
        Util.SetSelectedItemId("BIKE", "BIKE_0");
        InstantiateBike();
    }

    private void InstantiateBike()
    {
        var playersInventoryItem = EconomyManager.Instance.PlayersInventoryItems.Find(i => i.InventoryItemId == Util.GetSelectedItemId("BIKE"));
        string jsonData = playersInventoryItem?.InstanceData.GetAsString();
        MotorbikeData bikeData = JsonUtility.FromJson<MotorbikeData>(jsonData);

        GameObject bikeObject = (GameObject)Object.Instantiate(Resources.Load($"Game/Bike/{Util.GetSelectedItemId("BIKE")}"));
        Bike = bikeObject.GetComponent<Bike>()
            .With(b => b.Initialize(bikeData));
    }
}
