using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using System.Collections;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using System;
using Unity.Services.Core.Environments;
using Assets.Scripts;

namespace PG
{
    /// <summary>
    /// The game controller is responsible for initializing the player's car.
    /// </summary>
    public class GameController : Singleton<GameController>
    {
        [SerializeField] private CinemachineVirtualCamera followVirtualCamera;
        [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;

        [Space, Header("Scriptable Objects")]
        [SerializeField] private BikeScriptableObject[] bikes;

        private Camera mainCamera;
        private SlowMotion slowMotion;

        public Camera MainCamera => mainCamera;
        public SlowMotion SlowMotion => slowMotion;
        public CinemachineVirtualCamera FollowVirtualCamera => followVirtualCamera;
        public CinemachineVirtualCamera AimVirtualCamera => aimVirtualCamera;

        public bool InFightScene => inFightScene;

        private bool inFightScene;

        //public List<CarController> AllCars = new();

        //public InitializePlayer Player1 { get; private set; }
        //public CarController PlayerCar1 { get; private set; }

        //List<VehicleController> AllVehicles = new();
        //List<VehicleController> VehiclePrefabs = new();

        async void Start()
        {
            try
            {
                var options = new InitializationOptions();

                options.SetEnvironmentName(UtilConstants.DEVELOPMENT_ENVIRONMENT_NAME);
                await UnityServices.InitializeAsync(options);

                // Check that scene has not been unloaded while processing async wait to prevent throw.
                if (this == null) return;

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    if (this == null) return;
                }

                Debug.Log($"Player id:{AuthenticationService.Instance.PlayerId}");

                // Economy configuration should be refreshed every time the app initializes.
                // Doing so updates the cached configuration data and initializes for this player any items or
                // currencies that were recently published.
                // 
                // It's important to do this update before making any other calls to the Economy or Remote Config
                // APIs as both use the cached data list. (Though it wouldn't be necessary to do if only using Remote
                // Config in your project and not Economy.)

                await EconomyManager.Instance.RefreshEconomyConfiguration();
                if (this == null) return;

                await Task.WhenAll(RemoteConfigManager.Instance.FetchConfigs(), 
                    EconomyManager.Instance.RefreshCurrencyBalances());

                if (this == null) return;

                mainCamera = Camera.main;
                slowMotion = GetComponent<SlowMotion>();

                if (MobileUI.Instance.RestartSceneButton)
                    MobileUI.Instance.RestartSceneButton.onClick.AddListener(RestartScene);
                if (MobileUI.Instance.GameOverViewButton)
                    MobileUI.Instance.GameOverViewButton.onClick.AddListener(RestartScene);

                BikeScriptableObject bike = bikes.FirstOrDefault(bike => bike.Id == 1); // Instantiate the selected bike
                if (bike != null)
                {
                    Assets.Scripts.ArcadeBike b = Instantiate(bike.BikePrefab).GetComponent<Assets.Scripts.ArcadeBike>();
                    followVirtualCamera.Follow = b.FollowCam;
                    followVirtualCamera.LookAt = b.FollowCam;
                    aimVirtualCamera.Follow = b.AimCam;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        /*void Start()
        {
            mainCamera = Camera.main;
            slowMotion = GetComponent<SlowMotion>();

            if (MobileUI.Instance.RestartSceneButton)
                MobileUI.Instance.RestartSceneButton.onClick.AddListener(RestartScene);
            if (MobileUI.Instance.GameOverViewButton)
                MobileUI.Instance.GameOverViewButton.onClick.AddListener(RestartScene);

            BikeScriptableObject bike = bikes.FirstOrDefault(bike => bike.Id == 1);
            if(bike != null)
            {
                Assets.Scripts.ArcadeBike b = Instantiate(bike.BikePrefab).GetComponent<Assets.Scripts.ArcadeBike>();
                b.Init(bike);

                followVirtualCamera.Follow = b.FollowCam;
                followVirtualCamera.LookAt = b.FollowCam;
                aimVirtualCamera.Follow = b.AimCam;
            }

            *//*AllCars.RemoveAll(c => c == null);
            AllVehicles = FindObjectsOfType<VehicleController>().ToList();
            var allCars = FindObjectsOfType<CarController>().ToList();
            AllCars.AddRange(allCars.Where(c => !AllCars.Contains(c)));


            if (!PlayerCar1 && AllCars.Count == 0)
            {
                PlayerCar1 =
                    Instantiate(B.GameSettings.AvailableVehicles.First(v => v as CarController) as CarController);
                AllVehicles.Add(PlayerCar1);
                AllCars.Add(PlayerCar1);
            }
            else if (!PlayerCar1)
            {
                PlayerCar1 = AllCars[0];
            }

            foreach (var vehicle in AllVehicles)
            {
                var prefab = B.GameSettings.AvailableVehicles.FirstOrDefault(v => vehicle.VehicleName == v.VehicleName);
                if (prefab == null)
                {
                    prefab = AllVehicles[0];
                }

                VehiclePrefabs.Add(prefab);
            }*//*

            //UpdateSelectedCars();

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }*/

        /*        private void OnDestroy()
                {
                    if (MobileUI.Instance.RestartSceneButton)
                        MobileUI.Instance.RestartSceneButton.onClick.RemoveListener(RestartScene);
                }*/

        public void RestartScene()
        {
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
            Time.timeScale = 1;
            Time.fixedDeltaTime = 0.02f;
        }

/*        public void SetNextCar()
        {
            if (PlayerCar1)
            {
                var studioListiner = PlayerCar1.GetComponent<AudioListener>();

                if (studioListiner)
                {
                    studioListiner.enabled = false;
                }
            }

            var index = PlayerCar1 ? AllCars.IndexOf(PlayerCar1) : 0;
            index = MathExtentions.Repeat(index + 1, 0, AllCars.Count - 1);

            PlayerCar1 = AllCars[index];
            UpdateSelectedCars();
        }

        void UpdateSelectedCars()
        {
            Player1 = UpdateSelectedCar(Player1, PlayerCar1);
        }

        private static InitializePlayer UpdateSelectedCar(InitializePlayer player, CarController car)
        {
            var playerPrefab = IsMobilePlatform
                ? B.ResourcesSettings.PlayerControllerPrefab_ForMobile
                : B.ResourcesSettings.PlayerControllerPrefab;

            if (player && player.Car != car)
            {
                Destroy(player.gameObject);
                player = Instantiate(playerPrefab);
            }

            if (!player)
            {
                player = Instantiate(playerPrefab);
            }

            if (player.Initialize(car))
            {
                player.name = $"PlayerController_{player.Vehicle.name}";
                Debug.LogFormat("Player for {0} is initialized", player.Vehicle.name);
            }

            return player;
        }*/

        public void SetInFightSceneState(bool state)
        {
            inFightScene = state;
            SlowMotion.SetSlowMotionState(state);
            //BikeController.Instance.BikeFreezeState(state);
            followVirtualCamera.SetActive(!state);
            aimVirtualCamera.SetActive(state);
        }

        public void CameraShake(float duration, float amplitude)
        {
            StartCoroutine(CameraShakeCoroutine(duration, amplitude));
        }

        private IEnumerator CameraShakeCoroutine(float duration, float amplitude)
        {
            CinemachineVirtualCamera vCamera = CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera as CinemachineVirtualCamera;
            CinemachineBasicMultiChannelPerlin cameraNoise = vCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            cameraNoise.m_AmplitudeGain = amplitude;
            yield return new WaitForSeconds(duration);
            cameraNoise.m_AmplitudeGain = 0;
        }
    }
}