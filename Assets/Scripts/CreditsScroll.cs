using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsScroll : MonoBehaviour
{

    public float scrollSpeed = 20f; // Speed of the scrolling
    private RectTransform creditsTransform;
    public float delayBeforeStart = 1.5f;
    private bool scrollingStarted = false;
    private float timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        creditsTransform = GetComponent<RectTransform>();
    }

    void Update()
    {

        if (!scrollingStarted)
        {
            timer += Time.deltaTime;
            if (timer >= delayBeforeStart)
            {
                scrollingStarted = true;
                timer = 0f; 
            }
        }
        else
        {
            creditsTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
        }
    }
}
