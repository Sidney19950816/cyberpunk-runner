using UnityEngine;

public static class PlayerPrefsUtil
{
    private const string MUSIC_KEY = "MUSIC";
    private const string SOUNDS_KEY = "SOUNDS";
    private const string VIBRATION_KEY = "VIBRATION";

    public static void SetMusicEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(MUSIC_KEY, enabled ? 1 : 0);
    }

    public static bool GetMusicEnabled()
    {
        return PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
    }

    public static void SetSoundsEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(SOUNDS_KEY, enabled ? 1 : 0);
    }

    public static bool GetSoundsEnabled()
    {
        return PlayerPrefs.GetInt(SOUNDS_KEY, 1) == 1;
    }

    public static void SetVibrationEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(VIBRATION_KEY, enabled ? 1 : 0);
    }

    public static bool GetVibrationEnabled()
    {
        return PlayerPrefs.GetInt(VIBRATION_KEY, 1) == 1;
    }

    public static void SetTermsAccepted()
    {
        PlayerPrefs.SetInt("TermsAccepted", 1);
    }

    public static bool GetTermsAccepted()
    {
        return PlayerPrefs.GetInt("TermsAccepted", 0) == 1;
    }

    public static void SetTutorialCompleted()
    {
        PlayerPrefs.SetInt("TutorialCompleted", 1);
    }

    public static bool GetTutorialCompleted()
    {
        return PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;
    }

    public static void SetTutorialGameStateCompleted()
    {
        PlayerPrefs.SetInt("TutorialGameStateCompleted", 1);
    }

    public static bool GetTutorialGameStateCompleted()
    {
        return PlayerPrefs.GetInt("TutorialGameStateCompleted", 0) == 1;
    }
}
