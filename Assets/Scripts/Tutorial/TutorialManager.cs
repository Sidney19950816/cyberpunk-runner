using System.Collections;
using Assets.Scripts.Managers;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialState
    {
        None,
        TiltPhone,
        TapToBreak,
        FightState,
        TapToShoot,
        GameState,
        EarnRokens,
        MainMenu,
        MotorbikeBattery,
        Completed
    }

    private TutorialWindow tutorialWindow;

    private TutorialState currentState = TutorialState.None;
    private Coroutine moveToNextStateCoroutine;

    private void Start()
    {
        if (Application.isEditor)
            return;

        StartTutorial();
    }

    private void Update()
    {
        UpdateState();
    }

    private void UpdateState()
    {
        if (PlayerPrefsUtil.GetTutorialCompleted())
            return;

        switch (currentState)
        {
            case TutorialState.TiltPhone:
                if (Mathf.Abs(Input.acceleration.x) > 0.5f)
                {
                    MoveToNextState(5);
                    Time.timeScale = 1;
                }
                break;
            case TutorialState.TapToBreak:
                Time.timeScale = 0;
                if (Input.GetMouseButtonDown(0))
                {
                    MoveToNextState();
                    Time.timeScale = 1;
                }
                break;
            case TutorialState.FightState:
                if (StateManager.CurrentState is FightState)
                {
                    MoveToNextState();
                    Time.timeScale = 0;
                }
                break;
            case TutorialState.TapToShoot:
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.transform.CompareTag(UtilConstants.ENEMY))
                        {
                            MoveToNextState();
                            Time.timeScale = 0.05f;
                        }
                    }
                }
                break;
            case TutorialState.GameState:
                if (StateManager.CurrentState is GameState)
                {
                    MoveToNextState(5);
                }
                break;
            case TutorialState.EarnRokens:
                Time.timeScale = 0;
                if (Input.GetMouseButtonDown(0))
                {
                    Time.timeScale = 1;
                    MoveToNextState();
                    PlayerPrefsUtil.SetTutorialGameStateCompleted();
                }
                break;
            case TutorialState.MainMenu:
                if (StateManager.CurrentState is MainMenuState)
                {
                    MoveToNextState();
                }
                break;
            case TutorialState.MotorbikeBattery:
                if (Input.GetMouseButtonDown(0))
                {
                    MoveToNextState();
                }
                break;
            case TutorialState.Completed:
                EndTutorial();
                break;
        }
    }

    private void StartTutorial()
    {
        if (PlayerPrefsUtil.GetTutorialCompleted())
            return;

        tutorialWindow = Instantiate(Resources.Load<TutorialWindow>($"Tutorial/TutorialWindow"), GameSceneManager.Instance.Canvas.transform);
        Time.timeScale = 0;

        if (PlayerPrefsUtil.GetTutorialGameStateCompleted())
        {
            SetState(TutorialState.MainMenu);
        }
        else
        {
            StateManager.OnGameStateChanged += SetStartState;
        }
    }

    private void SetStartState(BaseState state)
    {
        if (state is GameState)
        {
            Time.timeScale = 0;
            SetState(TutorialState.TiltPhone);
            StateManager.OnGameStateChanged -= SetStartState;
        }
    }

    private void MoveToNextState()
    {
        currentState++;
        if (currentState == TutorialState.Completed)
        {
            EndTutorial();
        }
        else
        {
            UpdateInstructionText();
        }
    }

    private void SetState(TutorialState newState)
    {
        currentState = newState;
        UpdateInstructionText();
    }

    private void UpdateInstructionText()
    {
        tutorialWindow.gameObject.SetActive(true);
        tutorialWindow.SetState(currentState, GetInstructionText(currentState));
    }

    private string GetInstructionText(TutorialState state)
    {
        switch (state)
        {
            case TutorialState.TiltPhone:
                return $"Tilt the phone left and right to control your motorbike";
            case TutorialState.TapToBreak:
                return $"Tap anywhere on screen for braking";
            case TutorialState.TapToShoot:
                return $"Tap on enemies for shooting";
            case TutorialState.EarnRokens:
                return $"Earn more Rokens simply by driving more distance";
            case TutorialState.MotorbikeBattery:
                return $"Your motorbike needs time for charging, you have maximum 5 charges";
            case TutorialState.Completed:
                return $"Tutorial Complete";
            default:
                return string.Empty;
        }
    }

    private void EndTutorial()
    {
        Time.timeScale = 1;
        PlayerPrefsUtil.SetTutorialCompleted();
        Destroy(tutorialWindow.gameObject);
        tutorialWindow = null;
    }

    private void MoveToNextState(float delay)
    {
        if(moveToNextStateCoroutine == null)
            moveToNextStateCoroutine = StartCoroutine(MoveToNextStateCoroutine(delay));
    }

    private IEnumerator MoveToNextStateCoroutine(float delay)
    {
        tutorialWindow.gameObject.SetActive(false);
        yield return new WaitForSeconds(delay);

        MoveToNextState();
        moveToNextStateCoroutine = null;
    }

    //TODO
    //When the player reaches to the Tutorial MainMenu State, save the currentState value to playerprefs
    //Then when the player dies and goes to main menu, get the value if the value exists MoveToNextState to complete the tutorial
}
