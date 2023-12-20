using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.VFX;
using System.Linq;
using System.Collections;
using Assets.Scripts.Events;
using Assets.Scripts.World;

namespace Assets.Scripts
{
    public class Bike : MonoBehaviour
    {
        private const float NULL_INPUT = 0.01f;
        private const float NULL_SPEED = 0.01f;

        [Serializable]
        public struct Stats
        {
            [Header("Movement Settings")]
            [Min(0.001f), Tooltip("Top speed attainable when moving forward.")]
            public float TopSpeed;

            [Tooltip("How quickly the bike reaches top speed.")]
            public float Acceleration;

            [Min(0.001f), Tooltip("Top speed attainable when moving backward.")]
            public float ReverseSpeed;

            [Tooltip("How quickly the bike reaches top speed, when moving backward.")]
            public float ReverseAcceleration;

            [Tooltip("How quickly the bike starts accelerating from 0. A higher number means it accelerates faster sooner.")]
            [Range(0.2f, 1)]
            public float AccelerationCurve;

            [Tooltip("How quickly the bike slows down when the brake is applied.")]
            public float Braking;

            [Tooltip("How quickly the bike will reach a full stop when no inputs are made.")]
            public float CoastingDrag;

            [Range(0.0f, 1.0f)]
            [Tooltip("The amount of side-to-side friction.")]
            public float Grip;

            [Tooltip("How tightly the bike can turn left or right.")]
            public float Steer;

            [Tooltip("Additional gravity for when the bike is in the air.")]
            public float AddedGravity;

            [Min(0), Tooltip("Maximum health of the bike.")]
            public float MaxHealth;

            [Min(0), Tooltip("Maximum health of the bike.")]
            public float Health;

            // allow for stat adding for powerups.
            public static Stats operator +(Stats a, Stats b)
            {
                return new Stats
                {
                    Acceleration        = a.Acceleration + b.Acceleration,
                    AccelerationCurve   = a.AccelerationCurve + b.AccelerationCurve,
                    Braking             = a.Braking + b.Braking,
                    CoastingDrag        = a.CoastingDrag + b.CoastingDrag,
                    AddedGravity        = a.AddedGravity + b.AddedGravity,
                    Grip                = a.Grip + b.Grip,
                    ReverseAcceleration = a.ReverseAcceleration + b.ReverseAcceleration,
                    ReverseSpeed        = a.ReverseSpeed + b.ReverseSpeed,
                    TopSpeed            = a.TopSpeed + b.TopSpeed,
                    Steer               = a.Steer + b.Steer,
                };
            }
        }

        [Serializable]
        public struct RokenStats
        {
            [Header("Minimum Speed at which the player can obtain Rokens (In-game currency)")]
            public float MinRokenSpeed;

            [Header("Base roken value that the player can get every X seconds")]
            public int BaseRokenValue;

            [Header("Multiplicate Rokens if the player is driving at the top speed")]
            public int TopSpeedRokenMultiplicator;

            [Header("Rokens value that the player can get every time he jumps over a ramp")]
            public int RokensPerRamp;
        }

        [Header("Bike Visual")]
        [SerializeField] private List<GameObject> _visualWheels;

        [Space, Header("Bike Physics")]
        [Tooltip("The transform that determines the position of the bike's mass.")]
        [SerializeField] private Transform _centerOfMass;
        [SerializeField] private Rigidbody _rigidBody;

        [Range(0.0f, 20.0f), Tooltip("Coefficient used to reorient the bike in the air. The higher the number, the faster the bike will readjust itself along the horizontal plane.")]
        [SerializeField] private float _airborneCoefficient = 3.0f;

        [Space, Header("Physical Wheels")]
        [Tooltip("The physical representations of the bike's wheels.")]
        [SerializeField] private WheelCollider _frontWheel;
        [SerializeField] private WheelCollider _rearWheel;

        [Tooltip("Which layers the wheels will detect.")]
        [SerializeField] private LayerMask _groundLayers = 1 << 0 | 1 << 11; // Physics.DefaultRaycastLayers;

        [Space, Header("Drifting")]
        [Range(0.01f, 1.0f), Tooltip("The grip value when drifting.")]
        [SerializeField] private float _driftGrip = 0.4f;
        [Range(0.0f, 10.0f), Tooltip("Additional steer when the bike is drifting.")]
        [SerializeField] private float _driftAdditionalSteer = 5.0f;
        [Range(1.0f, 30.0f), Tooltip("The higher the angle, the easier it is to regain full grip.")]
        [SerializeField] private float _minAngleToFinishDrift = 10.0f;
        [Range(0.01f, 0.99f), Tooltip("Mininum speed percentage to switch back to full grip.")]
        [SerializeField] private float _minSpeedPercentToFinishDrift = 0.5f;
        [Range(1.0f, 20.0f), Tooltip("The higher the value, the easier it is to control the drift steering.")]
        [SerializeField] private float _driftControl = 10.0f;
        [Range(0.0f, 20.0f), Tooltip("The lower the value, the longer the drift will last without trying to control it by steering.")]
        [SerializeField] private float _driftDampening = 10.0f;

        [Space, Header("Suspensions")]
        [Tooltip("The maximum extension possible between the bike's body and the wheels.")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float _suspensionHeight = 0.2f;
        [Range(10.0f, 100000.0f), Tooltip("The higher the value, the stiffer the suspension will be.")]
        [SerializeField] private float _suspensionSpring = 20000.0f;
        [Range(0.0f, 5000.0f), Tooltip("The higher the value, the faster the bike will stabilize itself.")]
        [SerializeField] private float _suspensionDamp = 500.0f;
        [Tooltip("Vertical offset to adjust the position of the wheels relative to the bike's body.")]
        [Range(-1.0f, 1.0f)]
        [SerializeField] private float wheelsPositionVerticalOffset = 0.0f;

        [Space, Header("Camera Properties")]
        [SerializeField] private Transform _followCam;
        [SerializeField] private Transform _aimCam;

        [Space, Header("Player Reference")]
        [SerializeField] private Player _player;

        [Space, Header("Fight Properties")]
        [Range(-100f, 100f)]
        [SerializeField] private float _fightOffset;

        private RokenStats _rokenStats;

        private IInput _input;
        private IHealth _health;

        private Vector3 _verticalReference = Vector3.up;

        private List<Renderer> _renderers;

        // Drift params
        private bool _wantsToDrift = false;
        private bool _isDrifting = false;
        private float _currentGrip = 1.0f;
        private float _driftTurningPower = 0.0f;
        private float _previousGroundPercent = 1.0f;

        // Last valid properties
        private Quaternion _lastValidRotation;
        private Vector3 _lastValidPosition;
        private Vector3 _lastCollisionNormal;

        // Flags to track different states of the bike 
        private bool _hasCollision;
        private bool _inAir = false;
        private bool _canMove = true;
        private bool _isHit = false;

        // Bike physics stats
        private float _currentMagnitude;
        private float _airPercent;
        private float _groundPercent;

        private BikeReset _bikeReset;
        private Collider _triggerCollider;

        public Stats BikeStats;

        public Rigidbody Rigidbody => _rigidBody;
        public Transform FollowCam => _followCam;
        public Transform AimCam => _aimCam;
        public Player Player => _player;

        public Collider TriggerCollider => _triggerCollider;

        public InputData Input => _input.GenerateInput();
        public IHealth Health => _health;

        public float GroundPercent => _groundPercent;
        public float CurrentSpeed => _currentMagnitude * 3.6f;

        public float TopSpeed => GetMaxSpeed() * 3.6f;

        public float GetMaxSpeed() => Mathf.Max(BikeStats.TopSpeed, BikeStats.ReverseSpeed);

        public float FightOffset => _fightOffset;

        public void Initialize(MotorbikeData bikeData)
        {
            Dictionary<UserMotorbikeUpgrade, float> upgrades = new Dictionary<UserMotorbikeUpgrade, float>();
            foreach(MotorbikeUpgradeData upgradeData in bikeData.upgrades)
            {
                upgrades.Add(upgradeData.enumId, upgradeData.value);
            }

            BikeStats = new Stats
            {
                TopSpeed = upgrades[UserMotorbikeUpgrade.Speed] / 3.6f,
                Acceleration = 4,
                AccelerationCurve = 0.5f,
                Braking = upgrades[UserMotorbikeUpgrade.Brake],
                ReverseAcceleration = 3f,
                ReverseSpeed = 10f,
                Steer = upgrades[UserMotorbikeUpgrade.Handling] / 2,
                CoastingDrag = 5f,
                Grip = .95f,
                AddedGravity = 1f,
                MaxHealth = upgrades[UserMotorbikeUpgrade.Health],
                Health = upgrades[UserMotorbikeUpgrade.Health],
            };

            _rokenStats = new RokenStats
            {
                MinRokenSpeed = bikeData.minRokenSpeedRequirement,
                BaseRokenValue = bikeData.baseRokenAmount,
                TopSpeedRokenMultiplicator = bikeData.topSpeedRokenMultiplier,
                RokensPerRamp = bikeData.rampRokenBonus,
            };

            _health = GetComponent<IHealth>()
                .With(h => h.InitializeHealth(BikeStats.MaxHealth));
        }

        public float LocalSpeed()
        {
            if (_canMove)
            {
                float dot = Vector3.Dot(transform.forward, Rigidbody.velocity);
                if (Mathf.Abs(dot) > 0.1f)
                {
                    float speed = _currentMagnitude;
                    return dot < 0 ? -(speed / BikeStats.ReverseSpeed) : (speed / BikeStats.TopSpeed);
                }
                return 0f;
            }
            else
            {
                // use this value to play bike sound when it is waiting the race start countdown.
                return Input.Accelerate ? 1.0f : 0.0f;
            }
        }

        public void OnChunkEnter(Chunk chunk)
        {
            _bikeReset.SetChunkContext(chunk);
        }

        private void UpdateSuspensionParams(WheelCollider wheel)
        {
            wheel.suspensionDistance = _suspensionHeight;
            wheel.center = new Vector3(0.0f, wheelsPositionVerticalOffset, 0.0f);
            JointSpring spring = wheel.suspensionSpring;
            spring.spring = _suspensionSpring;
            spring.damper = _suspensionDamp;
            wheel.suspensionSpring = spring;
            wheel.motorTorque = 0.00001f; // This is needed to keep the wheel active so it doesn't get stuck to the ground
        }

        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _input = GetComponent<IInput>();
            _bikeReset = GetComponent<BikeReset>();
            _triggerCollider = GetComponent<Collider>();

            UpdateSuspensionParams(_frontWheel);
            UpdateSuspensionParams(_rearWheel);

            //_currentGrip = baseStats.Grip;

            // apply our physics property
            Rigidbody.inertiaTensorRotation = Quaternion.identity;

            InvokeRepeating("UpdateCurrencyValue", 1, 1);
        }

        private void FixedUpdate()
        {
            // CHEAT
            if (UnityEngine.Input.GetKeyUp(KeyCode.H))
            {
                EconomyManager.Instance.GetCurrencyAmount(UtilConstants.CURRENCY_ROKEN_ID);
            }

            UpdateSuspensionParams(_frontWheel);
            UpdateSuspensionParams(_rearWheel);

            GatherInputs();

            // apply our physics properties
            Rigidbody.centerOfMass = transform.InverseTransformPoint(_centerOfMass.position);

            int groundedCount = 0;
            if (_frontWheel.isGrounded && _frontWheel.GetGroundHit(out WheelHit hit))
                groundedCount++;
            if (_rearWheel.isGrounded && _rearWheel.GetGroundHit(out hit))
                groundedCount++;

            // calculate how grounded and airborne we are
            _groundPercent = groundedCount / 2.0f;
            _airPercent = 1 - _groundPercent;

            // apply Bike physics
            if (_canMove)
            {
                MoveBike(Input.Accelerate, Input.Brake, Input.TurnInput);
            }
            GroundAirbourne();

            _previousGroundPercent = _groundPercent;
        }

        private void GatherInputs()
        {
            _wantsToDrift = Input.Brake && Vector3.Dot(Rigidbody.velocity, transform.forward) > 0.0f;
        }

        private void GroundAirbourne()
        {
            // while in the air, fall faster
            if (_airPercent >= 0.5f)
            {
                Rigidbody.velocity += Physics.gravity * Time.fixedDeltaTime * BikeStats.AddedGravity;
            }
        }

        private void OnCollisionEnter(Collision collision) => _hasCollision = true;
        private void OnCollisionExit(Collision collision) => _hasCollision = false;

        private void OnCollisionStay(Collision collision)
        {
            _hasCollision = true;
            _lastCollisionNormal = Vector3.zero;
            float dot = -1.0f;

            foreach (var contact in collision.contacts)
            {
                if (Vector3.Dot(contact.normal, Vector3.up) > dot)
                    _lastCollisionNormal = contact.normal;
            }
        }

        private void MoveBike(bool accelerate, bool brake, float turnInput)
        {
            float accelInput = 1 - (brake ? 1.0f : 0.0f);

            // manual acceleration curve coefficient scalar
            float accelerationCurveCoeff = 5;
            Vector3 localVel = transform.InverseTransformVector(Rigidbody.velocity);

            bool accelDirectionIsFwd = accelInput >= 0;
            bool localVelDirectionIsFwd = localVel.z >= 0;

            // use the max speed for the direction we are going--forward or reverse.
            float maxSpeed = localVelDirectionIsFwd ? BikeStats.TopSpeed : BikeStats.ReverseSpeed;
            float accelPower = accelDirectionIsFwd ? BikeStats.Acceleration : BikeStats.ReverseAcceleration;

            _currentMagnitude = Rigidbody.velocity.magnitude;
            float accelRampT = _currentMagnitude / maxSpeed;
            float multipliedAccelerationCurve = BikeStats.AccelerationCurve * accelerationCurveCoeff;
            float accelRamp = Mathf.Lerp(multipliedAccelerationCurve, 1, accelRampT * accelRampT);

            bool isBraking = (localVelDirectionIsFwd && brake) || (!localVelDirectionIsFwd && accelerate);

            // if we are braking (moving reverse to where we are going)
            // use the braking accleration instead
            float finalAccelPower = isBraking ? BikeStats.Braking : accelPower;

            float finalAcceleration = finalAccelPower * accelRamp;

            // Restrict the bike turning if it is not moving
            turnInput = _currentMagnitude > 1 ? turnInput : 0;
            float turningPower = _isDrifting ? _driftTurningPower : turnInput * BikeStats.Steer;

            // apply inputs to forward/backward
            Quaternion turnAngle = Quaternion.AngleAxis(turningPower, transform.up);
            Vector3 fwd = turnAngle * transform.forward;
            Vector3 movement = fwd * accelInput * finalAcceleration * ((_hasCollision || _groundPercent > 0.0f) ? 1.0f : 0.0f);

            // forward movement
            bool wasOverMaxSpeed = _currentMagnitude >= maxSpeed;

            // if over max speed, cannot accelerate faster.
            if (wasOverMaxSpeed && !isBraking) 
                movement *= 0.0f;

            Vector3 newVelocity = Rigidbody.velocity + movement * Time.fixedDeltaTime;
            newVelocity.y = Rigidbody.velocity.y;

            //  clamp max speed if we are on ground
            if (_groundPercent > 0.0f && !wasOverMaxSpeed)
            {
                newVelocity = Vector3.ClampMagnitude(newVelocity, maxSpeed);
            }

            // coasting is when we aren't touching accelerate
            if (Mathf.Abs(accelInput) < NULL_INPUT && _groundPercent > 0.0f)
            {
                newVelocity = Vector3.MoveTowards(newVelocity, new Vector3(0, Rigidbody.velocity.y, 0), Time.fixedDeltaTime * BikeStats.CoastingDrag);
            }

            // Acceleration velocity
            Rigidbody.velocity = newVelocity; //Commented

            // Drift
            if (_groundPercent > 0.0f)
            {
                if (_inAir)
                {
                    _inAir = false;
                    //Instantiate(JumpVFX, transform.position, Quaternion.identity);
                }

                // manual angular velocity coefficient
                float angularVelocitySteering = 0.4f;
                float angularVelocitySmoothSpeed = 20f;

                // turning is reversed if we're going in reverse and pressing reverse
                if (!localVelDirectionIsFwd && !accelDirectionIsFwd) 
                    angularVelocitySteering *= -1.0f;

                var angularVel = Rigidbody.angularVelocity;

                // move the Y angular velocity towards our target
                angularVel.y = Mathf.MoveTowards(angularVel.y, turningPower * angularVelocitySteering, Time.fixedDeltaTime * angularVelocitySmoothSpeed);

                // apply the angular velocity
                Rigidbody.angularVelocity = angularVel;

                // rotate rigidbody's velocity as well to generate immediate velocity redirection
                // manual velocity steering coefficient
                float velocitySteering = 25f;

                // If the bikes lands with a forward not in the velocity direction, we start the drift
                if (_groundPercent >= 0.0f && _previousGroundPercent < 0.1f)
                {
                    Vector3 flattenVelocity = Vector3.ProjectOnPlane(Rigidbody.velocity, _verticalReference).normalized;
                    if (Vector3.Dot(flattenVelocity, transform.forward * Mathf.Sign(accelInput)) < Mathf.Cos(_minAngleToFinishDrift * Mathf.Deg2Rad))
                    {
                        _isDrifting = true;
                        _currentGrip = _driftGrip;
                        _driftTurningPower = 0.0f;
                    }
                }

                // Drift Management
                if (!_isDrifting)
                {
                    if ((_wantsToDrift || isBraking) && _currentMagnitude > maxSpeed * _minSpeedPercentToFinishDrift)
                    {
                        _isDrifting = true;
                        _driftTurningPower = turningPower + (Mathf.Sign(turningPower) * _driftAdditionalSteer);
                        _currentGrip = _driftGrip;

                        //ActivateDriftVFX(true);
                    }
                }

                if (_isDrifting)
                {
                    float turnInputAbs = Mathf.Abs(turnInput);
                    if (turnInputAbs < NULL_INPUT)
                        _driftTurningPower = Mathf.MoveTowards(_driftTurningPower, 0.0f, Mathf.Clamp01(_driftDampening * Time.fixedDeltaTime));

                    // Update the turning power based on input
                    float driftMaxSteerValue = BikeStats.Steer + _driftAdditionalSteer;
                    _driftTurningPower = Mathf.Clamp(_driftTurningPower + (turnInput * Mathf.Clamp01(_driftControl * Time.fixedDeltaTime)), -driftMaxSteerValue, driftMaxSteerValue);

                    bool facingVelocity = Vector3.Dot(Rigidbody.velocity.normalized, transform.forward * Mathf.Sign(accelInput)) > Mathf.Cos(_minAngleToFinishDrift * Mathf.Deg2Rad);

                    bool canEndDrift = true;
                    if (isBraking)
                        canEndDrift = false;
                    else if (!facingVelocity)
                        canEndDrift = false;
                    else if (turnInputAbs >= NULL_INPUT && _currentMagnitude > maxSpeed * _minSpeedPercentToFinishDrift)
                        canEndDrift = false;

                    if (canEndDrift || _currentMagnitude < NULL_SPEED)
                    {
                        // No Input, and car aligned with speed direction => Stop the drift
                        _isDrifting = false;
                        _currentGrip = BikeStats.Grip;
                    }

                }

                // rotate our velocity based on current steer value Commented
                Rigidbody.velocity = Quaternion.AngleAxis(turningPower * Mathf.Sign(localVel.z) * velocitySteering * _currentGrip * Time.fixedDeltaTime, transform.up) * Rigidbody.velocity;
            }
            else
            {
                _inAir = true;
            }

            bool validPosition = false;
            if (Physics.Raycast(transform.position + (transform.up * 0.1f), -transform.up, out RaycastHit hit, 3.0f, _groundLayers)) // Layer: Road (10) / Ground (11)
            {
                Vector3 lerpVector = (_hasCollision && _lastCollisionNormal.y > hit.normal.y) ? _lastCollisionNormal : hit.normal;
                _verticalReference = Vector3.Slerp(_verticalReference, lerpVector, Mathf.Clamp01(_airborneCoefficient * Time.fixedDeltaTime * (_groundPercent > 0.0f ? 10.0f : 1.0f)));    // Blend faster if on ground
            }
            else
            {
                Vector3 lerpVector = (_hasCollision && _lastCollisionNormal.y > 0.0f) ? _lastCollisionNormal : Vector3.up;
                _verticalReference = Vector3.Slerp(_verticalReference, lerpVector, Mathf.Clamp01(_airborneCoefficient * Time.fixedDeltaTime));
            }

            validPosition = _groundPercent > 0.7f && !_hasCollision && Vector3.Dot(_verticalReference, Vector3.up) > 0.9f;

            // Airborne / Half on ground management
            if (_groundPercent < 0.7f)
            {
                Rigidbody.angularVelocity = new Vector3(0.0f, Rigidbody.angularVelocity.y * 0.98f, 0.0f);
                Vector3 finalOrientationDirection = Vector3.ProjectOnPlane(transform.forward, _verticalReference);
                finalOrientationDirection.Normalize();
                if (finalOrientationDirection.sqrMagnitude > 0.0f)
                {
                    Rigidbody.MoveRotation(Quaternion.Lerp(Rigidbody.rotation, Quaternion.LookRotation(finalOrientationDirection, _verticalReference), Mathf.Clamp01(_airborneCoefficient * Time.fixedDeltaTime)));
                }
            }
            else if (validPosition)
            {
                _lastValidPosition = transform.position;
                _lastValidRotation.eulerAngles = new Vector3(0.0f, transform.rotation.y, 0.0f);
            }
        }

        private void UpdateCurrencyValue() //Invoke Repeating
        {
            if (CurrentSpeed > _rokenStats.MinRokenSpeed)
            {
                int valueToAdd = Mathf.RoundToInt(_rokenStats.BaseRokenValue + ((CurrentSpeed - _rokenStats.MinRokenSpeed) / 10));

                if (CurrentSpeed >= BikeStats.TopSpeed)
                    Player.Score.Add(valueToAdd * _rokenStats.TopSpeedRokenMultiplicator);
                else
                    Player.Score.Add(valueToAdd);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            float dotProduct = CalculateDotProduct(other.transform.position);
            if (IsHitValid(dotProduct, other))
            {
                HandleValidHit(other);
            }
        }

        private float CalculateDotProduct(Vector3 otherPosition)
        {
            return Vector3.Dot(transform.forward.normalized, (otherPosition - transform.position).normalized);
        }

        private bool IsHitValid(float dotProduct, Collider other)
        {
            return dotProduct > 0.5f && !other.isTrigger &&
                   other.gameObject.GetComponent<IgnoreDamage>() == null &&
                   other.GetComponent<Triggers.BikeTrigger>() == null;
        }

        private void HandleValidHit(Collider other)
        {
            other.isTrigger = true;
            if (!_isHit)
            {
                ApplyDamage();
                _isHit = true;
            }
        }

        private void ApplyDamage()
        {
            Health.TakeDamage(GetHitDamageAmount());
            Rigidbody.velocity *= 0.5f;

            if (Health.Current > 0)
                StartCoroutine(FlashRenderers(0.2f, 3));
        }

        private float GetNormalizedSpeed()
            => Mathf.Clamp01(CurrentSpeed / BikeStats.TopSpeed);

        private float GetHitDamageAmount()
            => GetNormalizedSpeed() * RemoteConfigManager.Instance.MotorbikeDamageFactor;

        private IEnumerator FlashRenderers(float interval, float totalTime)
        {
            if (_renderers == null)
            {
                _renderers = GetComponentsInChildren<Renderer>().ToList();
            }

            bool isVisible = false;
            float startTime = Time.time;
            while (Time.time - startTime < totalTime)
            {
                foreach (Renderer r in _renderers)
                {
                    r.enabled = isVisible;
                }

                yield return new WaitForSeconds(interval);

                isVisible = !isVisible;
            }

            foreach (Renderer r in _renderers)
            {
                r.enabled = true;
            }

            _isHit = false;
        }
    }
}
