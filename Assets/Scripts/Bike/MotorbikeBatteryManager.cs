using System;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts;

public class MotorbikeBatteryManager : BaseBehaviour
{
    [SerializeField] private TopBarView barView;
    [SerializeField] private MainMenuView mainMenuView;

    [SerializeField] private MotorbikeBatteryEventManager eventManager;

    public Action<MotorbikeBatteryEventManager> OnStatusUpdate;

    public async Task RefreshStatus()
    {
        try
        {
            await eventManager.RefreshMotorbikeBatteryStatus();

            if (this == null) return;

            Debug.Log("Motorbike Battery Initialization complete.");

            UpdateState();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private async void Update()
    {
        if (!eventManager.isEventReady)
        {
            return;
        }

        if (eventManager.UpdateMotorbikeBattery())
        {
            await RefreshStatus();
        }
    }

    private void UpdateState()
    {
        barView.UpdateStatus(eventManager);
        mainMenuView.UpdateStatus(eventManager);

        if (eventManager.firstVisit)
        {
            eventManager.MarkFirstVisitComplete();
        }
    }

    public async void OnAdButtonPressed()
    {
        try
        {
            await eventManager.UpdateMotorbikeBattery(1);
            if (this == null) return;

            UpdateState();
            await EventsService.ChargeMotorbikeAsync();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async void OnStartButtonPressed()
    {
        try
        {
            await eventManager.UpdateMotorbikeBattery(-1);
            if (this == null) return;

            UpdateState();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public int GetBatteryCount()
    {
        return eventManager.batteryCount;
    }
}
