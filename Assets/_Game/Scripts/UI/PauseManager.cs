using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public GameObject winPanel;
    public GameObject losePanel;
    [SerializeField] private GameObject statCard;
    
    void Start()
    {
        
        if (_Game.Scripts.Core.EventManager.current != null)
        {
            _Game.Scripts.Core.EventManager.current.onPlayerDeadAction += GameOver;
        }
    }
    
    public void GameOver()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
            statCard.SetActive(true);
            Time.timeScale = 0f;    
        }
    }

    public void PauseGame()
         {
             pauseMenuPanel.SetActive(true);
             statCard.SetActive(true);
             Time.timeScale = 0f; 
         }
     
         public void ResumeGame()
         {
     
             pauseMenuPanel.SetActive(false);
             statCard.SetActive(false);
             Time.timeScale = 1f; 
         }
         
         public void TogglePause()
         {

             if (pauseMenuPanel.activeSelf)
             {
                 ResumeGame();
             }
             else 
             {
                 PauseGame();
             }
         }
         
         public void RestartGame()
         {
             Time.timeScale = 1f; 
             SceneManager.LoadScene(SceneManager.GetActiveScene().name);
         }
         
         void OnDestroy()
         {
             if (_Game.Scripts.Core.EventManager.current != null)
             {
                 _Game.Scripts.Core.EventManager.current.onPlayerDeadAction -= GameOver;
             }
         }
}