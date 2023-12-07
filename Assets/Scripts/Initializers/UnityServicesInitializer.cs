using Assets.Scripts.Managers;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;

public class UnityServicesInitializer
{
    public static async Task InitializeAsync()
    {
        var options = new InitializationOptions();
        // Set environment based on build conditions
        SetEnvironmentBasedOnBuild(options);
        await UnityServices.InitializeAsync(options);
    }

    private static void SetEnvironmentBasedOnBuild(InitializationOptions options)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        options.SetEnvironmentName(UtilConstants.DEVELOPMENT_ENVIRONMENT_NAME);
#else
        options.SetEnvironmentName(UtilConstants.PRODUCTION_ENVIRONMENT_NAME);
#endif
    }
}