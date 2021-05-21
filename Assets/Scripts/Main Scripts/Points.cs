using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Points : MonoBehaviour
{
    public string lastHitter;
    public GameObject shuttle;
    public GameObject refLeft;
    public GameObject refRight;

    // doubles init
    public GameObject singP1;
    public GameObject singP2;
    public GameObject doubP1;
    public GameObject doubP2;
    public GameObject doubP3;
    public GameObject doubP4;

    public GameObject singlesBounds;
    public GameObject doublesBounds;

    // Service transform tools
    public Transform playerLocation;
    public Transform player2Location;
    public Transform[] serviceBoxes;
    public CharacterController playerMovement;
    public CharacterController player2Movement;

    public Transform doubP1Location;
    public Transform doubP2Location;
    public Transform doubP3Location;
    public Transform doubP4Location;
    // doubles bots positions 0 = front, 1 = back, 2 = right 3 = left
    public Transform[] blueServicePoints;
    public Transform[] redServicePoints;
    public Transform doubP2Pos;
    public Transform doubP3Pos;
    public Transform doubP4Pos;
    // doubles char movement
    public CharacterController doubPlayer1Movement;
    public CharacterController doubPlayer2Movement;
    public CharacterController doubPlayer3Movement;
    public CharacterController doubPlayer4Movement;

    int playerScore;
    int player2Score;

    int playerGameWins;
    int player2GameWins;
    int gamesPlayed = 0;

    public static int pointsToWin;
    public static int gamesToWin;

    public static bool landedIn;
    public static bool landedIn2;
    public static bool landedOut;
    public static bool gameStart;
    public static int server;

    public Text playerScoreText;
    public Text player2ScoreText;
    public Text winText;
    public Text playerGameText;
    public Text player2GameText;

    public AudioSource umpireSound;
    public AudioSource audienceSound;
    public AudioClip matchSound;
    public AudioClip gameSound;

    private bool redWonLastGame = false;
    private bool blueWonLastGame = false;

    public static bool doublesOn;
    // variable to check what side players were last on (0 = right, left = 1)
    private int p1LastSide;
    private int p2LastSide;
    public static int doublesReceiver;
    // variable to keep track of double hits
    public static int redHits;
    public static int bluHits;

    // Start is called before the first frame update
    private void Start()
    {
        shuttle.SetActive(false);
        gameStart = false;
        // doubles init
        if (doublesOn == true)
        {
            singP1.SetActive(false);
            singP2.SetActive(false);
            doubP1.SetActive(true);
            doubP2.SetActive(true);
            doubP3.SetActive(true);
            doubP4.SetActive(true);
            doublesBounds.SetActive(true);
            singlesBounds.SetActive(false);
            doublesReceiver = 0;
        }
        else
        {
            singP1.SetActive(true);
            singP2.SetActive(true);
            doubP1.SetActive(false);
            doubP2.SetActive(false);
            doubP3.SetActive(false);
            doubP4.SetActive(false);
            doublesBounds.SetActive(false);
            singlesBounds.SetActive(true);
        }

        bluHits = 0;
        redHits = 0;

        // Resets win text UI
        winText.text = "";
        HarderReset();

        // sets defaults
        if (gamesToWin == 0)
        {
            gamesToWin = 1;
        }
        if (pointsToWin == 0)
        {
            pointsToWin = 7;
        }
    }

    void Update()
    {
        if (gameStart)
        {
            // Increments score of player who last hit it if shuttle is in, or the other player if it was out
            if (landedIn || bluHits > 1)
            {
                playerScore++;
                GameReset();
            }
            else if (landedIn2 || redHits > 1)
            {
                player2Score++;
                GameReset();
            }
            else if (landedOut)
            {
                if (lastHitter == "Player1" || lastHitter == "Player3")
                {
                    player2Score++;
                }
                else if (lastHitter == "Player2" || lastHitter == "Player4")
                {
                    playerScore++;
                }
                GameReset();
            }
        }
    }

    void GameReset()
    {
        // energy recover
        EnergyRecover(10f);

        updateScores();

        if (landedIn)
        {
            StartCoroutine("RefRight");
            // Display conTEXT
            winText.text = "IN! Red wins a point!";

            // Sets server if doubles
            if (doublesOn == true)
            {
                RedDoubleServeWin();
            }
            // singles conditions
            else
            {
                server = 1;

                // Sets service positions
                if (playerScore % 2 == 0)
                {
                    EvenServes();
                }
                else
                {
                    OddServes();
                }
            }
        }
        else if (landedIn2)
        {
            StartCoroutine("RefLeft");
            // Display conTEXT
            winText.text = "IN! Blue wins a point!";

            // Sets server if doubles
            if (doublesOn == true)
            {
                BlueDoubleServeWin();
            }
            else
            {
                server = 2;

                if (player2Score % 2 == 0)
                {
                    EvenServes();
                }
                else
                {
                    OddServes();
                }
            }
        }
        else if ((lastHitter == "Player1" || lastHitter == "Player3") && landedOut == true)
        {
            StartCoroutine("RefLeft");
            // Display conTEXT
            winText.text = "OUT! Blue wins a point!";

            // Sets server if doubles
            if (doublesOn == true)
            {
                BlueDoubleServeWin();
            }
            else
            {
                server = 2;

                if (player2Score % 2 == 0)
                {
                    EvenServes();
                }
                else
                {
                    OddServes();
                }
            }
        }
        else if ((lastHitter == "Player2" || lastHitter == "Player4") && landedOut == true)
        {
            StartCoroutine("RefRight");
            winText.text = "OUT! Red wins a point!";

            // Sets server if doubles
            if (doublesOn == true)
            {
                RedDoubleServeWin();
            }
            // singles conditions
            else
            {
                server = 1;

                // Sets service positions
                if (playerScore % 2 == 0)
                {
                    EvenServes();
                }
                else
                {
                    OddServes();
                }
            }
        }
        else if (redHits > 1)
        {
            StartCoroutine("RefLeft");
            // Display conTEXT
            winText.text = "FAULT! Blue wins a point!";

            // Sets server if doubles
            if (doublesOn == true)
            {
                BlueDoubleServeWin();
            }
            else
            {
                server = 2;

                if (player2Score % 2 == 0)
                {
                    EvenServes();
                }
                else
                {
                    OddServes();
                }
            }
        }
        else if (bluHits > 1)
        {
            StartCoroutine("RefRight");
            // Display conTEXT
            winText.text = "FAULT! Red wins a point!";

            // Sets server if doubles
            if (doublesOn == true)
            {
                RedDoubleServeWin();
            }
            // singles conditions
            else
            {
                server = 1;

                // Sets service positions
                if (playerScore % 2 == 0)
                {
                    EvenServes();
                }
                else
                {
                    OddServes();
                }
            }
        }

        // Game
        if (playerScore >= pointsToWin && playerScore - player2Score >= 2)
        {
            StartCoroutine("RefRight");
            // Display win text
            winText.text = "Red wins this game!";
            playerGameWins++;
            gamesPlayed++;
            // service check
            redWonLastGame = true;
            blueWonLastGame = false;
            // Match
            if (playerGameWins == gamesToWin)
            {
                // Display win text
                winText.text = "Red wins this match!";
                HarderReset();
                StartCoroutine("GameOver");
            }
            else
            {
                HardReset();
            }
        }
        else if (player2Score >= pointsToWin && player2Score - playerScore >= 2)
        {
            StartCoroutine("RefLeft");
            // Display win text
            winText.text = "Blue wins this game!";
            player2GameWins++;
            gamesPlayed++;
            // service check
            redWonLastGame = false;
            blueWonLastGame = true;
            // Match
            if (player2GameWins == gamesToWin)
            {
                // Display win text
                winText.text = "Blue wins this match!";
                HarderReset();
                StartCoroutine("GameOver");
            }
            else
            {
                HardReset();
            }
        }

        //Resets game state
        landedIn = false;
        landedIn2 = false;
        landedOut = false;
        gameStart = false;
        shuttle.SetActive(false);
        PlayerMovement.strokeState = 0;
        PlayerMovement.strokePossible = 0;
        BotMovement.strokeState = 0;
        BotMovement.strokePossible = 0;
        DubP1Move.strokeState = 0;
        DubP1Move.strokePossible = 0;
        DubP2Move.strokeState = 0;
        DubP2Move.strokePossible = 0;
        DubP3Move.strokeState = 0;
        DubP3Move.strokePossible = 0;
        DubP4Move.strokeState = 0;
        DubP4Move.strokePossible = 0;
        bluHits = 0;
        redHits = 0;
    }

    void HardReset()
    {
        // energy recover
        EnergyRecover(75f);

        // Plays clip
        audienceSound.clip = gameSound;
        audienceSound.Play();

        // Reset player scores
        playerScore = 0;
        player2Score = 0;

        // Reset for Doubles
        if (doublesOn == true)
        {
            // randomizes who serves first, if red won last then red will serve
            if (redWonLastGame == true)
            {
                server = Random.Range(0, 2);
                if (server == 0)
                {
                    server = 1;
                }
                else
                {
                    server = 3;
                }
            }
            else if (blueWonLastGame == true)
            {
                server = Random.Range(0, 2);
                if (server == 0)
                {
                    server = 2;
                }
                else
                {
                    server = 4;
                }
            }
            else
            {
                server = Random.Range(1, 5);
            }
            // initializes 'last' positions
            if (server == 1)
            {
                p1LastSide = 0;
                p2LastSide = Random.Range(0, 2);
            }
            else if (server == 2)
            {
                p1LastSide = Random.Range(0, 2);
                p2LastSide = 0;
            }
            else if (server == 3)
            {
                p1LastSide = 1;
                p2LastSide = Random.Range(0, 2);
            }
            else if (server == 4)
            {
                p1LastSide = Random.Range(0, 2);
                p2LastSide = 1;
            }
            DoublesServePositions();
            bluHits = 0;
            redHits = 0;
        }
        // Reset for Singles
        else
        {
            // Sets starting positions
            playerMovement.enabled = false;
            player2Movement.enabled = false;
            playerLocation.transform.position = serviceBoxes[1].position;
            player2Location.transform.position = serviceBoxes[2].position;
            playerMovement.enabled = true;
            player2Movement.enabled = true;

            // Randomises who serves first
            if (redWonLastGame == true)
            {
                server = 1;
            }
            else if (blueWonLastGame == true)
            {
                server = 2;
            }
            else
            {
                server = Random.Range(1, 3);
            }
        }
        updateScores();
    }

    void HarderReset()
    {
        playerGameWins = 0;
        player2GameWins = 0;

        HardReset();
    }

    void EvenServes()
    {
        playerMovement.enabled = false;
        player2Movement.enabled = false;
        playerLocation.transform.position = serviceBoxes[1].position;
        player2Location.transform.position = serviceBoxes[2].position;
        playerMovement.enabled = true;
        player2Movement.enabled = true;
    }

    void OddServes()
    {
        playerMovement.enabled = false;
        player2Movement.enabled = false;
        playerLocation.transform.position = serviceBoxes[0].position;
        player2Location.transform.position = serviceBoxes[3].position;
        playerMovement.enabled = true;
        player2Movement.enabled = true;
    }

    // 2 functions for if red or blue wins point
    void RedDoubleServeWin()
    {
        if (server == 1) // if last server was p1, then he continues serving but flips sides
        {
            if (p1LastSide == 0)
            {
                p1LastSide = 1;
            }
            else
            {
                p1LastSide = 0;
            }
        }
        else if (server == 3) // if it was p3, same thing
        {
            if (p1LastSide == 0)
            {
                p1LastSide = 1;
            }
            else
            {
                p1LastSide = 0;
            }
        }
        else
        { // if it was opponents, check score
            if (playerScore % 2 == 0) // if even then if p1 was on right side, he serves
            {
                if (p1LastSide == 0)
                {
                    server = 1;
                }
                else
                {
                    server = 3; // otherwise its p3
                }
            }
            else
            {
                if (p1LastSide == 1) // same for odds
                {
                    server = 1;
                }
                else
                {
                    server = 3;
                }
            }
        }
        DoublesServePositions();
    }
    void BlueDoubleServeWin()
    {
        if (server == 2) // if last server was p1, then he continues serving but flips sides
        {
            if (p2LastSide == 0)
            {
                p2LastSide = 1;
            }
            else
            {
                p2LastSide = 0;
            }
        }
        else if (server == 4) // if it was p3, same thing
        {
            if (p2LastSide == 0)
            {
                p2LastSide = 1;
            }
            else
            {
                p2LastSide = 0;
            }
        }
        else
        { // if it was opponents, check score
            if (player2Score % 2 == 0) // if even then if p1 was on right side, he serves
            {
                if (p2LastSide == 0)
                {
                    server = 2;
                }
                else
                {
                    server = 4; // otherwise its p3
                }
            }
            else
            {
                if (p2LastSide == 1) // same for odds
                {
                    server = 2;
                }
                else
                {
                    server = 4;
                }
            }
        }
        DoublesServePositions();
    }

    // Updates the UI text to reflect scores
    void updateScores()
    {
        playerScoreText.text = "Red: " + playerScore;
        player2ScoreText.text = "Blue: " + player2Score;
        playerGameText.text = "" + playerGameWins;
        player2GameText.text = "" + player2GameWins;
    }

    void DoublesServePositions()
    {
        doubPlayer1Movement.enabled = false;
        doubPlayer2Movement.enabled = false;
        doubPlayer3Movement.enabled = false;
        doubPlayer4Movement.enabled = false;

        // red serve
        if (server == 1)
        {
            if (playerScore % 2 == 0)
            {
                // position of players
                doubP1Location.transform.position = serviceBoxes[5].transform.position;
                doubP3Location.transform.position = serviceBoxes[6].transform.position;
                doubP3Pos.transform.position = redServicePoints[1].transform.position;
                p1LastSide = 0;

                // checks position of where they were
                if (p2LastSide == 0)
                {
                    doubP2Location.transform.position = serviceBoxes[8].transform.position;
                    doubP4Location.transform.position = serviceBoxes[9].transform.position;
                    doubP2Pos.transform.position = blueServicePoints[0].transform.position;
                    doubP4Pos.transform.position = blueServicePoints[1].transform.position;
                    p2LastSide = 0;
                    doublesReceiver = 2;
                }
                else
                {
                    doubP4Location.transform.position = serviceBoxes[8].transform.position;
                    doubP2Location.transform.position = serviceBoxes[9].transform.position;
                    doubP4Pos.transform.position = blueServicePoints[0].transform.position;
                    doubP2Pos.transform.position = blueServicePoints[1].transform.position;
                    p2LastSide = 1;
                    doublesReceiver = 4;
                }
            }
            else
            {
                // position of players
                doubP1Location.transform.position = serviceBoxes[4].transform.position;
                doubP3Location.transform.position = serviceBoxes[6].transform.position;
                doubP3Pos.transform.position = redServicePoints[1].transform.position;
                p1LastSide = 1;

                // checks position of where they were
                if (p2LastSide == 0)
                {
                    doubP4Location.transform.position = serviceBoxes[7].transform.position;
                    doubP2Location.transform.position = serviceBoxes[9].transform.position;
                    doubP4Pos.transform.position = blueServicePoints[0].transform.position;
                    doubP2Pos.transform.position = blueServicePoints[1].transform.position;
                    p2LastSide = 0;
                    doublesReceiver = 4;
                }
                else
                {
                    doubP2Location.transform.position = serviceBoxes[7].transform.position;
                    doubP4Location.transform.position = serviceBoxes[9].transform.position;
                    doubP2Pos.transform.position = blueServicePoints[0].transform.position;
                    doubP4Pos.transform.position = blueServicePoints[1].transform.position;
                    p2LastSide = 1;
                    doublesReceiver = 2;
                }
            }
        }
        else if (server == 3)
        {
            if (playerScore % 2 == 0)
            {
                // position of players
                doubP1Location.transform.position = serviceBoxes[6].transform.position;
                doubP3Location.transform.position = serviceBoxes[5].transform.position;
                doubP3Pos.transform.position = redServicePoints[0].transform.position;
                p1LastSide = 1;

                // checks position of where they were
                if (p2LastSide == 0)
                {
                    doubP2Location.transform.position = serviceBoxes[8].transform.position;
                    doubP4Location.transform.position = serviceBoxes[9].transform.position;
                    doubP2Pos.transform.position = blueServicePoints[0].transform.position;
                    doubP4Pos.transform.position = blueServicePoints[1].transform.position;
                    p2LastSide = 0;
                    doublesReceiver = 2;
                }
                else
                {
                    doubP4Location.transform.position = serviceBoxes[8].transform.position;
                    doubP2Location.transform.position = serviceBoxes[9].transform.position;
                    doubP4Pos.transform.position = blueServicePoints[0].transform.position;
                    doubP2Pos.transform.position = blueServicePoints[1].transform.position;
                    p2LastSide = 1;
                    doublesReceiver = 4;
                }
            }
            else
            {
                // position of players
                doubP1Location.transform.position = serviceBoxes[6].transform.position;
                doubP3Location.transform.position = serviceBoxes[4].transform.position;
                doubP3Pos.transform.position = redServicePoints[0].transform.position;
                p1LastSide = 0;

                // checks position of where they were
                if (p2LastSide == 0)
                {
                    doubP4Location.transform.position = serviceBoxes[7].transform.position;
                    doubP2Location.transform.position = serviceBoxes[9].transform.position;
                    doubP4Pos.transform.position = blueServicePoints[0].transform.position;
                    doubP2Pos.transform.position = blueServicePoints[1].transform.position;
                    p2LastSide = 0;
                    doublesReceiver = 4;
                }
                else
                {
                    doubP2Location.transform.position = serviceBoxes[7].transform.position;
                    doubP4Location.transform.position = serviceBoxes[9].transform.position;
                    doubP2Pos.transform.position = blueServicePoints[0].transform.position;
                    doubP4Pos.transform.position = blueServicePoints[1].transform.position;
                    p2LastSide = 1;
                    doublesReceiver = 2;
                }
            }
        }
        // blue serve
        else if (server == 2)
        {
            if (player2Score % 2 == 0)
            {
                // position of players
                doubP2Location.transform.position = serviceBoxes[8].transform.position;
                doubP4Location.transform.position = serviceBoxes[9].transform.position;
                doubP2Pos.transform.position = blueServicePoints[0].transform.position;
                doubP4Pos.transform.position = blueServicePoints[1].transform.position;
                p2LastSide = 0;

                // checks position of where they were
                if (p1LastSide == 0)
                {
                    doubP1Location.transform.position = serviceBoxes[5].transform.position;
                    doubP3Location.transform.position = serviceBoxes[6].transform.position;
                    doubP3Pos.transform.position = redServicePoints[1].transform.position;
                    p1LastSide = 0;
                    doublesReceiver = 1;
                }
                else
                {
                    doubP3Location.transform.position = serviceBoxes[5].transform.position;
                    doubP1Location.transform.position = serviceBoxes[6].transform.position;
                    doubP3Pos.transform.position = redServicePoints[0].transform.position;
                    p1LastSide = 1;
                    doublesReceiver = 3;
                }
            }
            else
            {
                // position of players
                doubP2Location.transform.position = serviceBoxes[7].transform.position;
                doubP4Location.transform.position = serviceBoxes[9].transform.position;
                doubP2Pos.transform.position = blueServicePoints[0].transform.position;
                doubP4Pos.transform.position = blueServicePoints[1].transform.position;
                p2LastSide = 1;

                // checks position of where they were
                if (p1LastSide == 0)
                {
                    doubP3Location.transform.position = serviceBoxes[4].transform.position;
                    doubP1Location.transform.position = serviceBoxes[6].transform.position;
                    doubP3Pos.transform.position = redServicePoints[0].transform.position;
                    p1LastSide = 0;
                    doublesReceiver = 3;
                }
                else
                {
                    doubP1Location.transform.position = serviceBoxes[4].transform.position;
                    doubP3Location.transform.position = serviceBoxes[6].transform.position;
                    doubP3Pos.transform.position = redServicePoints[1].transform.position;
                    p1LastSide = 1;
                    doublesReceiver = 1;
                }
            }
        }
        else if (server == 4)
        {
            if (player2Score % 2 == 0)
            {
                // position of players
                doubP2Location.transform.position = serviceBoxes[9].transform.position;
                doubP4Location.transform.position = serviceBoxes[8].transform.position;
                doubP4Pos.transform.position = blueServicePoints[0].transform.position;
                doubP2Pos.transform.position = blueServicePoints[1].transform.position;
                p2LastSide = 1;

                // checks position of where they were
                if (p1LastSide == 0)
                {
                    doubP1Location.transform.position = serviceBoxes[5].transform.position;
                    doubP3Location.transform.position = serviceBoxes[6].transform.position;
                    doubP3Pos.transform.position = redServicePoints[1].transform.position;
                    p1LastSide = 0;
                    doublesReceiver = 1;
                }
                else
                {
                    doubP3Location.transform.position = serviceBoxes[5].transform.position;
                    doubP1Location.transform.position = serviceBoxes[6].transform.position;
                    doubP3Pos.transform.position = redServicePoints[0].transform.position;
                    p1LastSide = 1;
                    doublesReceiver = 3;
                }
            }
            else
            {
                // position of players
                doubP2Location.transform.position = serviceBoxes[9].transform.position;
                doubP4Location.transform.position = serviceBoxes[7].transform.position;
                doubP4Pos.transform.position = blueServicePoints[0].transform.position;
                doubP2Pos.transform.position = blueServicePoints[1].transform.position;
                p2LastSide = 0;

                // checks position of where they were
                if (p1LastSide == 0)
                {
                    doubP3Location.transform.position = serviceBoxes[4].transform.position;
                    doubP1Location.transform.position = serviceBoxes[6].transform.position;
                    doubP3Pos.transform.position = redServicePoints[0].transform.position;
                    p1LastSide = 0;
                    doublesReceiver = 3;
                }
                else
                {
                    doubP1Location.transform.position = serviceBoxes[4].transform.position;
                    doubP3Location.transform.position = serviceBoxes[6].transform.position;
                    doubP3Pos.transform.position = redServicePoints[1].transform.position;
                    p1LastSide = 1;
                    doublesReceiver = 1;
                }
            }
        }

        doubPlayer1Movement.enabled = true;
        doubPlayer2Movement.enabled = true;
        doubPlayer3Movement.enabled = true;
        doubPlayer4Movement.enabled = true;
    }

    void EnergyRecover(float amount)
    {
        // resets energy levels
        if (doublesOn)
        {
            DubP1Move.energyCurr = Mathf.Min(DubP1Move.energyMax, DubP1Move.energyCurr + amount);
            DubP2Move.energyCurr = Mathf.Min(DubP2Move.energyMax, DubP2Move.energyCurr + amount);
            DubP3Move.energyCurr = Mathf.Min(DubP3Move.energyMax, DubP3Move.energyCurr + amount);
            DubP4Move.energyCurr = Mathf.Min(DubP4Move.energyMax, DubP4Move.energyCurr + amount);
        }
        else
        {
            PlayerMovement.energyCurr = Mathf.Min(PlayerMovement.energyMax, PlayerMovement.energyCurr + amount);
            BotMovement.energyCurr = Mathf.Min(BotMovement.energyMax, BotMovement.energyCurr + amount);
        }
    }

    private float PlayerEXP(int level)
    {
        return 5 * Mathf.Pow(2.5f, Mathf.Sqrt(level - 1));
    }

    IEnumerator GameOver()
    {
        if (BotMovement.amIABot == true)
        {
            // determines xp based on number of games, etc
            float baseXP = pointsToWin * gamesToWin + gamesPlayed * 2;
            float modXP = (BotMovement.difficultyLevel + 1) * 0.75f * baseXP;
            // EXP statements. If you win you get exp, if you lose you get 25% of exp
            if (redWonLastGame == true)
            {
                PlayerMovement.playerEXP += modXP;
            }
            else
            {
                PlayerMovement.playerEXP += modXP * 0.25f;
            }
            
            // Checks to see if player exp is greater than required to level up, max level is 30
            if (PlayerMovement.playerEXP >= PlayerEXP(PlayerMovement.playerLevel) && PlayerMovement.playerLevel < 30)
            {
                PlayerMovement.playerEXP -= PlayerEXP(PlayerMovement.playerLevel);
                PlayerMovement.playerLevel++;
                PlayerMovement.playerPoints += 5;
            }
        }
        gamesPlayed = 0;
        pointsToWin = 0;
        gamesToWin = 0;
        SaveSystem.SavePlayer();
        yield return new WaitForSeconds(2f);
        BotMovement.amIABot = false;
        SceneManager.LoadScene(0);
    }

    IEnumerator RefLeft()
    {
        // Plays clip
        umpireSound.Play();
        refLeft.SetActive(true);
        yield return new WaitForSeconds(1f);
        refLeft.SetActive(false);
    }
    IEnumerator RefRight()
    {
        // Plays clip
        umpireSound.Play();
        refRight.SetActive(true);
        yield return new WaitForSeconds(1f);
        refRight.SetActive(false);
    }
}
