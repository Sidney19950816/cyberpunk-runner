using Facebook.Unity;

public class FacebookInitializer
{
    public static void InitializeFacebook()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
        }
        else
        {
            FB.Init(() => {
                FB.ActivateApp();
            });
        }
    }
}