using UnityEngine;
using Unity.RemoteConfig;
using Unity.Services.Core;
using Unity.Services.Authentication;

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

public class RemoteConfigManager : MonoBehaviour
{
    public struct userAttributes { }
    public struct appAttributes { }

    public static RemoteConfigManager Instance { get; private set; }

    public BikeConfig bikeConfig { get; private set; }

    public int EnemyHealthIncrease { get; private set; }
    public int EnemyHealthIncreaseInterval { get; private set; }
    public int MotorbikeDamageFactor { get; private set; }

    async Task InitializeRemoteConfigAsync()
    {
        await UnityServices.InitializeAsync();

        if(!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public async Task FetchConfigs()
    {
        try
        {
            //ConfigManager.FetchCompleted += ApplyRemoteSettings;

//#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ConfigManager.SetEnvironmentID(UtilConstants.DEVELOPMENT_ENVIRONMENT_ID);
//#else
//                ConfigManager.SetEnvironmentID(UtilConstants.PRODUCTION_ENVIRONMENT_ID);
//#endif

            //ConfigManager.FetchConfigs(new userAttributes(), new appAttributes() { });
            await FetchConfigsAsync();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private Task FetchConfigsAsync()
    {
        var fetchTaskCompletionSource = new TaskCompletionSource<bool>();

        ConfigManager.FetchCompleted += (response) =>
        {
            if (response.status == ConfigRequestStatus.Success)
            {
                if (!fetchTaskCompletionSource.Task.IsCompleted)
                {
                    fetchTaskCompletionSource.SetResult(true);
                    ApplyRemoteSettings(response);
                }
            }
        };

        ConfigManager.FetchConfigs(new userAttributes(), new appAttributes() { });

        return fetchTaskCompletionSource.Task;
    }

    void ApplyRemoteSettings(ConfigResponse configResponse)
    {
        // Conditionally update settings, depending on the response's origin:
        switch (configResponse.requestOrigin)
        {
            case ConfigOrigin.Default:
                Debug.Log("No settings loaded this session; using default values.");
                break;
            case ConfigOrigin.Cached:
                Debug.Log("No settings loaded this session; using cached values from a previous session.");
                break;
            case ConfigOrigin.Remote:
                Debug.Log("New settings loaded this session; update values accordingly.");
                string bikeConfigJson = ConfigManager.appConfig.GetJson(UtilConstants.BIKE_CONFIG);
                bikeConfig = JsonUtility.FromJson<BikeConfig>(bikeConfigJson);

                EnemyHealthIncrease = ConfigManager.appConfig.GetInt("ENEMY_HEALTH_INCREASE_CONFIG");
                EnemyHealthIncreaseInterval = ConfigManager.appConfig.GetInt("ENEMY_HEALTH_INCREASE_INTERVAL_CONFIG");
                MotorbikeDamageFactor = ConfigManager.appConfig.GetInt("MOTORBIKE_DAMAGE_FACTOR_CONFIG");
                break;
        }
    }

    [Serializable]
    public struct BikeConfig
    {
        public List<BikesConfig> bikes;

        public override string ToString()
        {
            return $"categories: {string.Join(", ", bikes.Select(bike => bike.ToString()).ToArray())}";
        }
    }

    [Serializable]
    public struct BikesConfig
    {
        public int id; // Change to string
        public BikeParametersConfig parameters;

        public override string ToString()
        {
            var returnString = new StringBuilder($"bike:\"{id}\" parameters.isPurchased: {parameters.isPurchased}");

            return returnString.ToString();
        }
    }

    [Serializable]
    public struct BikeParametersConfig
    {
        public string name;
        public string description;
        public int price;
        public bool isPurchased;
        public AttributesConfig[] attributes;
        public int minRokenSpeedRequirement;
        public int baseRokenAmount;
        public int topSpeedRokenMultiplier;
        public int rampRokenBonus;
    }

    [Serializable]
    public struct AttributesConfig
    {
        public string name;
        public int value;
        public int maxValue;
    }
}
