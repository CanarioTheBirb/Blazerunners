using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField]
    private TimelineAsset cutscene;
    [SerializeField]
    private string nextSceneName;

    private string currMapName;

    void Start()
    {
        currMapName = SceneManager.GetActiveScene().name;
        Invoke("LoadSceneAfterCutscene", (float)cutscene.fixedDuration);
    }

    private void LoadSceneAfterCutscene()
    {
        if (nextSceneName == null)
        {
            string nextMapName = currMapName.Substring(0, currMapName.Length - 9);
            SceneManager.LoadScene(nextMapName);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
