using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject gamePausePanel;
    public void ToGame()
    {
        // To play the Game
        SceneManager.LoadScene("Game");
    }
    public void ResumeGame()
    {
        Debug.Log("Game Resumed");
        gamePausePanel.SetActive(false);
    }

    // To quit the game
    public void ExitGame()
    {
        Debug.Log("Game Exited");
        Application.Quit();
    }
    public void GamePause()
    {
        Debug.Log("Game Paused");
        gamePausePanel.SetActive(true);       
    }
    

}
