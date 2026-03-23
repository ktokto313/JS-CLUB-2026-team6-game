using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement;
using _Game.Scripts.Core; 

public class MainMenu : MonoBehaviour
{

    public Slider musicSlider; 
    public Slider sfxSlider;   
    public AudioSource musicSource; 
    public AudioSource sfxSource;

    void Start()
    {
    
        if (musicSlider != null)
        {
            
            if (musicSource != null) musicSlider.value = musicSource.volume;
            
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            if (sfxSource != null) sfxSlider.value = sfxSource.volume;
            
            sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }
    }

    public void StartGame() {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("testchotien"); 
    }

    public void RestartGame() {
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
       
        if (musicSource != null) 
        {
            musicSource.volume = value;
        }
        
        if (EventManager.current != null)
        {
            EventManager.current.onMusicVolumeSliderUpdate(value);
        }
    }

    public void OnSFXSliderChanged(float value)
    {
        
        if (sfxSource != null) 
        {
            sfxSource.volume = value;
        }

        if (EventManager.current != null)
        {
            EventManager.current.onSFXVolumeSliderUpdate(value);
        }
    }
}