using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Managers;
using UnityEngine;

public class GameState : BaseState 
{
    public Assets.Scripts.ArcadeBike Bike { get; private set; }

    public GameState(Assets.Scripts.ArcadeBike bike)
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
