using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUIManager : MonoBehaviour
{
    //Timer TimerInstance;
    //[System.Serializable]
    //public class TutorialPrompt
    //{
    //    public string promptText;
    //    public float displayDuration;
    //    public float pauseDuration; 
    //}

    //public List<TutorialPrompt> prompts; 
    //public TextMeshProUGUI promptText;
    //public Image backgroundImage; 

    //private int currentPromptIndex = 0;
    //private float promptTimer;
    //public bool isDisplayingPrompt = false;

    //void Start()
    //{
    //    DisplayNextPrompt();
    //    TimerInstance = GameObject.FindAnyObjectByType<Timer>();
    //}

    //void Update()
    //{
    //    if (isDisplayingPrompt)
    //    {
    //        promptTimer += Time.deltaTime;
    //        if (promptTimer >= prompts[currentPromptIndex].displayDuration)
    //        {              
    //            HidePrompt();
    //            currentPromptIndex++;
    //            if (currentPromptIndex < prompts.Count)
    //            {
    //                StartCoroutine(StartPauseTimer(prompts[currentPromptIndex].pauseDuration));
    //            }
    //        }
    //    }

    //}

    //public void DisplayNextPrompt()
    //{
    //    // Set the prompt text from the current prompt in the list
    //    promptText.text = prompts[currentPromptIndex].promptText;

    //    // Show the prompt text
    //    promptText.gameObject.SetActive(true);

    //    // Show the background image if it exists
    //    if (backgroundImage != null)
    //    {
    //        backgroundImage.gameObject.SetActive(true);
    //    }

    //    // Reset the prompt timer
    //    promptTimer = 0f;

    //    // Set flag to indicate that a prompt is being displayed
    //    isDisplayingPrompt = true;
    //}

    //void HidePrompt()
    //{
    //    promptText.gameObject.SetActive(false);

    //    backgroundImage.gameObject.SetActive(false);
    //    isDisplayingPrompt = false;
    //}

    //IEnumerator StartPauseTimer(float duration)
    //{
    //    yield return new WaitForSeconds(duration);
    //    DisplayNextPrompt();
    //}
    [System.Serializable]
    public class TutorialPrompt
    {
        public string promptText;
        public float displayDuration;
        public float pauseDuration;
    }

    public List<TutorialPrompt> prompts;
    public TextMeshProUGUI promptText;
    public Image backgroundImage;

    private int currentPromptIndex = 0;
    public bool isDisplayingPrompt = false;
    Timer timerInstance;

    void Start()
    {
        DisplayNextPrompt();
        //timerInstance = GameObject.FindAnyObjectByType<Timer>();
    }

    void Update()
    {
        if (isDisplayingPrompt)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift)) // Check for spacebar input
            {
                HidePrompt();
                currentPromptIndex++;
                if (currentPromptIndex < prompts.Count)
                {
                    StartCoroutine(StartDisplayPrompt());
                }
                else
                {
                    isDisplayingPrompt = false; // No more prompts to display

                }
            }
        }
    }

    IEnumerator StartDisplayPrompt()
    {
        yield return new WaitForSeconds(prompts[currentPromptIndex].pauseDuration);
        DisplayNextPrompt();
    }

    void DisplayNextPrompt()
    {
        if (currentPromptIndex < prompts.Count)
        {
            // Set the prompt text from the current prompt in the list
            promptText.text = prompts[currentPromptIndex].promptText;

            // Show the prompt text
            promptText.gameObject.SetActive(true);

            // Show the background image if it exists
            if (backgroundImage != null)
            {
                backgroundImage.gameObject.SetActive(true);
            }

            // Set flag to indicate that a prompt is being displayed
            isDisplayingPrompt = true;
        }
    }

    void HidePrompt()
    {
        promptText.gameObject.SetActive(false);
        if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(false);
        }
    }

}
