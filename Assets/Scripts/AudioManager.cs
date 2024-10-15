using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance = null;

    [Header("Audio Source")]
    public AudioSource EffectsSource;
    public AudioSource MusicSource;

    [Header("Audio Clips")]
    public AudioClip[] raceMusic;
    public AudioClip ButtonSFX;

    [Header("Item Sounds")]
    public AudioClip itemPickup;
    public AudioClip boost;
    public AudioClip repair;
    public AudioClip rocketShoot;
    public AudioClip rocketBoom;
    public AudioClip rifleShot;
    public AudioClip lmgShot;

    [Header("Volume Values")]
    public float sfxVolume = 1;
    void Start()
    {
        PlayMusic();
    }
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void PlaySFX(AudioSource source, AudioClip sound)
    {
        source.volume = sfxVolume;
        source.clip = sound;
        source.Play();
    }

    public void Play(AudioClip clip)
    {
        EffectsSource.clip = clip;
        EffectsSource.Play();
    }

    public void PlayMusic()
    {
        AudioClip currentMusic = raceMusic[Random.Range(0, raceMusic.Length)];
        MusicSource.clip = currentMusic;
        MusicSource.Play();
        Invoke("PlayMusic", currentMusic.length);
    }

}
