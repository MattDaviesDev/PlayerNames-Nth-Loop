using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sReset : MonoBehaviour
{
    public static sReset instance;

    [SerializeField] private Transform curCheckpoint;
    private Transform player;
    [SerializeField] private LayerMask checkpointMask;
    private float timer = 0.0f;

    Coroutine resetting = null;
    public Transform resetMask;
    Vector3 resetScale;

    public bool isResetting = false;
    public bool brokenFromLoop = false;

    //public TMPro.TextMeshPro resetCounterText;

    void Start()
    {
        instance = this;
        player = GameManager.instance.playerObject.transform;
        resetScale = resetMask.localScale;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //timer -= Time.deltaTime;
        //if (timer <= 0)
        //{
        //    timer = 5.0f;
        //    Collider2D col = Physics2D.OverlapCircle(transform.position, 10.0f, checkpointMask);
        //    if (col is object)
        //    {
        //        curCheckpoint = col.transform;
        //    }
        //}
    }

    public void ResetThePlayer()
    {
        Debug.Log("Player reset to checkpoint");
        player.transform.position = curCheckpoint.position;
        Camera.main.transform.position = new Vector3(curCheckpoint.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
        player.GetComponent<sTimeControl>().ResetTimeTravelData();
    }

    public void StartPlayerReset()
    {
        if (!brokenFromLoop)
        {
            if (resetting == null)
            {
                resetting = StartCoroutine(ResetAnim());
            }
        }
        else
        {
            GameManager.instance.EndGame(false);
        }
    }

    public void Respawned()
    {
        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Checkpoint")
        {
            curCheckpoint = collision.transform;
        }
    }

    IEnumerator ResetAnim()
    {
        player.GetComponent<sPlayerMove>().enabled = false; // stop the player's control
        float t = 0f;
        do
        {
            t += Time.deltaTime / 0.5f;
            resetMask.localScale = Vector3.Lerp(resetScale, Vector3.zero, SinusoidalLerp(t));
            yield return null;
        } while (t <= 1f);
        t = 0f;
        ResetThePlayer();
        yield return new WaitForSeconds(0.5f); // wait half a second for the game to catch up with
        do                                     // the player's new pos
        {
            t += Time.deltaTime / 0.5f;
            resetMask.localScale = Vector3.Lerp(Vector3.zero, resetScale, SinusoidalLerp(t));
            if (t >= 0.5f)
            {
                player.GetComponent<sPlayerMove>().enabled = true; // re-enable control half way
            }                                                      // through the mask anim
            yield return null;
        } while (t <= 1f);
        isResetting = false;
        resetting = null;
    }

    public static float SinusoidalLerp(float t)
    {
        return 0.5f * (float)System.Math.Sin(((2 * Mathf.PI * t) - Mathf.PI) / 2) + 0.5f;
    }

}
