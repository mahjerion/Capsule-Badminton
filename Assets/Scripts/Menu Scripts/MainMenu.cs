using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public AudioSource clickSound;
    public GameObject backgroundObject;
    public GameObject backSceneObject;

    private void Awake()
    {        
        // init for some saved options
        if (ES3.KeyExists("musicOn"))
        {
            MusicOptionMenu.menuMusic = ES3.Load<bool>("musicOn");
        }
        else
        {
            MusicOptionMenu.menuMusic = true;
        }
        if (ES3.KeyExists("cameraOn"))
        {
            SinglePlayerCamera.altCamera = ES3.Load<bool>("cameraOn");
        }
        else
        {
            SinglePlayerCamera.altCamera = true;
        }
        if (ES3.KeyExists("qualityLevel"))
        {
            QualityOptionMenu.qualityMenu = ES3.Load<int>("qualityLevel");
            QualitySettings.SetQualityLevel(QualityOptionMenu.qualityMenu);
        }
        else
        {
            QualityOptionMenu.qualityMenu = 2;
            QualitySettings.SetQualityLevel(QualityOptionMenu.qualityMenu);
        }
        if (ES3.KeyExists("staticBack"))
        {
            BackgroundOptionMenu.staticBackground = ES3.Load<bool>("staticBack");
            if (BackgroundOptionMenu.staticBackground == true)
            {
                backgroundObject.SetActive(true);
                backSceneObject.SetActive(false);
            }
        }
        else
        {
            BackgroundOptionMenu.staticBackground = false;
        }
    }

    // Allows next scene to be loaded but with a bot (game)
    public void PlayGameEasy()
    {
        Points.doublesOn = false;
        SinglePlayerCamera.coopBots = false;
        SinglePlayerCamera.twoPlayerCam = false;
        BotMovement.amIABot = true;
        BotMovement.difficultyLevel = 0;
        PlayerMovement.playerType = 10;
    }
    public void PlayGameMedium()
    {
        Points.doublesOn = false;
        SinglePlayerCamera.coopBots = false;
        SinglePlayerCamera.twoPlayerCam = false;
        BotMovement.amIABot = true;
        BotMovement.difficultyLevel = 1;
        PlayerMovement.playerType = 10;
    }
    public void PlayGameHard()
    {
        Points.doublesOn = false;
        SinglePlayerCamera.coopBots = false;
        SinglePlayerCamera.twoPlayerCam = false;
        BotMovement.amIABot = true;
        BotMovement.difficultyLevel = 2;
        PlayerMovement.playerType = 10;
    }
    public void PlayGameVeryHard()
    {
        Points.doublesOn = false;
        SinglePlayerCamera.coopBots = false;
        SinglePlayerCamera.twoPlayerCam = false;
        BotMovement.amIABot = true;
        BotMovement.difficultyLevel = 3;
        PlayerMovement.playerType = 10;
    }
    public void PlayGameUnbeatable()
    {
        Points.doublesOn = false;
        SinglePlayerCamera.coopBots = false;
        SinglePlayerCamera.twoPlayerCam = false;
        BotMovement.amIABot = true;
        BotMovement.difficultyLevel = 4;
        PlayerMovement.playerType = 10;
    }

    public void PlayGameSecret()
    {
        Points.doublesOn = false;
        SinglePlayerCamera.coopBots = false;
        SinglePlayerCamera.twoPlayerCam = false;
        BotMovement.amIABot = true;
        BotMovement.difficultyLevel = 6;
        PlayerMovement.playerType = 10;
    }

    // Doubles Bots PlayGame
    public void DubGameEasy()
    {
        Points.doublesOn = true;
        BotMovement.amIABot = true;
        BotMovement.difficultyLevel = 0;
    }
    public void DubGameMedium()
    {
        Points.doublesOn = true;
        BotMovement.amIABot = true;
        BotMovement.difficultyLevel = 1;
    }
    public void DubGameHard()
    {
        Points.doublesOn = true;
        BotMovement.amIABot = true;
        BotMovement.difficultyLevel = 2;
    }
    public void DubGameVeryHard()
    {
        Points.doublesOn = true;
        BotMovement.amIABot = true;
        BotMovement.difficultyLevel = 3;
    }
    public void DubGameUnbeatable()
    {
        Points.doublesOn = true;
        BotMovement.amIABot = true;
        BotMovement.difficultyLevel = 4;
    }

    // Allows next scene to be loaded (game)
    public void PlayGame()
    {
        SaveSystem.SavePlayer();
        //MobileScript.isMobile = true;
        SceneManager.LoadScene(1);
    }

    // Allows next scene to be loaded (game)
    public void Play2PGame()
    {
        DubP3Move.amIABot = true;
        BotMovement.amIABot = false;
        Points.doublesOn = false;
        SinglePlayerCamera.twoPlayerCam = true;
        SinglePlayerCamera.coopBots = false;
    }

    public void TrainingMove()
    {
        TrainingS.trainingType = "move";
        SceneManager.LoadScene(2);

    }

    public void SinglePlayerDub()
    {
        DubP3Move.amIABot = true;
        Points.doublesOn = true;
        BotMovement.amIABot = true;
        SinglePlayerCamera.coopBots = false;
        SinglePlayerCamera.twoPlayerCam = false;
    }

    public void WithBotPlayDoublesGame()
    {
        DubP3Move.amIABot = false;
        Points.doublesOn = true;
        BotMovement.amIABot = true;
        SinglePlayerCamera.coopBots = true;
        SinglePlayerCamera.twoPlayerCam = true;
    }

    public void PlayDoublesGame()
    {
        DubP3Move.amIABot = false;
        Points.doublesOn = true;
        BotMovement.amIABot = false;
        SinglePlayerCamera.coopBots = false;
        SinglePlayerCamera.twoPlayerCam = false;
    }

    public void TrainingRacquet()
    {
        TrainingS.trainingType = "racquet";
        SceneManager.LoadScene(2);
    }

    public void QuitGame()
    {
        SaveSystem.SavePlayer();
        Application.Quit();
    }

    private void Start()
    {
        PlayerData data = SaveSystem.LoadPlayer();

        PlayerMovement.playerLevel = data.level;
        PlayerMovement.playerEXP = data.exp;
        PlayerMovement.playerPoints = data.points;
        PlayerMovement.speedBoost = data.spdBST;
        PlayerMovement.accuracyBoost = data.accBST;
        PlayerMovement.smashPowerBoost = data.pwrBST;
        PlayerMovement.energyBoost = data.nrgBST;
    }

    public void RESETGame()
    {
        PlayerMovement.playerLevel = 0;
        PlayerMovement.playerEXP = 0;
        PlayerMovement.playerPoints = 0;
        PlayerMovement.speedBoost = 0;
        PlayerMovement.accuracyBoost = 0;
        PlayerMovement.smashPowerBoost = 0;

        SaveSystem.SavePlayer();
    }

    public void SaveGame()
    {
        SaveSystem.SavePlayer();
    }

    public void ClickSound()
    {
        clickSound.Play();
    }
}
