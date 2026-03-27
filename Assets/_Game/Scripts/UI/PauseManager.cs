using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject winPanel;
    public GameObject losePanel;
    [SerializeField] private GameObject statCard;
    [SerializeField] private GameObject[] extraUIs; 
    private const string EndlessUnlockedKey = "EndlessUnlocked";

    void Start()
    {
        if (_Game.Scripts.Core.EventManager.current != null)
        {
            _Game.Scripts.Core.EventManager.current.onPlayerDeadAction += GameOver;
            _Game.Scripts.Core.EventManager.current.onWinAction += Win;
        }
    }

    private void SetPauseElements(bool isActive)
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(isActive);
        
        if (extraUIs != null)
        {
            foreach (var ui in extraUIs)
            {
                if (ui != null) ui.SetActive(isActive);
            }
        }
    }

    public void GameOver()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
            if (statCard != null) statCard.SetActive(true);
            Time.timeScale = 0f;    
        }
    }
    
    public void Win() 
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            if (statCard != null) statCard.SetActive(true);
            
            PlayerPrefs.SetInt(EndlessUnlockedKey, 1);
            PlayerPrefs.Save(); 
            Time.timeScale = 0f;
        }
    }

    public void PauseGame()
    {
        SetPauseElements(true);
        Time.timeScale = 0f; 
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (extraUIs != null)
        {
            foreach (var ui in extraUIs)
            {
                if (ui != null) ui.SetActive(false); 
            }
        }
        if (statCard != null) statCard.SetActive(false);
        Time.timeScale = 1f; 
    }
    
    public void TogglePause()
    {
        if (pauseMenuPanel.activeSelf) ResumeGame();
        else PauseGame();
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f; 

        
        if (SpawnManager.IsEndlessMode && losePanel.activeSelf)
        {
            SpawnManager.IsEndlessMode = false;
            Debug.Log("Thua ở Endless -> Tự động chuyển về Campaign");
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        SpawnManager.IsEndlessMode = false; 
        SceneManager.LoadScene("Menu"); 
    }

    public void PlayEndlessMode()
    {
        Time.timeScale = 1f; 
        SpawnManager.IsEndlessMode = true; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    void OnDestroy()
    {
        if (_Game.Scripts.Core.EventManager.current != null)
        {
            _Game.Scripts.Core.EventManager.current.onPlayerDeadAction -= GameOver;
            _Game.Scripts.Core.EventManager.current.onWinAction -= Win; 
        }
    }
}