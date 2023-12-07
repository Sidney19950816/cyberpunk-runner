using UnityEngine;

namespace Assets.Scripts
{
    public struct InputData
    {
        public bool Accelerate;
        public bool Brake;
        public float TurnInput;
    }

    public interface IInput
    {
        InputData GenerateInput();
    }

    public class BaseInput : MonoBehaviour, IInput
    {
        public InputData GenerateInput()
        {
            if (SystemInfo.deviceType == DeviceType.Handheld || IsRemoteConnected())
            {
                return new InputData
                {
                    Accelerate = !Input.GetMouseButton(0),
                    Brake = Input.GetMouseButton(0),
                    TurnInput = Input.acceleration.x
                };
            }
            else
            {
                return new InputData
                {
                    Accelerate = !Input.GetMouseButton(0) || !Input.GetKey(KeyCode.Space),
                    Brake = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space),
                    TurnInput = Input.GetAxis("Horizontal")
                };
            }
        }

        private bool IsRemoteConnected()
        {
            #if UNITY_EDITOR
                return UnityEditor.EditorApplication.isRemoteConnected;
            #else
                return false;
            #endif
        }
    }
}
