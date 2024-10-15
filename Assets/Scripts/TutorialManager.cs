using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    private void Awake()
    {

        int firstTime = PlayerPrefs.GetInt("FirstTime", 1);
        Debug.Log("FirstTime value: " + firstTime);
        if (PlayerPrefs.GetInt("FirstTime", 1) == 1)
        {
            PlayerPrefs.SetInt("FirstTime", 0); 
            SceneManager.LoadScene("Tutorial"); 
        }
        //else
        //{
        //    // Not the first time playing, load main game scene
        //    SceneManager.LoadScene("MainMenu"); // Load your main game scene
        //}
    }

    private void OnDestroy()
    {
        
    }
}
