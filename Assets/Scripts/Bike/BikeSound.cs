using Assets.Scripts;
using UnityEngine;
using SkrilStudio;
using Assets.Scripts.Managers;

public class BikeSound : MonoBehaviour
{
    [SerializeField] ElectricCarSounds ecs;
    [SerializeField] private ArcadeBike arcadeBike;
    [SerializeField] private WheelCollider rearWheelCollider;

    [Space, Header("AUDIO CLIPS")]
    [SerializeField] AudioClip idleClip;
    [SerializeField] AudioClip windNoiseClip;

    private void Start()
    {
        float maxSpeed = arcadeBike.GetMaxSpeed() * 3.6f; // TODO: Remove 3.6f when KMH/MPH are implemented
        ecs.maxTreshold = maxSpeed * 3; // Multiplied by 3 to get the low/medium load in ElectricCarSounds
        ecs.windNoiseClip = windNoiseClip;
        ecs.idleClip = idleClip;
    }

    private void Update()
    {
        if (ShouldBeMuted()) return;

        ecs.currentSpeed = arcadeBike.CurrentSpeed * 3.6f;

        if (arcadeBike.Input.Accelerate) // gas pedal is pressing
        {
            ecs.gasPedalPressing = true;
        }
        else if (!arcadeBike.Input.Accelerate && !arcadeBike.Input.Brake) // gas pedal is not pressing
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

        ecs.SetActive(!shouldBeMuted);
        return shouldBeMuted;
    }
}
