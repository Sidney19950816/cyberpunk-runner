using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using System;
using Assets.Scripts.World;
using UnityEngine.UI;

namespace Assets.Scripts.Managers
{
    public class GameSceneManager:MonoBehaviour
    {
        #region Instance
        public static GameSceneManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }

            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        [SerializeField] private CinemachineVirtualCamera followVirtualCamera;
        [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;

        [Space, Header("World Controller")]
        [SerializeField] private WorldController worldController;

        [Space, Header("Authentication")]
        [SerializeField] private Authentication authentication;

        [Space, Header("Loading GameObject")]
        [SerializeField] private GameObject loadingObject;

        [Space, Header("Canvas Object")]
        [SerializeField] private Canvas canvas;

        [Space, Header("Motorbike Battery")]
        [SerializeField] private MotorbikeBatteryManager motorbikeBattery;

        public MotorbikeBatteryManager MotorbikeBattery => motorbikeBattery;

        public Camera MainCamera { get; private set; }
        public SlowMotion SlowMotion { get; private set; }
        public CinemachineVirtualCamera FollowVirtualCamera => followVirtualCamera;
        public CinemachineVirtualCamera AimVirtualCamera => aimVirtualCamera;

        public WorldController WorldController => worldController; // Do I need a reference to this?
        public Authentication Authentication => authentication ?? GetComponent<Authentication>();
        public Canvas Canvas => canvas;

        async void Start()
        {
            MainCamera = Camera.main;
            SlowMotion = GetComponent<SlowMotion>();

            GameObject loading = GameObject.Find("LoadingCanvas");
            Slider slider = loading?.transform?.GetChild(1)?.GetComponent<Slider>();

            try
            {
                if (slider != null)
                    slider.value = .75f;

                GameInitializationManager gameInitializationManager = new GameInitializationManager(this);
                await gameInitializationManager.InitializeGameAsync();
                await MotorbikeBattery.RefreshStatus();

                if (slider != null)
                    slider.value = 1f;

                //EconomyManager.Instance.DeleteInventoryItemAsync("b39f6557-5291-40d1-9821-03b24f974d39");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                if (loading != null)
                    Destroy(loading);
            }
        }

        public void RestartScene()
        {
            SceneManager.LoadScene(1);
            Time.timeScale = 1;
            Time.fixedDeltaTime = 0.02f;
        }

        void OnApplicationPause(bool pauseStatus)
        {
            // Check the pauseStatus to see if we are in the foreground
            // or background
            if (!pauseStatus)
            {
                FacebookInitializer.InitializeFacebook();
            }
        }
    }
}