using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneLoader_UI : MonoBehaviour
{
    [SerializeField] private Slider loadingBarProgress;
    public void ShowBar(bool value)
    {
        loadingBarProgress.gameObject.SetActive(value);
    }
    public void ShowProgressValue(float progress)
    {
        loadingBarProgress.value = progress;
    }
}
