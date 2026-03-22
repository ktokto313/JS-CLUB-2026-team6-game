using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using _Game.Scripts.Core;

public class MainMenu : MonoBehaviour
{
    public GameObject musicSlider;
    public GameObject sfxSlider;

    public void StartGame() {
        SceneManager.LoadScene("testchotien"); 
    }
    
    public void ToggleMusic() {
        if(musicSlider != null) musicSlider.SetActive(!musicSlider.activeSelf);
    }
    

    public void ToggleSFX() {
        if(sfxSlider != null) sfxSlider.SetActive(!sfxSlider.activeSelf);
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
    
}