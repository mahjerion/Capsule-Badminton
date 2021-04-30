using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public GameObject adObject;
    private void Start()
    {
        if (SinglePlayerCamera.isMobile == true)
        {
            adObject.SetActive(true);
        }
    }
    // Allows previous scene to be loaded and resets game components
    public void ReturnToMain()
    {
        SceneManager.LoadScene(0);
        BotMovement.amIABot = false;
        TrainingS.weTraining = false;
        TrainingS.timerOn = false;
        Points.pointsToWin = 0;
        Points.gamesToWin = 0;
    }
}
