using UnityEngine.UI;

namespace PG
{
    public class MobileUI :Singleton<MobileUI>
    {

#pragma warning disable 0649

        public CustomButton LeftButton;
        public CustomButton RightButton;
        public CustomButton AccelerometrButton;
        public CustomButton BrakeButton;
        public CustomButton BoostButton;
        public CustomButton HandBrakeButton;
        public Button LeftTurnButton;
        public Button RightTurnButton;
        public Button AlarmButton;
        public Button LightsButton;
        public Button TrailerButton;
        public Button RepairButton;
        public Button ChangeViewButton;
        public Button ResetCarButton;
        public Button RestartSceneButton;
        public Button SelectNextCarButton;
        public Button GameOverViewButton;

        private CarStateUI CarStateUI;

#pragma warning restore 0649

        void Start ()
        {
            gameObject.SetActive (true);
            CarStateUI = FindObjectOfType<CarStateUI>();
        }

        public static bool LeftButtonPressed => Instance != null && Instance.LeftButton.Pressed;
        public static bool RightButtonPressed => Instance != null && Instance.RightButton.Pressed;
        public static bool AccelerometrButtonPressed => Instance != null && Instance.AccelerometrButton.Pressed;
        public static bool BrakeButtonPressed => Instance != null && Instance.BrakeButton.Pressed;
        public static bool BoostButtonPressed => Instance != null && Instance.BoostButton.IsPointerInside;
        public static bool HandBrakeButtonPressed => Instance != null && Instance.HandBrakeButton.Pressed;

        public static void GameOverViewState(bool state)
        {
            Instance.GameOverViewButton.SetActive(state);
        }
    }
}
