using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class sMainMenu : MonoBehaviour
{
    string playerName = "";
    int numberOfLoops = 0;

    public GameObject firstTimeLoading;
    public CanvasGroup firstTimeParentCanvasGroup;
    public CanvasGroup firstTimeCanvasGroup;
    public GameObject confrimButton;
    public CanvasGroup thankYouCanvas;
    public GameObject mainMenuCanvas;
    public CanvasGroup mainMenuCGroup;
    public TMP_InputField nameInput;
    bool validName = false;
    bool nameEntered = false;

    public GameObject buttons;
    public GameObject stats;
    public GameObject nameChanger;
    public TMP_InputField nameChangeInput;
    public TextMeshProUGUI gameNameText;
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI successfulLoopbreakText;
    public TextMeshProUGUI numberOfResetText;
    public TextMeshProUGUI timeInGameText;
    public TextMeshProUGUI fastestRunText;
    public TextMeshProUGUI crystalsText;

    public Color hintsOnColor;
    public Color hintsOffColor;
    public Toggle hintsToggle;
    public Toggle SFXToggle;
    public Image hintsToggleImage;
    public Image SFXToggleImage;

    public void EndEditNameBox()
    {
        if (nameInput.text == "")
        {
            nameInput.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
        }
        if (nameChangeInput.text == "")
        {
            nameChangeInput.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
        }
    }

    public void StartEditNameBox()
    {
        nameInput.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        nameChangeInput.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
    }


    private void Awake()
    {
        numberOfLoops = PlayerPrefs.GetInt("Loops", 1);
        bool firstTime = PlayerPrefs.GetInt("FirstTime", 0) == 0;
        if (firstTime)
        {
            firstTimeLoading.SetActive(true);
            StartCoroutine(FirstTimeManager());
            mainMenuCanvas.SetActive(false);
        }
        else
        {
            playerName = PlayerPrefs.GetString("PlayerName", "");
            SetGameName();
            firstTimeLoading.SetActive(false);
            mainMenuCanvas.SetActive(true);
        }
        successfulLoopbreakText.text = "Loops broken successfully: " + PlayerPrefs.GetInt("LoopBreaks", 0);
        numberOfResetText.text = "Loop resets: " + PlayerPrefs.GetInt("Resets", 0);
        timeInGameText.text = "Time spent in game: " + SecondsToText(PlayerPrefs.GetFloat("TimeInGame", 0f));
        if (PlayerPrefs.GetFloat("FastestRun", 0f) == 0f)
        {
            fastestRunText.text = "Fastest loop break: No data";
        }
        else
        {
            fastestRunText.text = "Fastest loop break: " + SecondsToText(PlayerPrefs.GetFloat("FastestRun", 0f));
        }
        crystalsText.text = "Crystals collected: " + PlayerPrefs.GetInt("Crystals", 0);
    }

    public void HintsToggleChanged()
    {
        SceneManagement.instance.showHints = hintsToggle.isOn;
        if (hintsToggle.isOn)
        {
            PlayerPrefs.SetInt("Hints", 1);
        }
        else
        {
            PlayerPrefs.SetInt("Hints", 0);
        }
        Color newColor = !hintsToggle.isOn ? hintsOffColor : hintsOnColor;
        hintsToggleImage.color = newColor;
    }

    public void SFXToggleChanged()
    {
        SceneManagement.instance.playSFX = SFXToggle.isOn;
        if (SFXToggle.isOn)
        {
            PlayerPrefs.SetInt("SFX", 1);
        }
        else
        {
            PlayerPrefs.SetInt("SFX", 0);
        }
        Color newColor = !SFXToggle.isOn ? hintsOffColor : hintsOnColor;
        SFXToggleImage.color = newColor;
    }

    IEnumerator FirstTimeManager()
    {
        yield return new WaitForSeconds(1f);
        float t = 0f;
        do
        {
            t += Time.deltaTime / 1f;
            firstTimeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        } while (t <= 1f);
        t = 0f;
        do
        {
            if (nameInput.text == "")
            {
                validName = false;
                confrimButton.SetActive(false);
            }
            else
            {
                validName = true;
                confrimButton.SetActive(true);
            }
            yield return null;
        } while (!nameEntered);
        SetGameName();
        firstTimeCanvasGroup.interactable = false;
        do
        {
            t += Time.deltaTime / 1f;
            firstTimeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        } while (t <= 1f);
        t = 0f;
        do
        {
            t += Time.deltaTime / 1f;
            thankYouCanvas.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        } while (t <= 1f);
        yield return new WaitForSeconds(1f);
        t = 0f;
        do
        {
            t += Time.deltaTime / 1f;
            thankYouCanvas.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        } while (t <= 1f);
        t = 0f;
        mainMenuCanvas.SetActive(true);
        do
        {
            t += Time.deltaTime / 1f;
            firstTimeParentCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        } while (t <= 1f);

    }

    public void SetGameName()
    {
        gameNameText.text = playerName + "'s<br>" + GetNumberAddition(numberOfLoops) + " loop";
    }

    string GetNumberAddition(int loopNumber)
    {
        if (loopNumber == 1)
        {
            return "first";
        }
        if (loopNumber % 10 == 1)
        {
            return loopNumber + "<size=75%>st<size=100%>";
        }
        if (loopNumber % 10 == 2)
        {
            return loopNumber + "<size=75%>nd<size=100%>";
        }
        if (loopNumber % 10 == 3)
        {
            return loopNumber + "<size=75%>rd<size=100%>";
        }
        return loopNumber + "<size=75%>th<size=100%>";
    }

    public void ConfirmName()
    {
        if (validName)
        {
            playerName = nameInput.text;
            statsText.text = playerName + "'s stats";
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.SetInt("FirstTime", 1);
            nameEntered = true;
        }
    }
  
    public void StartNewLoop()
    {
        SceneManagement.instance.SwitchScene(1, mainMenuCGroup);
        SceneManagement.instance.startGameButtonSound.Play();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowStats(bool active)
    {
        stats.SetActive(active);
        buttons.SetActive(!active);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("Hints", 1) == 1)
        {
            hintsToggle.isOn = true;
        }
        else
        {
            hintsToggle.isOn = false;
        }
        if (PlayerPrefs.GetInt("SFX", 1) == 1)
        {
            SFXToggle.isOn = true;
        }
        else
        {
            SFXToggle.isOn = false;
        }
        HintsToggleChanged();
        SFXToggleChanged();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public static string SecondsToText(float timeInSeconds)
    {
        string returnText = "";
        int seconds = 0;
        int minutes = 0;
        int hours = 0;

        minutes = (int)timeInSeconds / 60;
        hours = minutes / 60;
        seconds = (int)timeInSeconds - (minutes * 60);

        if (hours <= 9)
        {
            returnText += "0" + hours.ToString() + ":";
        }
        else
        {
            returnText += hours.ToString() + ":";
        }

        if (minutes <= 9)
        {
            returnText += "0" + minutes.ToString() + ":";
        }
        else
        {
            returnText += minutes.ToString() + ":";
        }

        if (seconds <= 9)
        {
            returnText += "0" + seconds.ToString();
        }
        else
        {
            returnText += seconds.ToString();
        }

        return returnText;
    }

    public void NameChangeRequest()
    {
        nameChanger.SetActive(true);
        buttons.SetActive(false);
    }

    public void NameChangeSubmission()
    {
        if (nameChangeInput.text != "")
        {
            playerName = nameChangeInput.text;
            SetGameName();
            statsText.text = playerName + "'s stats";
            PlayerPrefs.SetString("PlayerName", playerName);
            nameChanger.SetActive(false);
            buttons.SetActive(true);
        }
    }

    public void CancelNameChange()
    {
        nameChangeInput.text = "";
        nameChanger.SetActive(false);
        buttons.SetActive(true);
    }


}
