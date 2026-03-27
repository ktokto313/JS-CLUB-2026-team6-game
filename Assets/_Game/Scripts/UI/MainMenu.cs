using System;
using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;
using _Game.Scripts.Core; 

public class MainMenu : MonoBehaviour
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string EndlessUnlockedKey = "EndlessUnlocked"; 
    private const float DefaultVolume = 1.0f;

    [Header("Endless Mode Visuals")]
    public Button endlessButton;    
    public Sprite lockedSprite;     
    public Sprite unlockedSprite;   
    public GameObject endlessText;  

    [Header("Volume Settings")]
    public Slider musicSlider; 
    public Slider sfxSlider;   

    void Start()
    {
        
        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, DefaultVolume);
        float sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey, DefaultVolume);
    
        if (musicSlider != null)
        {
            musicSlider.value = musicVolume;
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
            sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }

        if (EventManager.current != null)
        {
            EventManager.current.onMusicVolumeSliderUpdate(musicVolume);
            EventManager.current.onSFXVolumeSliderUpdate(sfxVolume);
        }

        
        UpdateEndlessUI();
    }

    private void UpdateEndlessUI()
    {
        
        bool isUnlocked = PlayerPrefs.GetInt(EndlessUnlockedKey, 0) == 1;

        if (endlessButton != null)
        {
            endlessButton.interactable = isUnlocked;

            
            Image buttonImage = endlessButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = isUnlocked ? unlockedSprite : lockedSprite;
            }

            
            if (endlessText != null)
            {
                endlessText.SetActive(isUnlocked);
            }
        }
    }

    private void OnDestroy()
    {
        SaveVolumePreferences();
    }

    public void StartGame() {
        SaveVolumePreferences();
        SpawnManager.IsEndlessMode = false; 
        Time.timeScale = 1f; 
        SceneManager.LoadScene("testchotien"); 
    }

    public void StartEndlessMode() {
        SaveVolumePreferences();
        SpawnManager.IsEndlessMode = true; 
        Time.timeScale = 1f; 
        SceneManager.LoadScene("testchotien"); 
    }

    
    public void RestartGame() {
        SaveVolumePreferences();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void ToggleMusic() {
        if(musicSlider != null) musicSlider.gameObject.SetActive(!musicSlider.gameObject.activeSelf);
    }

    public void ToggleSFX() {
        if(sfxSlider != null) sfxSlider.gameObject.SetActive(!sfxSlider.gameObject.activeSelf);
    }

    public void OnMusicSliderChanged(float value)
    {
        if (EventManager.current != null) EventManager.current.onMusicVolumeSliderUpdate(value);
    }

    public void OnSFXSliderChanged(float value)
    {
        if (EventManager.current != null) EventManager.current.onSFXVolumeSliderUpdate(value);
    }
    
    public void QuitGame()
    {
        SaveVolumePreferences();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void SaveVolumePreferences()
    {
        if (musicSlider != null) PlayerPrefs.SetFloat(MusicVolumeKey, musicSlider.value);
        if (sfxSlider != null) PlayerPrefs.SetFloat(SFXVolumeKey, sfxSlider.value);
        PlayerPrefs.Save();
    }
}