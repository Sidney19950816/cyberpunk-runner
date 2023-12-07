using System;
using UnityEngine;

namespace Assets.Scripts
{
    [DefaultExecutionOrder(100)]
    public class BikeAnimation : MonoBehaviour
    {
        [Serializable] public class Wheel
        {
            [Tooltip("A reference to the transform of the wheel.")]
            public Transform wheelTransform;
            [Tooltip("A reference to the WheelCollider of the wheel.")]
            public WheelCollider wheelCollider;
            
            Quaternion m_SteerlessLocalRotation;

            public void Setup() => m_SteerlessLocalRotation = wheelTransform.localRotation;

            public void StoreDefaultRotation() => m_SteerlessLocalRotation = wheelTransform.localRotation;
            public void SetToDefaultRotation() => wheelTransform.localRotation = m_SteerlessLocalRotation;
        }

        [Tooltip("What bike do we want to listen to?")]
        public ArcadeBike bikeController;

        [Space]
        [Tooltip("The damping for the appearance of steering compared to the input.  The higher the number the less damping.")]
        public float steeringAnimationDamping = 10f;

        [Space]
        [Tooltip("The maximum angle in degrees that the front wheels can be turned away from their default positions, when the Steering input is either 1 or -1.")]
        public float maxSteeringAngle;
        [Tooltip("Information referring to the front wheel of the bike.")]
        public Wheel frontWheel;
        [Tooltip("Information referring to the rear wheel of the bike.")]
        public Wheel rearWheel;

        float m_SmoothedSteeringInput;

        void Start()
        {
            frontWheel.Setup();
            rearWheel.Setup();
        }

        void FixedUpdate() 
        {
            m_SmoothedSteeringInput = Mathf.MoveTowards(m_SmoothedSteeringInput, bikeController.Input.TurnInput, 
                steeringAnimationDamping * Time.deltaTime);

            // Steer front wheel
            float rotationAngle = m_SmoothedSteeringInput * maxSteeringAngle;
            frontWheel.wheelCollider.steerAngle = rotationAngle;

            // Tilt the bike if the wheels are grounded
            if(bikeController.GroundPercent == 1)
            {
                transform.rotation = GetBikeRotation(rotationAngle * 2);
            }

            // Update position and rotation from WheelCollider
            UpdateWheelFromCollider(frontWheel);
            UpdateWheelFromCollider(rearWheel);
        }

        void LateUpdate()
        {
            // Update position and rotation from WheelCollider
            UpdateWheelFromCollider(frontWheel);
            UpdateWheelFromCollider(rearWheel);
        }

        void UpdateWheelFromCollider(Wheel wheel)
        {
            wheel.wheelCollider.GetWorldPose(out Vector3 position, out Quaternion rotation);
            wheel.wheelTransform.position = position;
            wheel.wheelTransform.rotation = rotation;
        }

        private Quaternion GetBikeRotation(float rotationAngle)
        {
            float zRot = bikeController.Rigidbody.velocity.magnitude > 1 ? -rotationAngle : 0;
            Quaternion toRot = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, zRot);

            if (bikeController.Rigidbody.velocity.magnitude > 1)
                return Quaternion.Slerp(transform.rotation, toRot, Time.fixedDeltaTime * bikeController.LocalSpeed());
            else
                return Quaternion.Slerp(transform.rotation, toRot, Time.fixedDeltaTime * 5); // Should this be a constant number?
        }
    }
}
