using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{

    public GameObject settingsMenuPanel;
    bool settingsMenuActive = false;
    SettingsController settingsControllerInstance;
    // Start is called before the first frame update
    void Start()
    {
        //settingsMenuPanel.SetActive(false);
        settingsMenuActive = false;
    }

    // Update is called once per frame
    void Update()
    {
        //InGameSettingsMenus();
        //InGameSettingsMenus();
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("CourseSelect");
    }

    public void PlayCoopGame()
    {
        SceneManager.LoadScene("Player Color Select");
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void SettingsMenus()
    {
        SceneManager.LoadScene("SettingMenu");
    }

    public void CreditsMenu()
    {
        SceneManager.LoadScene("CreditMenu");
    }

    public void LobbyMenu()
    {
        SceneManager.LoadScene("LobbyMenu");
    }

    public void TutorialButton()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void InGameSettingsMenus()
    {
        settingsMenuActive = true;
      if (settingsMenuActive == true )
      {
            settingsMenuPanel.SetActive(true);
      }
   
            // Activate the settings menu panel
            //settingsMenuPanel.SetActive(true);
            // You may want to pause the game here if necessary
            // Time.timeScale = 0f;
    }

    public void SettingsBackButton()
    {
        settingsMenuActive = false;
        settingsMenuPanel.SetActive(false);
    }
}
