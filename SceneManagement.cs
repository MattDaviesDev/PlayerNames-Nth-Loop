using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public static SceneManagement instance;

    public GameObject sceneSwitcher;
    public CanvasGroup sceneSwitchCanvas;

    Coroutine switchingScene = null;

    public AudioSource startGameButtonSound;

    public bool showHints = true;
    public bool playSFX = true;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(sceneSwitcher);
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(sceneSwitcher);
            DontDestroyOnLoad(this);
        }
    }

    public void SwitchScene(int sceneID, CanvasGroup fadeOutCanvas)
    {
        if (switchingScene == null)
        {
            switchingScene = StartCoroutine(Switch(sceneID, fadeOutCanvas));
        }
    }

    IEnumerator Switch(int sceneID, CanvasGroup fadeOutCanvas)
    {
        sceneSwitcher.SetActive(true);
        float t = 0f;
        do
        {
            t += Time.unscaledDeltaTime / 0.5f;
            fadeOutCanvas.alpha = Mathf.Lerp(1f, 0f, t);
            sceneSwitchCanvas.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        } while (t <= 1f);
        yield return new WaitForSecondsRealtime(1f);
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneID);
        asyncOperation.allowSceneActivation = false;
        while (!asyncOperation.isDone)
        {
            if (asyncOperation.progress >= 0.9f)
            {
                asyncOperation.allowSceneActivation = true;
            }
            yield return null;
        }
        t = 0f;
        do
        {
            t += Time.unscaledDeltaTime / 0.5f;
            sceneSwitchCanvas.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        } while (t <= 1f);
        sceneSwitcher.SetActive(false);
        switchingScene = null;
        Time.timeScale = 1f;
    }


}
