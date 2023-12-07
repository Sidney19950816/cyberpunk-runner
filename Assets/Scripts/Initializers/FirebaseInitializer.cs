using Firebase;
using System;
using UnityEngine;

public class FirebaseInitializer
{
    public static void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                FirebaseApp app = FirebaseApp.DefaultInstance;
                Firebase.Analytics.FirebaseAnalytics
                    .LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLogin);

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                Debug.LogError(String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }
}