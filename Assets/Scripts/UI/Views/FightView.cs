using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Assets.Scripts;

public class FightView : BaseView
{
    [Space, Header("BULLETS")]
    [SerializeField] private TextMeshProUGUI bulletsCountText;

    [Space, Header("SLIDERS")]
    [SerializeField] private Slider remainingTimeSlider;

    private int headshotMultiplier;

    public override void UpdateView(BaseState newState)
    {
        // Update the UI based on the current game state
        if (newState is FightState)
        {
            Show(newState);
        }
        else
        {
            Hide(newState);
        }
    }

    protected override void Show(BaseState state)
    {
        base.Show(state);

        FightState fightState = state as FightState;

        Player player = fightState?.Bike?.Player;

        if (player == null)
            return;

        Initialize(player.Weapon.WeaponParameters.BulletsCount, fightState.RemainingTime);

        player.Weapon.OnProjectileUpdateAction += SetBulletsCount;
        headshotMultiplier = player.Weapon.WeaponParameters.HeadshotMultiplier;

        foreach (Enemy e in fightState.Enemies)
        {
            e.Head.OnHeadshot += () => OnEnemyHeadshot(player); // TODO
            e.GetComponent<EnemyDeath>().Died += () 
                => player.Score.Add(player.Weapon.WeaponParameters.RokensPerKill); // TODO
        }
    }

    public void Initialize(int bulletsCount, float remainingTime)
    {
        SetBulletsCount(bulletsCount);
        SetRemainingTimeValue(remainingTime);
    }

    private void Update()
    {
        if (Time.timeScale == 0)
            return;

        remainingTimeSlider.value -= Time.unscaledDeltaTime;
        remainingTimeSlider.gameObject.SetActive(remainingTimeSlider.value > 0);
    }

    private void SetBulletsCount(int bulletsCount)
    {
        bulletsCountText.text = bulletsCount.ToString();
    }

    private void SetRemainingTimeValue(float time)
    {
        remainingTimeSlider.value = remainingTimeSlider.maxValue = time;
    }

    private void OnEnemyHeadshot(Player player)
    {
        PlayerWeapon.WeaponParams weaponParams = player.Weapon.WeaponParameters;
        headshotMultiplier = weaponParams.HeadshotMultiplier > 1 ? headshotMultiplier : 1;
        player.Score.Add(weaponParams.RokensPerKill * headshotMultiplier);

        if (weaponParams.HeadshotMultiplier > 1)
            headshotMultiplier++;
    }

    protected override void Hide(BaseState state = null)
    {
        base.Hide(state);

        if(state is GameState)
        {
            GameState gameState = state as GameState;

            Player player = gameState?.Bike?.Player;

            if (player == null || player.Weapon == null)
                return;

            player.Weapon.OnProjectileUpdateAction -= SetBulletsCount;
        }
    }
}
