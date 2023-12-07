using Assets.Scripts.Managers;

public class DailyRewardsState : BaseState
{
    public DailyRewardsState()
    {
    }

    public override void OnStateEnter()
    {
    }

    public override void OnStateExit()
    {
    }

    public void OnBackButtonClick()
    {
        StateManager.SetState(new MainMenuState());
    }
}
