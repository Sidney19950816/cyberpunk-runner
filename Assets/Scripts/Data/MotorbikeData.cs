using System.Collections.Generic;
using Assets.Scripts.Events;

public class MotorbikeData : ItemData
{
    public UserMotorbike enumId;

    public List<MotorbikeUpgradeData> upgrades;

    public int minRokenSpeedRequirement;
    public int baseRokenAmount;
    public int topSpeedRokenMultiplier;
    public int rampRokenBonus;
}
