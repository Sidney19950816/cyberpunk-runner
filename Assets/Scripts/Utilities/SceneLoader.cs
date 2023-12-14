using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private SceneLoader_UI ui_SceneLoader;
    [SerializeField] private GameObject Background;
    void Start()
    {
        DontDestroyOnLoad(Background);
        LoadScene(1);
    }
    private void LoadScene(int sceneIndex)
    {
        ui_SceneLoader.ShowBar(true);
        StartCoroutine(LoadAsync(sceneIndex));
    }
    private IEnumerator LoadAsync(int sceneIndex)
    {
        var operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;
        float progress = 0f;
        while (!operation.isDone)
        {
            progress = Mathf.MoveTowards(progress,operation.progress,Time.deltaTime);
            ui_SceneLoader.ShowProgressValue(progress);
            if (progress >= .5f)
            {
                progress = 1f;
                ui_SceneLoader.ShowProgressValue(.55f);
                operation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
