using UnityEngine;

[CreateAssetMenu(fileName = "Bike", menuName = "ScriptableObjects/BikeScriptableObject", order = 1)]
public class BikeScriptableObject : ScriptableObject
{
    [Header("Bike ID")]
    [SerializeField] private int _id;

    [Space, Header("Bike Info")]
    [SerializeField] private string _bikeName;
    [SerializeField] private string _bikeDescription;
    [SerializeField] private float _bikePrice;

    [Space, Header("Bike Parameters")]
    [SerializeField] private float _topSpeed;
    [SerializeField] private float _acceleration;
    [SerializeField] private float _braking;
    [SerializeField] private float _steer;
    [SerializeField] private bool _isUnlocked;

    [Header("Roken Parameters")]
    [SerializeField] private float _minRokenSpeed;
    [SerializeField] private int _baseRokenValue;
    [SerializeField] private int _topSpeedRokenMultiplicator;
    [SerializeField] private int _rokensPerRamp;


    [Space, Header("Bike Models")]
    [SerializeField] private GameObject _shopObject;
    [SerializeField] private GameObject _bikePrefab;

    public int Id => _id;

    public string BikeName => _bikeName;
    public string BikeDescription => _bikeDescription;
    public float BikePrice => _bikePrice;

    public float TopSpeed => _topSpeed;
    public float Acceleration => _acceleration;
    public float Braking => _braking;
    public float Steer => _steer;
    public float MinRokenSpeed => _minRokenSpeed;
    public int BaseRokenValue => _baseRokenValue;
    public int TopSpeedRokenMultiplicator => _topSpeedRokenMultiplicator;
    public int RokensPerRamp => _rokensPerRamp;

    public bool IsUnlocked(bool state)
    {
        return _isUnlocked = state;
    }

    public GameObject ShopObject => _shopObject;
    public GameObject BikePrefab => _bikePrefab;
}
