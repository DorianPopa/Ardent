using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppController : MonoBehaviour
{
    public enum Scenes { Preload = 0, Login = 1, ARPlacer = 2}

    public string objPathToLoad = "";

    void Start()
    {
        LoadScene("LoginMenuScene");
    }

    public void LoadVisualizerScene(string objPath)
    {
        objPathToLoad = objPath;
        LoadScene("ObjectPlacerScene");
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneSingleCoroutine(sceneName));
    }

    public void LoadNextScene()
    {
        StartCoroutine(LoadNextSceneCoroutine());
    }

    private IEnumerator UnloadCurrentSceneCoroutine()
    {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

        while (!asyncUnload.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator LoadSceneSingleCoroutine(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator LoadNextSceneCoroutine()
    {
        int nextSceneToLoad = SceneManager.GetActiveScene().buildIndex + 1;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneToLoad);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        print($"Scene {nextSceneToLoad} was loaded");
    }
}
