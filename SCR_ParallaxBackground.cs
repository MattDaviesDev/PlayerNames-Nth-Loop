using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_ParallaxBackground : MonoBehaviour
{
    private float startpos;

    private Transform camera;

    public float parallaxEffect;

    // Start is called before the first frame update
    void Start()
    {
        startpos = transform.position.x;
        camera = Camera.main.transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float dist = (camera.position.x * parallaxEffect);

        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

    }
}
