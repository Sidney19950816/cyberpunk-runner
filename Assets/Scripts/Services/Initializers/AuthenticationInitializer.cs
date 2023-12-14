using Assets.Scripts.Managers;
using System.Threading.Tasks;

public class AuthenticationInitializer
{
    public static async Task InitializeAsync(GameSceneManager gameSceneManager)
    {
        await gameSceneManager.Authentication.InitializeAsync();
    }
}