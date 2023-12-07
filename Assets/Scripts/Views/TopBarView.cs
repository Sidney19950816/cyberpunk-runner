using System;
using System.Collections;
using Assets.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

public class TopBarView : BaseView
{
    [Header("BUTTONS")]
    [SerializeField] private Button addChargeButton;
    [SerializeField] private Button addRokenButton;
    [SerializeField] private Button addBiochipButton;

    [Space, Header("MOTORBIKE BATTERY UI")]
    [SerializeField] private GameObject motorbikeBatteryObject;
    [SerializeField] private VerticalLayoutGroup batteryLayoutGroup;
    [SerializeField] private Image batteryImage;

    private void Start()
    {
        ShopView shopView = GameSceneManager.Instance.Canvas.GetComponentInChildren<ShopView>(true);

        if (shopView != null)
        {
            addRokenButton.onClick.AddListener(shopView.SnapToCurrency);
            addBiochipButton.onClick.AddListener(shopView.SnapToCurrency);

            addRokenButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);
            addBiochipButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);

            addChargeButton.onClick.AddListener(AddCharge);
            addChargeButton.onClick.AddListener(AudioManager.Instance.PlayUIButtonSound);
        }
    }

    public override void UpdateView(BaseState state)
    {
        // Update the UI based on the current game state
        if (state is GameState || state is FightState || state is GameOverState)
        {
            Hide();
        }
        else
        {
            Show(state);
        }
    }

    private void AddCharge()
    {
        GoogleMobileAdsController.Instance?.RewardedAdController?.ShowAd(() => {
            GameSceneManager.Instance.MotorbikeBattery.OnAdButtonPressed();
        });
    }

    public void UpdateStatus(MotorbikeBatteryEventManager eventManager)
    {
        if(eventManager.batteryCount > 0)
        {
            addChargeButton.interactable = eventManager.batteryCount < 5;

            if(batteryLayoutGroup.transform.childCount == 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    Instantiate(batteryImage, batteryLayoutGroup.transform);
                }
            }

            int batteryCount = eventManager.batteryCount;
            foreach (Transform child in batteryLayoutGroup.transform)
            {
                child.SetActive(batteryCount-- > 0);
            }
        }
        else
        {
            foreach(Transform child in batteryLayoutGroup.transform)
            {
                child.SetActive(false);
            }
        }
    }
}
