using Assets.Scripts;
using Assets.Scripts.Managers;

public class GameOverState : BaseState
{
    public ArcadeBike Bike { get; private set; }

    public GameOverState(ArcadeBike bike)
    {
        Bike = bike;
    }

    public override void OnStateEnter()
    {
    }

    public override void OnStateExit()
    {
    }

    public void SetMainMenuState()
    {
        Bike.Player.CollectRokens();
        GameSceneManager.Instance.RestartScene();
    }

    public void OnWatchAdButtonClick()
    {
        GoogleMobileAdsController.Instance?.RewardedAdController?.ShowAd(() => {
            Bike.Player.CollectRokens(2);
            GameSceneManager.Instance.RestartScene();
        });
    }
}
