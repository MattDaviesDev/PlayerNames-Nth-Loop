using EZCameraShake;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sCollectilbe : MonoBehaviour
{

    public enum CrystalType
    {
        standardCollectibe, SwordUpgrade, TimeTravelUpgrade, BreakFromLoop, GrappleUpgrade, SlidingUpgrade, EndGame
    }

    public CrystalType typeOfCollectible = CrystalType.standardCollectibe;

    public string upgradeName = "only used for upgrades";
    public string htmlColorString = "";
    public string tutorialMessageString = "";
    public float distanceForInteractHint = 2f;


    public Animator anim;

    public GameObject collectedSound;


    bool collected = false;
    float lerpTime = 0.1f;
    float t = 0f;
    Vector3 scale;

    Vector2 startPos;

    float movementMultiplier = 0.02f;

    // Start is called before the first frame update
    void Start()
    {
        scale = transform.localScale;
        startPos = transform.position;
        InvokeRepeating("SparkleAndShine", 0.1f, 6f);
    }

    void SparkleAndShine()
    {
        anim.SetTrigger("Shine");
    }

    private void Update()
    {
        transform.position = startPos + new Vector2(0.0f, Mathf.Sin(Time.time) * movementMultiplier);

        if (typeOfCollectible != CrystalType.standardCollectibe && !collected)
        {

            float distance = Vector2.Distance(transform.position, GameManager.instance.playerObject.transform.position);
            if (distance < distanceForInteractHint) // check if the player is within X units of the crystal
            {
                GameManager.instance.ShowInteractionHint(upgradeName, htmlColorString); // show the interaction message
                if (Input.GetButton("Collect")) // if they press the collect button 
                {
                    Collected(GameManager.instance.playerObject); // collect the crystal
                    GameManager.instance.HideInteractionHint();
                    if (typeOfCollectible != CrystalType.EndGame)
                    {
                        CameraShaker.Instance.ShakeOnce(1f, 1f, 0.2f, 0.3f);
                    }
                }
            }
            else
            {
                GameManager.instance.HideInteractionHint();
            }
        }


        if (collected)
        {
            t += Time.deltaTime / lerpTime;
            transform.localScale = Vector3.Lerp(scale, Vector3.zero, t);
            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collected && typeOfCollectible == CrystalType.standardCollectibe)
        {
            if (collision.CompareTag("Player"))
            {
                GameManager.instance.IncreaseCrystalCounter();
                if (SceneManagement.instance.playSFX)
                {
                    GameObject temp = Instantiate(collectedSound);
                    temp.GetComponent<AudioSource>().pitch += (GameManager.instance.recentCollections - 1) * GameManager.instance.pitchIncrease;
                }
                collected = true;
            }
        }
    }

    private void Collected(GameObject player)
    {
        if (SceneManagement.instance != null)
        {
            if (SceneManagement.instance.showHints)
            {
                GameManager.instance.NewTutorialMessage(tutorialMessageString);
            }
        }
        switch (typeOfCollectible)
        {
            case CrystalType.SwordUpgrade:
                player.GetComponent<sPlayerMove>().attackUnlocked = true;
                break;
            case CrystalType.TimeTravelUpgrade:
                GameManager.instance.UnlockTimeControl();
                break;
            case CrystalType.BreakFromLoop:
                player.GetComponentInChildren<sReset>().brokenFromLoop = true;
                GameManager.instance.BreakFromLoop(false);
                GameManager.instance.endCrystal.tutorialMessageString = "Thank you for playing.";
                GameManager.instance.ChangeMixerSettings(GameManager.instance.newGroup);
                break;
            case CrystalType.SlidingUpgrade:
                player.GetComponent<sPlayerMove>().slidingUnlocked = true;
                break;
            case CrystalType.EndGame:
                if (sReset.instance.brokenFromLoop)
                {
                    player.GetComponent<sPlayerMove>().enabled = false;
                    GameManager.instance.BreakFromLoop(true);
                    GameManager.instance.EndGame(true);
                    CameraShaker.Instance.ShakeOnce(1f, 1f, 1f, 3f);
                }
                break;
            default:
                break;
        }
        if (SceneManagement.instance.playSFX)
        {
            if (!sReset.instance.brokenFromLoop && typeOfCollectible == CrystalType.EndGame)
            {
                return;
            }
            Instantiate(collectedSound);
        }
        collected = true;
    }


}
