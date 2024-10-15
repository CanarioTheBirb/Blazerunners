using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChoiceSelect : MonoBehaviour
{
    public Button startButton;
    public GameObject player1;
    private void Start()
    {
        PlayerCarrier.instance.playerCars[0].uiControls.curButton = startButton;
        PlayerCarrier.instance.playerCars[0].uiControls.playerCursor = player1;
    }
    public void Load1stLevel()
    {
        SceneManager.LoadScene("1st_Race_Track_Cutscene");
    }

    public void Load2ndLevel()
    {
        SceneManager.LoadScene("5th_Race_Track_Cutscene");
    }

    public void Load3rdLevel()
    {
        SceneManager.LoadScene("4th_Race_Track_Cutscene");
    }
    public void LoadMultiplayer1stLevel()
    {
        SceneManager.LoadScene("Multiplayer1st_Race_Track_Cutscene");
    }

    public void LoadMultiplayer2ndLevel()
    {
        SceneManager.LoadScene("Multiplayer5th_Race_Track_Cutscene");
    }

    public void LoadMultiplayer3rdLevel()
    {
        SceneManager.LoadScene("Multiplayer4th_Race_Track_Cutscene");
    }

    public void Return()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ReturnColor()
    {
        Destroy(PlayerCarrier.instance.gameObject);
        SceneManager.LoadScene("Player Color Select");
    }
}
