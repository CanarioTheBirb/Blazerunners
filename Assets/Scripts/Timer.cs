using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public bool raceStarted = false;
    public float waitDuration = 3f;
    private float startTime;
    private float endTime;
    public bool raceFinished = false;
    public bool isPaused = false;
    public GameObject pauseMenuPanel;
    public KeyCode pauseKey = KeyCode.P;
    [SerializeField] public TextMeshProUGUI ammoText;
    [SerializeField] public GameObject ammoHolder;
    private NewBaseCar player;
    GameManager gameManager;
    TutorialUIManager tutorialUiManagerInstance;

    // Start is called before the first frame update
    //void Start()
    public void Initialize()
    {
        Invoke("StartRace", waitDuration);
        gameManager = GameObject.FindAnyObjectByType<GameManager>();
        //StartRace();
        //pauseMenuPanel.SetActive(false);
        //isPaused = false;
        player = GameObject.FindAnyObjectByType<PlayerMovement>().GetComponentInChildren<NewBaseCar>();
        tutorialUiManagerInstance = GameObject.FindAnyObjectByType<TutorialUIManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(pauseKey) && !gameManager.raceOver && tutorialUiManagerInstance.isDisplayingPrompt == false)
        {
            //TogglePause();
            PauseGame();
        }
        else if (raceStarted && !raceFinished && !isPaused)
        {
            UpdateTimer();
        }

        //UpdateTimer();
        AmmoUpdate();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("StartLine"))
        {
            StartRace();
        }

        if (other.CompareTag("FinishLine") && raceStarted && !raceFinished)
        {
            FinishRace();
        }
    }

    public void StartRace()
    {
        raceStarted = true;
        startTime = Time.time;
        //timerText.text = "Race Started!";
    }

    void UpdateTimer()
    {
        float currentTime = Time.time - startTime;
        string minutes = ((int)currentTime / 60).ToString("00");
        string seconds = (currentTime % 60).ToString("00.00");
        timerText.text = minutes + ":" + seconds;
    }

    void FinishRace()
    {
        raceFinished = true;

        endTime = Time.time;

        float raceTime = endTime - startTime;

        string minutes = ((int)raceTime / 60).ToString("00");
        string seconds = (raceTime % 60).ToString("00.00");
        timerText.text = "Time: " + minutes + ":" + seconds;
    }

    public void PauseGame()
    { 
         Time.timeScale = 0f;
         pauseMenuPanel.SetActive(true);
         Cursor.lockState = CursorLockMode.None;
         Cursor.visible = true;
         //isPaused = true;
        
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void AmmoUpdate()
    {
        if (player.item != null)
        {
            ammoHolder.SetActive(true);
            ammoText.text = player.item.GetComponent<ItemManager>().Uses.ToString();
        }
        else
        {
            ammoHolder.SetActive(false);
        }
    }
}

