using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RaceCountDown : MonoBehaviour
{

    public RawImage RaceStartIndicator;
    public Color[] countdownColors; // Array of colors for each stage of the countdown
    public float countdownDuration = 3f;
    private float timeLeft;
    private bool raceStarted = false;

    [SerializeField] private AudioClip countdownClip;

    // Start is called before the first frame update
    void Start()
    {
        timeLeft = countdownDuration;
        RaceStartIndicator.color = countdownColors[0];
        //AudioManager.instance.Play(countdownClip); 
    }

    // Update is called once per frame
    void Update()
    {
        if (!raceStarted)
        {
            PlayerMovement[] playerCars = GameObject.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
            AIMovement[] aiCars = GameObject.FindObjectsByType<AIMovement>(FindObjectsSortMode.None);

            foreach(PlayerMovement car in playerCars)
            {
                car.GetComponent<PlayerMovement>().enabled = false;
            }
            foreach(AIMovement car in aiCars)
            {
                car.GetComponent<AIMovement>().isDriving = false;
            }

            timeLeft -= Time.deltaTime;
            // Calculate the index of the current countdown stage
            int countdownIndex = Mathf.Clamp(Mathf.CeilToInt(timeLeft), 0, countdownColors.Length - 1);

            // Change the color of the RawImage based on the countdown stage
            RaceStartIndicator.color = countdownColors[countdownIndex];

            if (timeLeft <= 0)
            {
                StartRace();

                foreach (PlayerMovement car in playerCars)
                {
                    car.GetComponent<PlayerMovement>().enabled = true;
                }

                foreach(AIMovement car in aiCars)
                {
                    car.GetComponent<AIMovement>().isDriving = true;
                }
            }
        }
    }
    void StartRace()
    {
        raceStarted = true;
        RaceStartIndicator.enabled = false;
    }
}
