using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PG
{
    /// <summary>
    /// For user multiplatform control. This way of implementing the input is chosen to be able to implement control of several players for one device.
    /// </summary>
    public class UserInput :InitializePlayer, ICarControl
    {
        public float HorizontalChangeSpeed = 10;            //To simulate the use of a keyboard trigger.
        public float CameraRotateSensitivity = 1;
        public float Horizontal { get; private set; }
        public float Acceleration { get; private set; }
        public float BrakeReverse { get; private set; }
        public float Pitch { get; protected set; }
        public bool HandBrake { get; private set; }
        public bool Boost { get; private set; }
        public Vector2 PrevMousePos { get; private set; }
        public Vector2 ViewDelta { get; private set; }
        public bool ManualCameraRotation { get; private set; }

        public event System.Action OnChangeViewAction;

        float TargetHorizontal;
        int FingerIDRotateCamera;

        CarLighting CarLighting;

        static List<UserInput> Inputs = new();
        event System.Action OnDestroyAction;

        private void Awake ()
        {
            Inputs.Add (this);
        }

        private void Start ()
        {
            if (MobileUI.Instance != null)
            {
                if (CarLighting)
                {
                    MobileUI.Instance.LightsButton.onClick.AddListener (CarLighting.SwitchMainLights);
                    MobileUI.Instance.LeftTurnButton.onClick.AddListener (OnLeftTurn);
                    MobileUI.Instance.RightTurnButton.onClick.AddListener (OnRightTurn);
                    MobileUI.Instance.AlarmButton.onClick.AddListener (OnAlarm);
                }
                if (Car)
                {
                    MobileUI.Instance.ResetCarButton.onClick.AddListener (Vehicle.ResetVehicle);
                    MobileUI.Instance.TrailerButton.onClick.AddListener (Car.TryConnectDisconnectTrailer);
                }
            }
        }

        private void OnDestroy ()
        {
            Inputs.Remove (this);
            OnDestroyAction.SafeInvoke ();

            if (MobileUI.Instance != null)
            {
                if (CarLighting)
                {
                    MobileUI.Instance.LightsButton.onClick.RemoveListener (CarLighting.SwitchMainLights);
                    MobileUI.Instance.LeftTurnButton.onClick.RemoveListener (OnLeftTurn);
                    MobileUI.Instance.RightTurnButton.onClick.RemoveListener (OnRightTurn);
                    MobileUI.Instance.AlarmButton.onClick.RemoveListener (OnAlarm);
                }
                if (Car)
                {
                    MobileUI.Instance.ResetCarButton.onClick.RemoveListener (Vehicle.ResetVehicle);
                    MobileUI.Instance.TrailerButton.onClick.RemoveListener (Car.TryConnectDisconnectTrailer);
                }
            }
        }

        public override bool Initialize (VehicleController car)
        {
            base.Initialize (car);

            var inputIndex = Inputs.IndexOf(this);

            if (Car)
            {
                CarLighting = Car.GetComponent<CarLighting> ();
                var aiControl = Car.GetComponent<ICarControl>();
                if (aiControl == null || !(aiControl is PositioningAIControl))
                {
                    Car.CarControl = this;
                }
            }

            return IsInitialized;
        }

        private void Update ()
        {
            #if UNITY_EDITOR
            if(UnityEditor.EditorApplication.isRemoteConnected)
            {
                TargetHorizontal = Input.acceleration.x * 1.2f;
            }
            #endif
            if (SystemInfo.deviceType == DeviceType.Handheld)
            {
                TargetHorizontal = Input.acceleration.x * 1.2f;
            }
            if (SystemInfo.deviceType == DeviceType.Desktop)
            {
                BikeController.Instance.GetComponent<BikeController>().Steer.EnableSteerLimit = Car.CurrentBrake <= 0;
            }

            Horizontal = Mathf.MoveTowards (Horizontal, TargetHorizontal, Time.deltaTime * HorizontalChangeSpeed);

            if (!Application.isMobilePlatform)
            {
                ManualCameraRotation = Input.GetKey (KeyCode.Mouse0);

                ViewDelta = ((Vector2)Input.mousePosition - PrevMousePos) * CameraRotateSensitivity;
                PrevMousePos = (Vector2)Input.mousePosition;
            }
            else
            {
                if (ManualCameraRotation)
                {
                    var hasFingerID = false;
                    for (var i = 0; i < Input.touchCount; i++)
                    {
                        if (FingerIDRotateCamera == Input.touches[i].fingerId)
                        {
                            ViewDelta = (Input.touches[i].position - PrevMousePos) * CameraRotateSensitivity;
                            PrevMousePos = Input.touches[i].position;
                            hasFingerID = true;
                            break;
                        }
                    }
                    if (!hasFingerID)
                    {
                        ManualCameraRotation = false;
                    }
                }
                else
                {
                    for (var i = 0; i < Input.touchCount; i++)
                    {
                        if (Input.touches[i].phase == TouchPhase.Began)
                        {
                            if (!IsPointerOverUIObject (Input.touches[i].position))
                            {
                                ManualCameraRotation = true;
                                FingerIDRotateCamera = Input.touches[i].fingerId;
                                ViewDelta = Vector2.zero;
                                PrevMousePos = Input.touches[i].position;
                            }
                            break;
                        }
                    }
                }
            }

            var vertical = Input.GetAxis ("Vertical");

            Acceleration = MobileUI.AccelerometrButtonPressed ? 1 : vertical.Clamp (0, 1);
            BrakeReverse = MobileUI.BrakeButtonPressed ? 1 : vertical.Clamp (-1, 0).Abs ();

            TargetHorizontal = MobileUI.LeftButtonPressed ? -1 : MobileUI.RightButtonPressed ? 1 : Input.GetAxis ("Horizontal");

            Pitch = Input.GetAxis ("Pitch");

            if (Car)
            {
                if (Input.GetButtonDown ("NextGear"))
                    Car.NextGear ();
                if (Input.GetButtonDown ("PrevGear"))
                    Car.PrevGear ();
                if (Input.GetButtonDown ("ConnectTrailer"))
                    Car.TryConnectDisconnectTrailer ();
                if (Input.GetButtonDown ("ResetCar"))
                    Vehicle.ResetVehicle ();
                if (Input.GetButtonDown ("ChangeView"))
                    OnChangeViewAction.SafeInvoke ();
                HandBrake = Input.GetButton ("HandBrake") || MobileUI.HandBrakeButtonPressed;
                Boost = Input.GetButton ("Boost") || MobileUI.BoostButtonPressed;
            }

            if (CarLighting)
            {
                if (Input.GetButtonDown ("SwitchLights"))
                    CarLighting.SwitchMainLights ();
                if (Input.GetButtonDown ("LeftTurn"))
                    OnLeftTurn ();
                if (Input.GetButtonDown ("RightTurn"))
                    OnRightTurn ();
                if (Input.GetButtonDown ("AlarmTurn"))
                    OnAlarm ();
            }
        }

        void OnLeftTurn ()
        {
            CarLighting.TurnsEnable (TurnsStates.Left);
        }

        void OnRightTurn ()
        {
            CarLighting.TurnsEnable (TurnsStates.Right);
        }

        void OnAlarm ()
        {
            CarLighting.TurnsEnable (TurnsStates.Alarm);
        }

        bool IsPointerOverUIObject (Vector2 position)
        {
            var eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = position;
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll (eventDataCurrentPosition, results);
            return results.Count > 0;
        }
    }
}
