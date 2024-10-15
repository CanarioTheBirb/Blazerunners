using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//script for tracking the players current placement **used for local multiplayer**
public class PlayerPlacementTracker : MonoBehaviour
{
    [Header("----- References -----")]
    public NewBaseCar car;
    private GameManager manager;

    [Header("----- Placement Stuff -----")]
    public TextMeshProUGUI placementText;

    [Header("----- Lap Counter Stuff -----")]
    public GameObject lapCounterObject;
    public TextMeshProUGUI lapText;
    private float curLap = 1;

    [Header("----- Win Screen Stuff -----")]
    public GameObject winScreenObject;
    public TextMeshProUGUI finishedTimeText;
    public TextMeshProUGUI finishedPlacementText;
    public float startTime;


    public void Start()
    {
        manager = GameObject.FindAnyObjectByType<GameManager>();
        finishedTimeText.text = "Your finished time: 00:00";
        finishedPlacementText.text = "You finished in 1st";
    }

    // Update is called once per frame
    public void Update()
    {
        if (car == null || manager == null) return;

        PlacementTracker();
        LapPopup();
    }

    private void PlacementTracker()
    {
        if (!car.finishedRace)
            placementText.text = $"{car.curPlacement} place / {manager.curRacerAmount} racers";
        else
            placementText.text = $"finished in {PlacementNum(car.finalPlacement)}";
    }

    private string PlacementNum(int placement)
    {
        string name = "";
        switch (placement)
        {
            case 1:
                name = "1st";
                break;
            case 2:
                name = "2nd";
                break;
            case 3:
                name = "3rd";
                break;
            case 4:
                name = "4th";
                break;
            case 5:
                name = "5th";
                break;
            case 6:
                name = "6th";
                break;
            case 7:
                name = "7th";
                break;
            case 8:
                name = "8th";
                break;
            default: return name;
        }
        return name;
    }

    private void LapPopup()
    {
        if (car.curLap > curLap)
        {
            // Sets the lap counts before display
            if (car.curLap == manager.laps)
            {
                lapText.text = $"Final Lap";
            }
            else
            {
                lapText.text = $"Lap: {car.curLap}/{manager.laps}";
            }

            // The Popup screen shouldn't pop up if you finished the game
            if (car.curLap <= manager.laps)
            {
                //Shows a lap counter 
                lapCounterObject.SetActive(true);
                Invoke("LapPopupOff", 1.5f);
            }
            else
            {
               //Add player's finish screen
               WinScreenPopup();
            }

            //Ensures that this only happens once, like the event that the value changed
            curLap = car.curLap;
        }
    }

    private void LapPopupOff()
    {
        lapCounterObject.SetActive(false);
    }

    private void WinScreenPopup()
    {
        float currentTime = Time.time - startTime;
        string minutes = ((int)currentTime / 60).ToString("00");
        string seconds = (currentTime % 60).ToString("00.00");
        finishedTimeText.text = car.transform.parent.name + "'s finished time: " + minutes + ":" + seconds;

        finishedPlacementText.text = car.transform.parent.name + " finished in " + PlacementNum(car.finalPlacement) + "!";

        winScreenObject.SetActive(true);
    }
}
