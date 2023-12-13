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
            
            Quaternion _steerlessLocalRotation;

            public void Setup() => _steerlessLocalRotation = wheelTransform.localRotation;

            public void StoreDefaultRotation() => _steerlessLocalRotation = wheelTransform.localRotation;
            public void SetToDefaultRotation() => wheelTransform.localRotation = _steerlessLocalRotation;
        }

        [Space]
        [Tooltip("The damping for the appearance of steering compared to the input.  The higher the number the less damping.")]
        [SerializeField] private float _steeringAnimationDamping = 10f;

        [Space]
        [Tooltip("The maximum angle in degrees that the front wheels can be turned away from their default positions, when the Steering input is either 1 or -1.")]
        [SerializeField] private float _maxSteeringAngle;
        [Tooltip("Information referring to the front wheel of the bike.")]
        [SerializeField] private Wheel _frontWheel;
        [Tooltip("Information referring to the rear wheel of the bike.")]
        [SerializeField] private Wheel _rearWheel;

        private Bike _bike;
        private float _smoothedSteeringInput;

        private void Start()
        {
            _bike = GetComponent<Bike>();

            _frontWheel.Setup();
            _rearWheel.Setup();
        }

        private void FixedUpdate() 
        {
            _smoothedSteeringInput = Mathf.MoveTowards(_smoothedSteeringInput, _bike.Input.TurnInput, 
                _steeringAnimationDamping * Time.deltaTime);

            // Steer front wheel
            float rotationAngle = _smoothedSteeringInput * _maxSteeringAngle;
            _frontWheel.wheelCollider.steerAngle = rotationAngle;

            // Tilt the bike if the wheels are grounded
            if(_bike.GroundPercent == 1)
            {
                transform.rotation = GetBikeRotation(rotationAngle * 2);
            }

            // Update position and rotation from WheelCollider
            UpdateWheelFromCollider(_frontWheel);
            UpdateWheelFromCollider(_rearWheel);
        }

        private void LateUpdate()
        {
            // Update position and rotation from WheelCollider
            UpdateWheelFromCollider(_frontWheel);
            UpdateWheelFromCollider(_rearWheel);
        }

        private void UpdateWheelFromCollider(Wheel wheel)
        {
            wheel.wheelCollider.GetWorldPose(out Vector3 position, out Quaternion rotation);
            wheel.wheelTransform.position = position;
            wheel.wheelTransform.rotation = rotation;
        }

        private Quaternion GetBikeRotation(float rotationAngle)
        {
            float zRot = _bike.Rigidbody.velocity.magnitude > 1 ? -rotationAngle : 0;
            Quaternion toRot = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, zRot);

            if (_bike.Rigidbody.velocity.magnitude > 1)
                return Quaternion.Slerp(transform.rotation, toRot, Time.fixedDeltaTime * _bike.LocalSpeed());
            else
                return Quaternion.Slerp(transform.rotation, toRot, Time.fixedDeltaTime * 5); // Should this be a constant number?
        }
    }
}
