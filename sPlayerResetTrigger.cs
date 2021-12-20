using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sPlayerResetTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.transform.tag);
        if (collision.transform.tag == "Player")
        {
            GameManager.instance.playerObject.GetComponent<sPlayerMove>().StartReset();
            Debug.Log("Reset");
        }
    }
}
