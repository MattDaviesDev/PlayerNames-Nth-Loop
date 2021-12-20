using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class sButton : MonoBehaviour, IPointerEnterHandler
{
    public AudioSource hover;
    public GameObject clickSound;

    public void OnHover()
    {
        if (SceneManagement.instance.playSFX)
        {
            hover.Play();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHover();
    }

    public void OnClick()
    {
        if (SceneManagement.instance.playSFX)
        {
            Instantiate(clickSound);
        }
    }

    private void Start()
    {
        if (hover == null)
        {
            hover = GetComponent<AudioSource>();
        }
    }
}
