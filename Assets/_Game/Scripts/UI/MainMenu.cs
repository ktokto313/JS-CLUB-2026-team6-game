using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;
using _Game.Scripts.Core; 

public class MainMenu : MonoBehaviour
{
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const float DefaultVolume = 1.0f;

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
    }

    public void StartGame() {
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
        if (EventManager.current != null)
        {
            EventManager.current.onMusicVolumeSliderUpdate(value);
        }
    }

    public void OnSFXSliderChanged(float value)
    {
        if (EventManager.current != null)
        {
            EventManager.current.onSFXVolumeSliderUpdate(value);
        }
    }
    
    public void QuitGame()
    {
        SaveVolumePreferences();
        Debug.Log("Game đang thoát...");

        #if UNITY_EDITOR
            
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void SaveVolumePreferences()
    {
        if (musicSlider != null)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, musicSlider.value);
        }

        if (sfxSlider != null)
        {
            PlayerPrefs.SetFloat(SFXVolumeKey, sfxSlider.value);
        }

        PlayerPrefs.Save();
    }
}