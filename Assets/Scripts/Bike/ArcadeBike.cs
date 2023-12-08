using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.VFX;
using System.Linq;
using System.Collections;
using Assets.Scripts.Events;

namespace Assets.Scripts
{
    public class ArcadeBike : MonoBehaviour, IDamageable, IResettable
    {
        [System.Serializable]
        public class StatPowerup
        {
            public Stats modifiers;
            public string PowerUpID;
            public float ElapsedTime;
            public float MaxTime;
        }

        [System.Serializable]
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

        [System.Serializable]
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

        public Rigidbody Rigidbody { get; private set; }
        public InputData Input     { get; private set; }
        public float AirPercent    { get; private set; }
        public float GroundPercent { get; private set; }

        [Header("CAMERA TRANSFORM")]
        [SerializeField] private Transform _followCam;
        [SerializeField] private Transform _aimCam;

        [Space, Header("PLAYER")]
        [SerializeField] private Player player;

        public Transform FollowCam => _followCam;
        public Transform AimCam => _aimCam;

        public Player Player => player;

        [Space]
        public Stats baseStats = new Stats
        {
            TopSpeed            = 10f,
            Acceleration        = 5f,
            AccelerationCurve   = 4f,
            Braking             = 10f,
            ReverseAcceleration = 5f,
            ReverseSpeed        = 5f,
            Steer               = 5f,
            CoastingDrag        = 4f,
            Grip                = .95f,
            AddedGravity        = 1f,
        };

        [Header("Bike Visual")] 
        public List<GameObject> m_VisualWheels;

        [Header("Bike Physics")]
        [Tooltip("The transform that determines the position of the bike's mass.")]
        public Transform CenterOfMass;

        [Range(0.0f, 20.0f), Tooltip("Coefficient used to reorient the bike in the air. The higher the number, the faster the bike will readjust itself along the horizontal plane.")]
        public float AirborneReorientationCoefficient = 3.0f;

        [Header("Drifting")]
        [Range(0.01f, 1.0f), Tooltip("The grip value when drifting.")]
        public float DriftGrip = 0.4f;
        [Range(0.0f, 10.0f), Tooltip("Additional steer when the bike is drifting.")]
        public float DriftAdditionalSteer = 5.0f;
        [Range(1.0f, 30.0f), Tooltip("The higher the angle, the easier it is to regain full grip.")]
        public float MinAngleToFinishDrift = 10.0f;
        [Range(0.01f, 0.99f), Tooltip("Mininum speed percentage to switch back to full grip.")]
        public float MinSpeedPercentToFinishDrift = 0.5f;
        [Range(1.0f, 20.0f), Tooltip("The higher the value, the easier it is to control the drift steering.")]
        public float DriftControl = 10.0f;
        [Range(0.0f, 20.0f), Tooltip("The lower the value, the longer the drift will last without trying to control it by steering.")]
        public float DriftDampening = 10.0f;

        [Header("Suspensions")]
        [Tooltip("The maximum extension possible between the bike's body and the wheels.")]
        [Range(0.0f, 1.0f)]
        public float SuspensionHeight = 0.2f;
        [Range(10.0f, 100000.0f), Tooltip("The higher the value, the stiffer the suspension will be.")]
        public float SuspensionSpring = 20000.0f;
        [Range(0.0f, 5000.0f), Tooltip("The higher the value, the faster the bike will stabilize itself.")]
        public float SuspensionDamp = 500.0f;
        [Tooltip("Vertical offset to adjust the position of the wheels relative to the bike's body.")]
        [Range(-1.0f, 1.0f)]
        public float WheelsPositionVerticalOffset = 0.0f;

        [Header("Physical Wheels")]
        [Tooltip("The physical representations of the bike's wheels.")]
        public WheelCollider FrontWheel;
        public WheelCollider RearWheel;

        [Tooltip("Which layers the wheels will detect.")]
        public LayerMask GroundLayers = 1 << 0 | 1 << 11; // Physics.DefaultRaycastLayers;

        // the input sources that can control the bike
        IInput m_Inputs;

        const float k_NullInput = 0.01f;
        const float k_NullSpeed = 0.01f;
        Vector3 m_VerticalReference = Vector3.up;

        // Drift params
        public bool WantsToDrift { get; private set; } = false;
        public bool IsDrifting { get; private set; } = false;
        float m_CurrentGrip = 1.0f;
        float m_DriftTurningPower = 0.0f;
        float m_PreviousGroundPercent = 1.0f;
        readonly List<(GameObject trailRoot, WheelCollider wheel, TrailRenderer trail)> m_DriftTrailInstances = new List<(GameObject, WheelCollider, TrailRenderer)>();
        readonly List<(WheelCollider wheel, float horizontalOffset, float rotation, ParticleSystem sparks)> m_DriftSparkInstances = new List<(WheelCollider, float, float, ParticleSystem)>();

        // can the bike move?
        bool m_CanMove = true;
        List<StatPowerup> m_ActivePowerupList = new List<StatPowerup>();
        public Stats BikeStats;
        RokenStats _rokenStats;

        Quaternion m_LastValidRotation;
        Vector3 m_LastValidPosition;
        Vector3 m_LastCollisionNormal;
        bool m_HasCollision;
        bool m_InAir = false;

        public void AddPowerup(StatPowerup statPowerup) => m_ActivePowerupList.Add(statPowerup);
        public void SetCanMove(bool move) => m_CanMove = move;
        public float GetMaxSpeed() => Mathf.Max(BikeStats.TopSpeed, BikeStats.ReverseSpeed);
        public float CurrentSpeed { get; private set; }

        public Action<float> OnHealthUpdate;

        public void Init(MotorbikeData bikeData)
        {
            Dictionary<UserMotorbikeUpgrade, float> upgrades = new Dictionary<UserMotorbikeUpgrade, float>();
            foreach(MotorbikeUpgradeData upgradeData in bikeData.upgrades)
            {
                upgrades.Add(upgradeData.enumId, upgradeData.value);
            }

            Stats bStats = new Stats
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
            BikeStats = bStats;

            RokenStats rStats = new RokenStats
            {
                MinRokenSpeed = bikeData.minRokenSpeedRequirement,
                BaseRokenValue = bikeData.baseRokenAmount,
                TopSpeedRokenMultiplicator = bikeData.topSpeedRokenMultiplier,
                RokensPerRamp = bikeData.rampRokenBonus,
            };

            _rokenStats = rStats;
        }

        void UpdateSuspensionParams(WheelCollider wheel)
        {
            wheel.suspensionDistance = SuspensionHeight;
            wheel.center = new Vector3(0.0f, WheelsPositionVerticalOffset, 0.0f);
            JointSpring spring = wheel.suspensionSpring;
            spring.spring = SuspensionSpring;
            spring.damper = SuspensionDamp;
            wheel.suspensionSpring = spring;
            wheel.motorTorque = 0.00001f; // This is needed to keep the wheel active so it doesn't get stuck to the ground
        }

        void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            m_Inputs = GetComponent<IInput>();

            UpdateSuspensionParams(FrontWheel);
            UpdateSuspensionParams(RearWheel);

            m_CurrentGrip = baseStats.Grip;

            // apply our physics property
            Rigidbody.inertiaTensorRotation = Quaternion.identity;

            InvokeRepeating("UpdateCurrencyValue", 1, 1);
        }

        void FixedUpdate()
        {
            // CHEAT
            if (UnityEngine.Input.GetKeyUp(KeyCode.H))
            {
                EconomyManager.Instance.GetCurrencyAmount(UtilConstants.CURRENCY_ROKEN_ID);
            }

            UpdateSuspensionParams(FrontWheel);
            UpdateSuspensionParams(RearWheel);

            GatherInputs();

            // apply our physics properties
            Rigidbody.centerOfMass = transform.InverseTransformPoint(CenterOfMass.position);

            int groundedCount = 0;
            if (FrontWheel.isGrounded && FrontWheel.GetGroundHit(out WheelHit hit))
                groundedCount++;
            if (RearWheel.isGrounded && RearWheel.GetGroundHit(out hit))
                groundedCount++;

            // calculate how grounded and airborne we are
            GroundPercent = groundedCount / 2.0f;
            AirPercent = 1 - GroundPercent;

            // apply Bike physics
            if (m_CanMove)
            {
                MoveBike(Input.Accelerate, Input.Brake, Input.TurnInput);
            }
            GroundAirbourne();

            m_PreviousGroundPercent = GroundPercent;
        }

        void GatherInputs()
        {
            Input = m_Inputs.GenerateInput();
            WantsToDrift = Input.Brake && Vector3.Dot(Rigidbody.velocity, transform.forward) > 0.0f;
        }

        void GroundAirbourne()
        {
            // while in the air, fall faster
            if (AirPercent >= 0.5f)
            {
                Rigidbody.velocity += Physics.gravity * Time.fixedDeltaTime * BikeStats.AddedGravity;
            }
        }

        public void Reset()
        {
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = euler.z = 0f;
            transform.rotation = Quaternion.Euler(euler);
        }

        public float LocalSpeed()
        {
            if (m_CanMove)
            {
                float dot = Vector3.Dot(transform.forward, Rigidbody.velocity);
                if (Mathf.Abs(dot) > 0.1f)
                {
                    float speed = Rigidbody.velocity.magnitude;
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

        void OnCollisionEnter(Collision collision) => m_HasCollision = true;
        void OnCollisionExit(Collision collision) => m_HasCollision = false;

        void OnCollisionStay(Collision collision)
        {
            m_HasCollision = true;
            m_LastCollisionNormal = Vector3.zero;
            float dot = -1.0f;

            foreach (var contact in collision.contacts)
            {
                if (Vector3.Dot(contact.normal, Vector3.up) > dot)
                    m_LastCollisionNormal = contact.normal;
            }
        }

        void MoveBike(bool accelerate, bool brake, float turnInput)
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

            CurrentSpeed = Rigidbody.velocity.magnitude;
            float accelRampT = CurrentSpeed / maxSpeed;
            float multipliedAccelerationCurve = BikeStats.AccelerationCurve * accelerationCurveCoeff;
            float accelRamp = Mathf.Lerp(multipliedAccelerationCurve, 1, accelRampT * accelRampT);

            bool isBraking = (localVelDirectionIsFwd && brake) || (!localVelDirectionIsFwd && accelerate);

            // if we are braking (moving reverse to where we are going)
            // use the braking accleration instead
            float finalAccelPower = isBraking ? BikeStats.Braking : accelPower;

            float finalAcceleration = finalAccelPower * accelRamp;

            // Restrict the bike turning if it is not moving
            turnInput = Rigidbody.velocity.magnitude > 1 ? turnInput : 0;
            float turningPower = IsDrifting ? m_DriftTurningPower : turnInput * BikeStats.Steer;

            // apply inputs to forward/backward
            Quaternion turnAngle = Quaternion.AngleAxis(turningPower, transform.up);
            Vector3 fwd = turnAngle * transform.forward;
            Vector3 movement = fwd * accelInput * finalAcceleration * ((m_HasCollision || GroundPercent > 0.0f) ? 1.0f : 0.0f);

            // forward movement
            bool wasOverMaxSpeed = CurrentSpeed >= maxSpeed;

            // if over max speed, cannot accelerate faster.
            if (wasOverMaxSpeed && !isBraking) 
                movement *= 0.0f;

            Vector3 newVelocity = Rigidbody.velocity + movement * Time.fixedDeltaTime;
            newVelocity.y = Rigidbody.velocity.y;

            //  clamp max speed if we are on ground
            if (GroundPercent > 0.0f && !wasOverMaxSpeed)
            {
                newVelocity = Vector3.ClampMagnitude(newVelocity, maxSpeed);
            }

            // coasting is when we aren't touching accelerate
            if (Mathf.Abs(accelInput) < k_NullInput && GroundPercent > 0.0f)
            {
                newVelocity = Vector3.MoveTowards(newVelocity, new Vector3(0, Rigidbody.velocity.y, 0), Time.fixedDeltaTime * BikeStats.CoastingDrag);
            }

            // Acceleration velocity
            Rigidbody.velocity = newVelocity; //Commented

            // Drift
            if (GroundPercent > 0.0f)
            {
                if (m_InAir)
                {
                    m_InAir = false;
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
                if (GroundPercent >= 0.0f && m_PreviousGroundPercent < 0.1f)
                {
                    Vector3 flattenVelocity = Vector3.ProjectOnPlane(Rigidbody.velocity, m_VerticalReference).normalized;
                    if (Vector3.Dot(flattenVelocity, transform.forward * Mathf.Sign(accelInput)) < Mathf.Cos(MinAngleToFinishDrift * Mathf.Deg2Rad))
                    {
                        IsDrifting = true;
                        m_CurrentGrip = DriftGrip;
                        m_DriftTurningPower = 0.0f;
                    }
                }

                // Drift Management
                if (!IsDrifting)
                {
                    if ((WantsToDrift || isBraking) && CurrentSpeed > maxSpeed * MinSpeedPercentToFinishDrift)
                    {
                        IsDrifting = true;
                        m_DriftTurningPower = turningPower + (Mathf.Sign(turningPower) * DriftAdditionalSteer);
                        m_CurrentGrip = DriftGrip;

                        //ActivateDriftVFX(true);
                    }
                }

                if (IsDrifting)
                {
                    float turnInputAbs = Mathf.Abs(turnInput);
                    if (turnInputAbs < k_NullInput)
                        m_DriftTurningPower = Mathf.MoveTowards(m_DriftTurningPower, 0.0f, Mathf.Clamp01(DriftDampening * Time.fixedDeltaTime));

                    // Update the turning power based on input
                    float driftMaxSteerValue = BikeStats.Steer + DriftAdditionalSteer;
                    m_DriftTurningPower = Mathf.Clamp(m_DriftTurningPower + (turnInput * Mathf.Clamp01(DriftControl * Time.fixedDeltaTime)), -driftMaxSteerValue, driftMaxSteerValue);

                    bool facingVelocity = Vector3.Dot(Rigidbody.velocity.normalized, transform.forward * Mathf.Sign(accelInput)) > Mathf.Cos(MinAngleToFinishDrift * Mathf.Deg2Rad);

                    bool canEndDrift = true;
                    if (isBraking)
                        canEndDrift = false;
                    else if (!facingVelocity)
                        canEndDrift = false;
                    else if (turnInputAbs >= k_NullInput && CurrentSpeed > maxSpeed * MinSpeedPercentToFinishDrift)
                        canEndDrift = false;

                    if (canEndDrift || CurrentSpeed < k_NullSpeed)
                    {
                        // No Input, and car aligned with speed direction => Stop the drift
                        IsDrifting = false;
                        m_CurrentGrip = BikeStats.Grip;
                    }

                }

                // rotate our velocity based on current steer value Commented
                Rigidbody.velocity = Quaternion.AngleAxis(turningPower * Mathf.Sign(localVel.z) * velocitySteering * m_CurrentGrip * Time.fixedDeltaTime, transform.up) * Rigidbody.velocity;
            }
            else
            {
                m_InAir = true;
            }

            bool validPosition = false;
            if (Physics.Raycast(transform.position + (transform.up * 0.1f), -transform.up, out RaycastHit hit, 3.0f, GroundLayers)) // Layer: Road (10) / Ground (11)
            {
                Vector3 lerpVector = (m_HasCollision && m_LastCollisionNormal.y > hit.normal.y) ? m_LastCollisionNormal : hit.normal;
                m_VerticalReference = Vector3.Slerp(m_VerticalReference, lerpVector, Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime * (GroundPercent > 0.0f ? 10.0f : 1.0f)));    // Blend faster if on ground
            }
            else
            {
                Vector3 lerpVector = (m_HasCollision && m_LastCollisionNormal.y > 0.0f) ? m_LastCollisionNormal : Vector3.up;
                m_VerticalReference = Vector3.Slerp(m_VerticalReference, lerpVector, Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime));
            }

            validPosition = GroundPercent > 0.7f && !m_HasCollision && Vector3.Dot(m_VerticalReference, Vector3.up) > 0.9f;

            // Airborne / Half on ground management
            if (GroundPercent < 0.7f)
            {
                Rigidbody.angularVelocity = new Vector3(0.0f, Rigidbody.angularVelocity.y * 0.98f, 0.0f);
                Vector3 finalOrientationDirection = Vector3.ProjectOnPlane(transform.forward, m_VerticalReference);
                finalOrientationDirection.Normalize();
                if (finalOrientationDirection.sqrMagnitude > 0.0f)
                {
                    Rigidbody.MoveRotation(Quaternion.Lerp(Rigidbody.rotation, Quaternion.LookRotation(finalOrientationDirection, m_VerticalReference), Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime)));
                }
            }
            else if (validPosition)
            {
                m_LastValidPosition = transform.position;
                m_LastValidRotation.eulerAngles = new Vector3(0.0f, transform.rotation.y, 0.0f);
            }
        }

        private void UpdateCurrencyValue() //Invoke Repeating
        {
            if (Rigidbody.velocity.magnitude * 3.6f > _rokenStats.MinRokenSpeed)
            {
                float currentSpeed = Mathf.Round(Rigidbody.velocity.magnitude * 3.6f);

                int valueToAdd = Mathf.RoundToInt(_rokenStats.BaseRokenValue + ((currentSpeed - _rokenStats.MinRokenSpeed) / 10));

                if (currentSpeed >= BikeStats.TopSpeed)
                    Player.Score.Add(valueToAdd * _rokenStats.TopSpeedRokenMultiplicator);
                else
                    Player.Score.Add(valueToAdd);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            float dotProduct = Vector3.Dot(transform.forward.normalized, (other.transform.position - transform.position).normalized);
            if (dotProduct > 0.5f && !other.isTrigger)
            {
                if (other.gameObject.GetComponent<IgnoreDamage>() != null || other.GetComponent<Triggers.BikeTrigger>() != null)
                    return;

                other.isTrigger = true;
                if(!isHit)
                {
                    TakeDamage();
                    isHit = true;
                }
            }
        }

        private List<Renderer> renderers;
        private bool isHit;

        private IEnumerator FlashRenderers(float interval, float totalTime)
        {
            if (renderers == null)
            {
                renderers = GetComponentsInChildren<Renderer>().ToList();
            }

            bool isVisible = false;
            float startTime = Time.time;
            while (Time.time - startTime < totalTime)
            {
                foreach (Renderer r in renderers)
                {
                    r.enabled = isVisible;
                }

                yield return new WaitForSeconds(interval);

                isVisible = !isVisible;
            }

            foreach (Renderer r in renderers)
            {
                r.enabled = true;
            }

            isHit = false;
        }

        public void TakeDamage(float amount = 0)
        {
            float normalizedSpeed = Mathf.Clamp01((Rigidbody.velocity.magnitude * 3.6f) / BikeStats.TopSpeed);
            float healthLoss = normalizedSpeed * RemoteConfigManager.Instance.MotorbikeDamageFactor;
            BikeStats.Health -= healthLoss;
            BikeStats.Health = Mathf.Clamp(BikeStats.Health, 0, BikeStats.MaxHealth);
            Rigidbody.velocity *= 0.5f;

            OnHealthUpdate?.Invoke(BikeStats.Health);

            if (BikeStats.Health <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(FlashRenderers(0.2f, 3));
            }
        }

        public void Die()
        {
            StateManager.SetState(new GameOverState(this));
        }
    }
}
