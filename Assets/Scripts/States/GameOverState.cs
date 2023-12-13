using Assets.Scripts;
using Assets.Scripts.Managers;

public class GameOverState : BaseState
{
    public Bike Bike { get; private set; }

    public GameOverState(Bike bike)
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
