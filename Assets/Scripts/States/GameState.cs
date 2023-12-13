using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Managers;
using UnityEngine;

public class GameState : BaseState 
{
    public Assets.Scripts.Bike Bike { get; private set; }

    public GameState(Assets.Scripts.Bike bike)
    {
        Bike = bike;
    }

    public override void OnStateEnter()
    {
        if (Bike != null)
            Bike.enabled = true;
    }

    public override void OnStateExit()
    {
        if(Bike != null)
            Bike.enabled = false;
    }
}
