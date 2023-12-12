using System;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Managers;
using UnityEngine;
using UnityEngine.Rendering;

public class FightState : BaseState
{
    public Bike Bike { get; private set; }
    public List<Enemy> Enemies { get; private set; }
    public float RemainingTime { get; private set; }

    private Vector3 savedVelocity;

    public FightState(Bike bike, List<Enemy> enemies, float remainingTime)
    {
        Bike = bike;
        Enemies = enemies;
        RemainingTime = remainingTime;
    }

    public override void OnStateEnter()
    {
        SetFightState(true);
        savedVelocity = Bike.Rigidbody.velocity;
        Bike.Rigidbody.velocity = savedVelocity * 0.2f;
        Bike.Collider.enabled = false;
    }

    public override void OnStateExit()
    {
        SetFightState(false);
        Bike.Player.Weapon.Reset();
        Bike.Rigidbody.velocity = savedVelocity;
    }

    private void SetFightState(bool state)
    {
        GameSceneManager.Instance.SlowMotion.SetSlowMotionState(state);
        GameSceneManager.Instance.FollowVirtualCamera.gameObject.SetActive(!state);
        GameSceneManager.Instance.AimVirtualCamera.gameObject.SetActive(state);
    }
}
