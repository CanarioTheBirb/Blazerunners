using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    public float savedMasterVolume;
    public float savedMusicVolume;
    public float savedSFXVolume;

    [Header("Volume Settings")]
    [SerializeField] private TMP_Text masterVolumePrecentValue;
    [SerializeField] private Slider masterVolumeSlider;

    [SerializeField] private TMP_Text musicVolumePercentValue;
    [SerializeField] private Slider musicVolumeSlider;

    [SerializeField] private TMP_Text sfxVolumePercentValue;
    [SerializeField] private Slider sfxVolumeSlider;

    void Start()
    {
        //I know its like the same but what this means is that if there isnt a key value already then it's going to default to the value set in inspector
        savedMasterVolume = PlayerPrefs.GetFloat("MasterVolume", masterVolumeSlider.normalizedValue);
        masterVolumeSlider.normalizedValue = savedMasterVolume;
        LoadMasterSliderPosition();
        ApplyMasterVolume();

        savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", musicVolumeSlider.normalizedValue);
        musicVolumeSlider.normalizedValue = savedMusicVolume;
        LoadMusicSliderPosition();
        ApplyMusicVolume();
        
        savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", sfxVolumeSlider.normalizedValue);
        sfxVolumeSlider.normalizedValue = savedSFXVolume;
        LoadSFXSliderPosition();
        ApplySFXVolume();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetMasterVolume()
    {
        //sets Master volume
        float volume = masterVolumeSlider.normalizedValue;
        AudioListener.volume = volume;
        masterVolumePrecentValue.text = volume.ToString("0.0");
    }

    public void SetMusicVolume()
    {
        float volume = musicVolumeSlider.normalizedValue;
        AudioManager.instance.MusicSource.volume = volume;
        musicVolumePercentValue.text = volume.ToString("0.0");
    }

    public void SetSFXVolume()
    {
        float volume = sfxVolumeSlider.normalizedValue;
        sfxVolumePercentValue.text = volume.ToString("0.0");
    }

    public void ApplyMasterVolume()
    {
        SetMasterVolume();
        SaveMasterSliderPosition();
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.normalizedValue);
        PlayerPrefs.Save();
    }

    public void ApplyMusicVolume()
    {
        SetMusicVolume();
        SaveMusicSliderPosition();
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.normalizedValue);
        PlayerPrefs.Save();
    }

    public void ApplySFXVolume()
    {

        SetSFXVolume();
        SaveSFXSliderPosition();
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.normalizedValue);
        PlayerPrefs.Save();
        AudioManager.instance.sfxVolume = PlayerPrefs.GetFloat("SFXVolume", sfxVolumeSlider.normalizedValue);
    }
    public void SaveMasterSliderPosition()
    {
        PlayerPrefs.SetFloat("MasterSliderPosition", masterVolumeSlider.normalizedValue);
        PlayerPrefs.Save();
        savedMasterVolume = PlayerPrefs.GetFloat("MasterSliderPosition", masterVolumeSlider.normalizedValue);
    }

    public void SaveMusicSliderPosition()
    {
        PlayerPrefs.SetFloat("MusicSliderPosition", musicVolumeSlider.normalizedValue);
        PlayerPrefs.Save();
        savedMusicVolume = PlayerPrefs.GetFloat("MusicSliderPosition", musicVolumeSlider.normalizedValue);
    }

    public void SaveSFXSliderPosition()
    {
        PlayerPrefs.SetFloat("SFXSliderPosition", sfxVolumeSlider.normalizedValue);
        PlayerPrefs.Save();
        savedSFXVolume = PlayerPrefs.GetFloat("SFXSliderPosition", sfxVolumeSlider.normalizedValue);
    }

    public void LoadMasterSliderPosition()
    {
        float savedSliderPosition = PlayerPrefs.GetFloat("MasterSliderPosition", 1f);
        masterVolumeSlider.normalizedValue = savedSliderPosition;
    }

    public void LoadMusicSliderPosition()
    {
        float savedMusicSliderPosition = PlayerPrefs.GetFloat("MusicSliderPosition", 1f);
        musicVolumeSlider.normalizedValue = savedMusicSliderPosition;
    }

    public void LoadSFXSliderPosition()
    {
        float savedSFXSliderPosition = PlayerPrefs.GetFloat("SFXSliderPosition", 1f);
        sfxVolumeSlider.normalizedValue = savedSFXSliderPosition;
    }

    public void ResetDefaultSettings()
    {
        float defaultVolume = 1.0f;

        masterVolumeSlider.normalizedValue = defaultVolume;
        SetMasterVolume();
        musicVolumeSlider.normalizedValue = defaultVolume;
        SetMusicVolume();
        sfxVolumeSlider.normalizedValue = defaultVolume;
        SetSFXVolume();

        PlayerPrefs.SetFloat("MasterVolume", defaultVolume);
        PlayerPrefs.SetFloat("MasterSliderPosition", defaultVolume);

        PlayerPrefs.SetFloat("MusicVolume", defaultVolume);
        PlayerPrefs.SetFloat("MusicSliderPosition", defaultVolume);

        PlayerPrefs.SetFloat("SFXVolume", defaultVolume);
        PlayerPrefs.SetFloat("SFXSliderPosition", defaultVolume);

        PlayerPrefs.Save();

        AudioManager.instance.sfxVolume = defaultVolume;
    }

}
