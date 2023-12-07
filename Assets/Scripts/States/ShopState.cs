using Assets.Scripts.Managers;

public class ShopState : BaseState
{
    public ShopState()
    {
    }

    public override void OnStateEnter()
    {
    }

    public override void OnStateExit()
    {
    }

    public void OnBackButtonPressed()
    {
        StateManager.SetState(new MainMenuState());
    }
}
