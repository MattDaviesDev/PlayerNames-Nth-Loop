using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sDestroyAfterTime : MonoBehaviour
{

    public float destroyAfterSeconds = 3f;
    float t = 0f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        if (t >= destroyAfterSeconds)
        {
            Destroy(gameObject);
        }
    }
}
