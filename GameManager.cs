using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Networking.Match;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    float gameTime = 0f;

    public int resetCounter = 0;

    public int collectedCrystals = 0;

    public GameObject playerObject;


    public bool timeTravelUnlocked = false;

    [Header("UI Stuff")]
    public CanvasGroup rootCanvasGroup;
    public TextMeshProUGUI crystalCounter;
    public GameObject interactionHInt;
    public TextMeshPro interactHintText;
    public GameObject timeControlOnCooldown;
    public TextMeshProUGUI timeControlCooldownText;
    public Transform clockhand;
    public float timeBetweenHandMovement = 0.3f;
    public int numberOfHandMovements = 20;
    float timeControlCooldown = 5f;
    float tTimeControl = 0f;
    public bool timeControlReady = false;
    public GameObject pauseCanvas;
    public GameObject mainCanvasObject;
    public GameObject confirmQuitObject;

    Coroutine rotatingHandOnClock = null;

    [Header("Text Stuff")]
    public CanvasGroup tutorialMessageCanvas;
    public TextMeshProUGUI tutorialText;
    Coroutine showingMsg = null;


    public bool gameEnded = false;
    Coroutine ending = null;
    public CanvasGroup endCanvas;
    public TextMeshProUGUI crystalsStat;
    public TextMeshProUGUI resetsStat;
    public TextMeshProUGUI gameTimerStat;

    public float tLastCollect = 0f;
    public float timeToResetCollecTSound = 2f;
    public int recentCollections = 0;
    public float pitchIncrease = 0.1f;

    public PostProcessProfile profile;
    ColorGrading colorGrading;
    Vignette vignette;
    Grain grain;
    LensDistortion distortion;
    ChromaticAberration chrom;

    public GameObject endParticles;
    public TextMeshProUGUI endMessage;
    public string lossEndMessage = "";
    public string winEndMessage = "";


    public AudioMixerGroup startGroup;
    public AudioMixerGroup newGroup;
    public AudioSource firstTrack;
    public AudioSource secondTrack;
    public AudioSource thirdTrack;
    Coroutine blending = null;
    public sCollectilbe endCrystal;
    bool songChanged = false;
    public float xPosSongChangeVal;

    bool success = false;
    private bool isRestarting = false;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    void TimeControlOnCooldown()
    {
        string timer = tTimeControl <= 1f ? tTimeControl.ToString("F1") : ((int)tTimeControl).ToString();
        timeControlCooldownText.text = timer + "<size=50%>s";
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        crystalCounter.text = "x " + collectedCrystals;
        profile.TryGetSettings(out colorGrading);
        profile.TryGetSettings(out vignette);
        profile.TryGetSettings(out grain);
        profile.TryGetSettings(out distortion);
        profile.TryGetSettings(out chrom);
    }

    private void OnApplicationQuit()
    {
        ResetPostProcessing();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("QuickRestart") != 0f && !isRestarting)
        {
            RestartGame();
        }
        if (playerObject.transform.position.x > xPosSongChangeVal && !songChanged)
        {
            StartNewTrack(firstTrack, secondTrack, 5f);
            StartCoroutine(ChangeSaturationFirst());
            songChanged = true;
        }
        if (Input.GetButton("Pause") && !gameEnded && !sReset.instance.isResetting)
        {
            PauseGame(true);
        }
        if (!gameEnded)
        {
            gameTime += Time.deltaTime;
        }
        if (timeControlReady && timeTravelUnlocked)
        {
            if (rotatingHandOnClock == null)
            {
                StartClockMovement(false);
            }
            tTimeControl = timeControlCooldown;
            timeControlOnCooldown.SetActive(false);
        }
        else
        {
            timeControlOnCooldown.SetActive(true);
            if (rotatingHandOnClock != null)
            {
                StopCoroutine(rotatingHandOnClock);
                rotatingHandOnClock = null;
            }
            tTimeControl -= Time.deltaTime;
            tTimeControl = Mathf.Clamp(tTimeControl, 0f, timeControlCooldown);
            TimeControlOnCooldown();
            if (tTimeControl <= 0)
            {
                timeControlReady = true;
            }
        }
        if (recentCollections > 0)
        {
            tLastCollect += Time.deltaTime;
            if (tLastCollect >= timeToResetCollecTSound)
            {
                recentCollections = 0;
            }
        }
        else
        {
            tLastCollect = 0f;
        }
    }

    public void PauseGame(bool pause)
    {
        Cursor.visible = pause;
        Cursor.lockState = pause ? CursorLockMode.None : CursorLockMode.Locked;
        Time.timeScale = pause ? 0 : 1;
        pauseCanvas.SetActive(pause);
        mainCanvasObject.SetActive(!pause);
    }

    public void RestartGame()
    {
        isRestarting = true;
        SceneManagement.instance.SwitchScene(1, rootCanvasGroup);
    }

    public void QuitGamePressed(bool confirm)
    {
        if (confirm)
        {
            GameManager.instance.ReturnToMainMenu();
        }
        else
        {
            confirmQuitObject.SetActive(true);
        }
    }

    public void CancelQuit()
    {
        confirmQuitObject.SetActive(false);
    }

    public void IncreaseCrystalCounter()
    {
        collectedCrystals++;
        recentCollections++;
        crystalCounter.text = "x " + collectedCrystals;
    }

    public void ShowInteractionHint(string objectName, string color)
    {
        interactHintText.text = "Press 'E' to grab <color=" + color + ">" + objectName;
        interactionHInt.SetActive(true);
    }

    public void HideInteractionHint()
    {
        interactionHInt.SetActive(false);
    }

    public void StartClockMovement(bool rewinding)
    {
        if (rotatingHandOnClock == null)
        {
            rotatingHandOnClock = StartCoroutine(RotateClockHand(rewinding));
        }
    }

    public void OverrideClockHandMovement(bool rewinding)
    {
        if (rotatingHandOnClock != null)
        {
            StopCoroutine(rotatingHandOnClock);
            rotatingHandOnClock = null;
            rotatingHandOnClock = StartCoroutine(RotateClockHand(rewinding));
        }
    }

    public void UnlockTimeControl()
    {
        timeTravelUnlocked = true;
        timeControlOnCooldown.SetActive(false);
        timeControlCooldownText.gameObject.SetActive(true);
        timeControlReady = true;
        StartClockMovement(false);
    }

    IEnumerator RotateClockHand(bool rewinding)
    {
        int multipler = rewinding ? -1 : 1;
        do
        {
            yield return new WaitForSeconds(timeBetweenHandMovement);
            clockhand.rotation *= Quaternion.Euler(0, 0, -360 / numberOfHandMovements * multipler);
        } while (true);
    }

    public void NewTutorialMessage(string msg)
    {
        if (showingMsg == null)
        {
            showingMsg = StartCoroutine(ShowNewMessage(msg));
        }
    }

    IEnumerator ShowNewMessage(string msg)
    {
        float t = 0f;
        char[] outputStr = msg.ToCharArray();
        tutorialText.text = "";
        do
        {
            t += Time.deltaTime / 0.25f;
            tutorialMessageCanvas.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        } while (t <= 1f);

        for (int i = 0; i < msg.Length; i++)
        {
            t = 0f;
            if (outputStr[i] == '<')
            {
                string htmlCode = "";
                for (int j = i; j < msg.Length; j++)
                {
                    htmlCode += outputStr[j];
                    i++;
                    if (outputStr[j] == '>')
                    {
                        break;
                    }
                }
                tutorialText.text += htmlCode;
            }
            tutorialText.text += outputStr[i];
            do
            {
                t += Time.deltaTime;
                if (Input.GetMouseButtonDown(0))
                {
                    tutorialText.text = msg;
                    break;
                }
                yield return null;
            } while (t <= 0.025f);
        }
        yield return new WaitForSeconds(0.5f);
        do
        {
            if (Input.GetMouseButtonDown(0))
            {
                break;
            }
            yield return null;
        } while (true);
        t = 0f;
        do
        {
            t += Time.deltaTime / 0.25f;
            tutorialMessageCanvas.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        } while (t <= 1f);
        showingMsg = null;
    }

    public void ReturnToMainMenu()
    {
        ResetPostProcessing();
        if (success)
        {
            if (gameTime < PlayerPrefs.GetFloat("FastestRun", 0f) || PlayerPrefs.GetFloat("FastestRun", 0f) == 0f)
            {
                print(gameTime);
                PlayerPrefs.SetFloat("FastestRun", gameTime);
            }
        }
        PlayerPrefs.SetInt("Crystals", PlayerPrefs.GetInt("Crystals") + collectedCrystals);
        PlayerPrefs.SetInt("Resets", PlayerPrefs.GetInt("Resets") + resetCounter);
        PlayerPrefs.SetFloat("TimeInGame", PlayerPrefs.GetFloat("TimeInGame") + gameTime);
        PlayerPrefs.SetInt("Loops", PlayerPrefs.GetInt("Loops") + 1);
        SceneManagement.instance.SwitchScene(0, rootCanvasGroup);
    }

    public void EndGame(bool win)
    {
        if (win)
        {
            success = true;
            PlayerPrefs.SetInt("LoopBreaks", PlayerPrefs.GetInt("LoopBreaks") + 1);
            endMessage.text = winEndMessage;
        }
        else
        {
            endMessage.text = lossEndMessage;
        }
        if (ending == null)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            gameEnded = true;
            ending = StartCoroutine(FadeIntoEndCanvas());
        }
    }

    IEnumerator FadeIntoEndCanvas()
    {
        gameTimerStat.text = "Time in game: 00:00:00";
        crystalsStat.text = "Crystals collected: 0";
        resetsStat.text = "Times reset: 0";
        yield return new WaitForSeconds(2f);
        CanvasGroup[] childCG = rootCanvasGroup.GetComponentsInChildren<CanvasGroup>();
        for (int i = 0; i < childCG.Length; i++)
        {
            childCG[i].ignoreParentGroups = false;
        }
        float t = 0f;
        endCanvas.interactable = true;
        endCanvas.blocksRaycasts = true;
        do
        {
            t += Time.deltaTime / 2f;
            rootCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            endCanvas.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        } while (t <= 1f);
        StartCoroutine(IncreaseAllStats());
    }

    IEnumerator IncreaseAllStats()
    {
        float t = 0f;
        int crystals = 0;
        int resets = 0;
        float tTime = 0f;
        do
        {
            t += Time.deltaTime / 1.5f;
            tTime = Mathf.Lerp(0f, gameTime, sReset.SinusoidalLerp(t));
            crystals = (int)Mathf.Lerp(0, collectedCrystals, sReset.SinusoidalLerp(t));
            resets = (int)Mathf.Lerp(0, resetCounter, sReset.SinusoidalLerp(t));
            gameTimerStat.text = "Time in game: " + sMainMenu.SecondsToText(tTime);
            crystalsStat.text = "Crystals collected: " + crystals;
            resetsStat.text = "Times reset: " + resets;
            yield return null;
        } while (t <= 1f);
    }

    float startVal = 0f;

    public void BreakFromLoop(bool finishedGame)
    {
        if (finishedGame)
        {
            StartCoroutine(BrokenFromLoopPostProcessOff());
        }
        else
        {
            StartCoroutine(BrokenFromLoopPostProcessOn());
        }
    }

    IEnumerator BrokenFromLoopPostProcessOn()
    {
        float t = 0f;
        startVal = colorGrading.saturation.value;
        endParticles.SetActive(true);
        do
        {
            t += Time.deltaTime / 0.5f;
            colorGrading.saturation.value = Mathf.Lerp(startVal, -100f, sReset.SinusoidalLerp(t));
            vignette.intensity.value = Mathf.Lerp(0f, 0.5f, sReset.SinusoidalLerp(t));
            grain.intensity.value = Mathf.Lerp(0f, 0.5f, sReset.SinusoidalLerp(t));
            yield return null;
        } while (t <= 1f);
    }

    IEnumerator BrokenFromLoopPostProcessOff()
    {
        float t = 0f;
        endParticles.SetActive(true);
        ParticleSystem.EmissionModule emission = endParticles.GetComponent<ParticleSystem>().emission;
        emission.rateOverTime = 0f;
        endParticles.AddComponent<sDestroyAfterTime>();
        ChangeMixerSettings(startGroup);
        do
        {
            t += Time.deltaTime / 0.5f;
            colorGrading.saturation.value = Mathf.Lerp(-100f, startVal, sReset.SinusoidalLerp(t));
            vignette.intensity.value = Mathf.Lerp(0.5f, 0f, sReset.SinusoidalLerp(t));
            grain.intensity.value = Mathf.Lerp(0.5f, 0f, sReset.SinusoidalLerp(t));
            yield return null;
        } while (t <= 1f);
    }

    void ResetPostProcessing()
    {
        colorGrading.temperature.value = 0f;
        colorGrading.saturation.value = 36f;
        vignette.intensity.value = 0f;
        grain.intensity.value = 0f;
        chrom.intensity.value = 0f;
        distortion.intensity.value = 0f;
    }

    public void StartNewTrack(AudioSource a, AudioSource b, float overTime)
    {
        if (blending == null)
        {
            blending = StartCoroutine(BlendBetweenTracks(a, b, overTime));
        }
        else
        {
            StopCoroutine(blending);
            blending = null;
        }
    }

    public void ChangeMixerSettings(AudioMixerGroup nGroup)
    {
        secondTrack.outputAudioMixerGroup = nGroup;
    }

    IEnumerator BlendBetweenTracks(AudioSource a, AudioSource b, float overTime)
    {
        float t = 0f;
        b.Play();
        do
        {
            t += Time.deltaTime / overTime;
            a.volume = Mathf.Lerp(1f, 0f, t);
            b.volume = Mathf.Lerp(0f, 1f, t);
            yield return null;
        } while (t <= 1f);
        a.Stop();
        blending = null;
    }

    IEnumerator ChangeSaturationFirst()
    {
        float t = 0f;
        float startVal = colorGrading.temperature.value;
        do
        {
            t += Time.deltaTime / 5;
            colorGrading.temperature.value = Mathf.Lerp(startVal, -10f, sReset.SinusoidalLerp(t));
            yield return null;
        } while (t <= 1f);
    }

}
