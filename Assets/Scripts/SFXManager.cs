using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    //[SerializeField] public AudioSource sfxobj;                                                                                       
    private AudioSource sfxobj;
    private void Awake()
    {
        if (instance == null) 
        { 
            instance = this;
            //DontDestroyOnLoad(sfxobj);
        }
    }

    public void PlaySFXAudioClip(AudioClip audioClip, Transform transform, float volume)
    {
        //spawn the gameobj
        AudioSource audioSource = Instantiate(sfxobj, transform.position, Quaternion.identity);

        //assign the audioClip
        audioSource.clip = audioClip;

        //assign volume
        audioSource.volume = volume;

        //play the sound
        audioSource.Play();

        //get length of the clip
        float cliplength = audioSource.clip.length;

        //destroy the clip after it is done playing
        Destroy(audioSource, cliplength);
    }
}
