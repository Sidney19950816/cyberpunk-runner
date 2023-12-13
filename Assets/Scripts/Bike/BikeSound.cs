using Assets.Scripts;
using UnityEngine;
using SkrilStudio;
using Assets.Scripts.Managers;

public class BikeSound : MonoBehaviour
{
    [SerializeField] ElectricCarSounds ecs;
    [SerializeField] private WheelCollider rearWheelCollider;

    [Space, Header("AUDIO CLIPS")]
    [SerializeField] AudioClip idleClip;
    [SerializeField] AudioClip windNoiseClip;

    private Bike bike;

    private void Start()
    {
        bike = GetComponent<Bike>();

        float maxSpeed = bike.TopSpeed;
        ecs.maxTreshold = maxSpeed * 3; // Multiplied by 3 to get the low/medium load in ElectricCarSounds
        ecs.windNoiseClip = windNoiseClip;
        ecs.idleClip = idleClip;
    }

    private void Update()
    {
        if (ShouldBeMuted()) return;

        ecs.currentSpeed = bike.CurrentSpeed * 3.6f;

        if (bike.Input.Accelerate) // gas pedal is pressing
        {
            ecs.gasPedalPressing = true;
        }
        else if (!bike.Input.Accelerate && !bike.Input.Brake) // gas pedal is not pressing
        {
            ecs.gasPedalPressing = false;
            if (rearWheelCollider.motorTorque > 0)
                ecs.isReversing = false;
        }

        if (rearWheelCollider.motorTorque < 0) // car is reversing, play reverse sound
        {
            ecs.gasPedalPressing = true;
            ecs.isReversing = true;
        }
    }

    private bool ShouldBeMuted()
    {
        bool shouldBeMuted = !PlayerPrefsUtil.GetSoundsEnabled() || Time.timeScale == 0
            || StateManager.CurrentState is MainMenuState; //TODO: Remove currentState check when possible

        ecs.gameObject.SetActive(!shouldBeMuted);
        return shouldBeMuted;
    }
}
