using Assets.Scripts;
using Assets.Scripts.Managers;
using UnityEngine;

public class GameState : BaseState 
{
    private const string BIKE = "Bike";

    public Bike Bike { get; private set; }

    public GameState()
    {
        InstantiateBike();
    }

    public GameState(Bike bike)
    {
        Bike = bike;
    }

    public override void OnStateEnter()
    {
        if (Bike != null)
            Bike.enabled = true;
    }

    public override void OnStateExit()
    {
        if(Bike != null)
            Bike.enabled = false;
    }

    private void InstantiateBike()
    {
        var playersInventoryItem = EconomyManager.Instance.PlayersInventoryItems.Find(i => i.InventoryItemId == Util.GetSelectedItemId(BIKE));
        string jsonData = playersInventoryItem?.InstanceData.GetAsString();
        MotorbikeData bikeData = JsonUtility.FromJson<MotorbikeData>(jsonData);

        GameObject bikeObject = (GameObject)Object.Instantiate(Resources.Load($"Game/Bike/{Util.GetSelectedItemId(BIKE)}"));
        Bike = bikeObject.GetComponent<Bike>()
            .With(b => b.Initialize(bikeData));

        GameSceneManager.Instance.FollowVirtualCamera.Follow = Bike.FollowCam;
        GameSceneManager.Instance.FollowVirtualCamera.LookAt = Bike.FollowCam;
        GameSceneManager.Instance.AimVirtualCamera.Follow = Bike.AimCam;

        Bike.transform.position = new Vector3(0, 0, 3);
    }
}
