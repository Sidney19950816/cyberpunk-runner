using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/WeaponScriptableObject", order = 1)]
public class WeaponScriptableObject : ScriptableObject
{
    public enum ProjectileType
    {
        Bullet,
        Grenade
    }

    [Header("Weapon ID")]
    [SerializeField] private int _id;

    [Space, Header("Weapon Info")]
    [SerializeField] private string _weaponName;
    [SerializeField] private string _weaponDescription;
    [SerializeField] private float _weaponPrice;

    [Space, Header("Weapon Parameters")]
    [SerializeField] private ProjectileType _projectileType;
    [SerializeField] private int _projectilesCount;
    [SerializeField] private int _fireRate;
    [SerializeField] private float _reloadDelay;
    [SerializeField] private float _projectileLoadSpeed;
    [SerializeField] private float _projectileSpeed;
    [SerializeField] private float _damage;
    [SerializeField] private float _critChance;

    [Space, Header("Models")]
    [SerializeField] private GameObject _weaponObject;
    [SerializeField] private GameObject _projectileObject;

    public int Id => _id;

    public string WeaponName => _weaponName;
    public string WeaponDescription => _weaponDescription;
    public float WeaponPrice => _weaponPrice;

    public int ProjectilesCount => _projectilesCount;
    public int FireRate => _fireRate;
    public float ReloadDelay => _reloadDelay;
    public float ProjectileLoadSpeed => _projectileLoadSpeed;
    public float ProjectileSpeed => _projectileSpeed;
    public float Damage => _damage;
    public float CritChance => _critChance;


    public GameObject WeaponObject => _weaponObject;
    public GameObject ProjectileObject => _projectileObject; // Not needed

    public ProjectileType GetProjectileType()
    {
        return _projectileType;
    }
}
