using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuScript : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;

    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;

    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Home (int sceneID)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
        BotMovement.amIABot = false;
        TrainingS.weTraining = false;
        TrainingS.timerOn = false;
        Points.pointsToWin = 0;
        Points.gamesToWin = 0;
    }
}
