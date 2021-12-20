using EZCameraShake;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class sTimeControl : MonoBehaviour
{

    private bool isRewinding = false;

    float lengthOfRewind = 2f;

    public Transform objectToRewind;
    public bool recordPositions;
    public bool recordRotations;
    public Animator spriteAnimator;
    public string idleAnimParamName = "Idle";
    public SpriteRenderer spriteRend;
    public bool recordSprite;
    public Rigidbody2D rb;
    public bool recordRigidBodyVelocity;

    int numberOfListIndexes;
    List<Vector3> positions;
    List<Quaternion> rotations;
    List<Sprite> sprites;
    List<Vector2> rbVels;

    public KeyCode rewindKey = KeyCode.Z;

    public PostProcessProfile postProcessVolume;
    public AudioSource rewindSFX;
    public GameObject particles;
    Vignette vignette;
    ColorGrading colorGrading;
    ChromaticAberration chrom;
    LensDistortion lens;


    public Transform playerGhost;
    public SpriteRenderer playerGhostImage;

    // Start is called before the first frame update
    void Start()
    {
        if (playerGhost != null)
        {
            playerGhost.parent = null;
        }
        numberOfListIndexes = (int)(lengthOfRewind / Time.fixedDeltaTime);
        if (postProcessVolume != null)
        {
            AssignPostProcessingThings();
        }
        if (recordPositions)
            positions = new List<Vector3>();
        if (recordRotations)
            rotations = new List<Quaternion>();
        if (recordSprite)
            sprites = new List<Sprite>();
        if (recordRigidBodyVelocity)
            rbVels = new List<Vector2>();
    }

    private void FixedUpdate()
    {
        if (GameManager.instance.timeTravelUnlocked)
        {
            if (GameManager.instance.timeControlOnCooldown)
            {
                playerGhost.gameObject.SetActive(true);
            }
            else
            {
                playerGhost.gameObject.SetActive(false);
            }
            if (playerGhost != null)
            {
                if (positions.Count > 0)
                {
                    playerGhost.position = positions[positions.Count - 1];
                    playerGhost.rotation = rotations[rotations.Count - 1];
                    playerGhostImage.sprite = sprites[sprites.Count - 1];
                }
                else
                {
                }
            }

            if (isRewinding)
                Rewind();
            else
                Record();
        }
        else
        {
            playerGhost.gameObject.SetActive(false);

        }
    }

    void Record()
    {
        if (recordPositions && objectToRewind != null)
        {
            positions.Insert(0, objectToRewind.position);
            if (positions.Count > numberOfListIndexes)
            {
                positions.RemoveAt(positions.Count - 1);
            }
        }
        if (recordRotations && objectToRewind != null)
        {
            rotations.Insert(0, objectToRewind.rotation);
            if (rotations.Count > numberOfListIndexes)
            {
                rotations.RemoveAt(rotations.Count - 1);
            }
        }
        if (recordSprite && spriteRend != null)
        {
            sprites.Insert(0, spriteRend.sprite);
            if (sprites.Count > numberOfListIndexes)
            {
                sprites.RemoveAt(sprites.Count - 1);
            }
        }
        if (recordRigidBodyVelocity && rb != null)
        {
            rbVels.Insert(0, rb.velocity);
            if (rbVels.Count > numberOfListIndexes)
            {
                rbVels.RemoveAt(rbVels.Count - 1);
            }
        }
    }

    void Rewind()
    {
        if (postProcessVolume != null)
        {
            Time.timeScale = 2f;
        }

        if (recordPositions && objectToRewind != null )
        {
            if (positions.Count > 0)
            {
                objectToRewind.position = positions[0];
                positions.RemoveAt(0);
            }
            else
            {
                StopRewind();
            }
        }
        if (recordRotations && objectToRewind != null && rotations.Count > 0)
        {
            if (rotations.Count > 0)
            {
                objectToRewind.rotation = rotations[0];
                rotations.RemoveAt(0);
            }
            else
            {
                StopRewind();
            }
        }
        if (recordSprite && spriteRend != null && sprites.Count > 0)
        {
            if (sprites.Count > 0)
            {
                spriteRend.sprite = sprites[0];
                sprites.RemoveAt(0);
            }
            else
            {
                StopRewind();
            }
        }
        if (recordRigidBodyVelocity && rb != null && rbVels.Count > 0)
        {
            if (rbVels.Count > 0)
            {
                rb.velocity = rbVels[0];
                rbVels.RemoveAt(0);
            }
            else
            {
                StopRewind();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.timeTravelUnlocked)
        {
            if (GameManager.instance.timeControlReady)
            {
                if (Input.GetButton("Rewind") && !isRewinding && !sReset.instance.isResetting)
                {
                    StartRewind();
                    GameManager.instance.OverrideClockHandMovement(true);
                }
            }
            //if (Input.GetKeyUp(rewindKey))
            //{
            //    StopRewind();
            //}
        }
    }

    public void StartRewind()
    {
        isRewinding = true;
        print(positions.Count);
        CameraShaker.Instance.ShakeOnce(1f, 1f, 0f, 1f);
        if (postProcessVolume != null)
        {
            if (SceneManagement.instance.playSFX)
            {
                rewindSFX.Play();
            }
            GetComponent<BoxCollider2D>().enabled = false;
            StartCoroutine(StartRewindPostProcessChange());
            particles.SetActive(true);
        }
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        if (spriteAnimator != null)
        {
            spriteAnimator.StopPlayback();
        }
    }

    public void StopRewind()
    {
        if (postProcessVolume != null)
        {
            Time.timeScale = 1f;
            GetComponent<BoxCollider2D>().enabled = true;
            particles.SetActive(false);
        }
        isRewinding = false;
        GameManager.instance.timeControlReady = false;
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        if (spriteAnimator != null)
        {
            spriteAnimator.SetTrigger(idleAnimParamName);
        }
    }


    public void ResetTimeTravelData()
    {
        positions.Clear();
        rotations.Clear();
        sprites.Clear();
        rbVels.Clear();
    }

    void AssignPostProcessingThings()
    {
        postProcessVolume.TryGetSettings(out vignette);
        postProcessVolume.TryGetSettings(out colorGrading);
        postProcessVolume.TryGetSettings(out chrom);
        postProcessVolume.TryGetSettings(out lens);
    }


    IEnumerator StartRewindPostProcessChange()
    {
        float t = 0f;
        float startVinVal = vignette.intensity.value;
        float startColTempVal = colorGrading.temperature.value;
        do
        {
            t += Time.deltaTime / 0.2f;
            chrom.intensity.value = Mathf.Lerp(0f, 1f, sReset.SinusoidalLerp(t));
            lens.intensity.value = Mathf.Lerp(0f, -50f, sReset.SinusoidalLerp(t));
            vignette.intensity.value = Mathf.Lerp(startVinVal, 0.6f, sReset.SinusoidalLerp(t));
            colorGrading.temperature.value = Mathf.Lerp(startColTempVal, -60f, sReset.SinusoidalLerp(t));
            yield return null;
        } while (t <= 1f);
        do
        {
            yield return null;
        } while (isRewinding);
        t = 0f;
        do
        {
            t += Time.deltaTime / 0.2f;
            chrom.intensity.value = Mathf.Lerp(1f, 0f, sReset.SinusoidalLerp(t));
            lens.intensity.value = Mathf.Lerp(-50f, 0f, sReset.SinusoidalLerp(t));
            vignette.intensity.value = Mathf.Lerp(0.6f, startVinVal, sReset.SinusoidalLerp(t));
            colorGrading.temperature.value = Mathf.Lerp(-60f, startColTempVal, sReset.SinusoidalLerp(t));
            yield return null;
        } while (t <= 1f);
    }


}
