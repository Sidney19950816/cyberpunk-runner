using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts;
using Unity.Services.CloudCode;
using UnityEngine;

public class MotorbikeBatteryEventManager : BaseBehaviour
{
    GetStatusResult m_Status;

    public bool isEventReady => m_Status.success;

    public bool firstVisit => m_Status.firstVisit;

    public bool isChargeable => m_Status.isChargeable;

    public long lastRechargeTime => m_Status.lastRechargeTime;

    public int secondsPerCharge => m_Status.secondsPerCharge;

    public int batteryCount => m_Status.batteryCount;

    public async Task RefreshMotorbikeBatteryStatus()
    {
        try
        {
            m_Status = await CloudCodeManager.instance.CallBatteryGetStatusEndpoint();
            if (this == null) return;

            Debug.Log($"Motorbike Battery Status: {m_Status}");
        }
        catch (CloudCodeResultUnavailableException)
        {
            // Exception already handled by CloudCodeManager
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public bool UpdateMotorbikeBattery()
    {
        if (m_Status.batteryCount < 5 && m_Status.lastRechargeTime > 0)
        {
            var timeSinceLastRecharge = GetCurrentTimeInMillis() - m_Status.lastRechargeTime;

            //secondsPerCharge in milliseconds
            var rechargeInterval = m_Status.secondsPerCharge * 1000;

            if (timeSinceLastRecharge >= rechargeInterval)
            {
                m_Status.lastRechargeTime = 0;
                return true;
            }
        }

        return false;
    }

    public long GetCurrentTimeInMillis()
    {
        // Get the current UTC time
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;

        // Convert it to milliseconds since the Unix epoch
        long currentTimeInMillis = currentTime.ToUnixTimeMilliseconds();

        return currentTimeInMillis;
    }

    public async Task UpdateMotorbikeBattery(int batteryChangeAmount)
    {
        try
        {
            Debug.Log("Calling Cloud Code 'MotorbikeBattery_Update' to update the battery.");

            var updateResult = await CloudCodeManager.instance.CallBatteryUpdateEndpoint(batteryChangeAmount);
            if (this == null) return;

            // Save updated status so we can return it later, when requested
            SaveStatus(updateResult);
        }
        catch (CloudCodeResultUnavailableException)
        {
            // Exception already handled by CloudCodeManager
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void MarkFirstVisitComplete()
    {
        m_Status.firstVisit = false;
    }

    void SaveStatus(UpdateResult updateResult)
    {
        m_Status.success = updateResult.success;
        m_Status.firstVisit = updateResult.firstVisit;
        m_Status.isChargeable = updateResult.isChargeable;
        m_Status.batteryCount = updateResult.batteryCount;
        m_Status.lastRechargeTime = updateResult.lastRechargeTime;
        m_Status.secondsPerCharge = updateResult.secondsPerCharge;
    }

    // Struct matches response from the Cloud Code get-status call to retrieve the current state of
    // Motorbike Battery event.
    public struct GetStatusResult
    {
        public bool success;
        public bool firstVisit;
        public bool isChargeable;
        public int batteryCount;
        public int secondsPerCharge;
        public long lastRechargeTime;

        public override string ToString()
        {
            return $"success:{success} || firstVisit:{firstVisit} || isChargeable:{isChargeable} " +
                $" || secondsPerCharge:{secondsPerCharge:0.#} || batteryCount:{batteryCount} || lastRechargeTime: {lastRechargeTime}";
        }
    }

    public struct UpdateResult
    {
        public bool success;
        public bool firstVisit;
        public bool isChargeable;
        public int batteryCount;
        public int secondsPerCharge;
        public long lastRechargeTime;

        public override string ToString()
        {
            return $"success:{success} || firstVisit:{firstVisit} || isChargeable:{isChargeable} " +
                $" || secondsPerCharge:{secondsPerCharge:0.#} || batteryCount:{batteryCount} || lastRechargeTime: {lastRechargeTime}";
        }
    }
}
