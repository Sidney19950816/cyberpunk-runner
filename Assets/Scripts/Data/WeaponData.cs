using System.Collections.Generic;
using Assets.Scripts.Events;

public class WeaponData : ItemData
{
    public UserWeapon enumId;
    public WeaponMode enumMode;

    public List<WeaponUpgradeData> upgrades;

    public int headshotMultiplier;
    public int rokensPerKill;
    public int bulletsCount;
}
