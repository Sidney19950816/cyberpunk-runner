using Assets.Scripts.Managers;
using System.Threading.Tasks;
using Unity.Services.Analytics;

public class GameInitializationManager
{
    private readonly GameSceneManager gameSceneManager;

    public GameInitializationManager(GameSceneManager gameSceneManager)
    {
        this.gameSceneManager = gameSceneManager;
    }

    public async Task InitializeGameAsync()
    {
        await UnityServicesInitializer.InitializeAsync();
        await AuthenticationInitializer.InitializeAsync(gameSceneManager);
        await ServiceManager.InitializeServices();
        FirebaseInitializer.InitializeFirebase();
        FacebookInitializer.InitializeFacebook();
        AnalyticsService.Instance.StartDataCollection();
        StateManager.SetState(new MainMenuState());
    }
}