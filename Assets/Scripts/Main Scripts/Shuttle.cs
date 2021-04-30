using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shuttle : MonoBehaviour
{
    public GameObject redGlow;
    public GameObject blueGlow;
    public GameObject yellowGlow;
    public GameObject greenGlow;

    bool redBGlow;
    bool blueBGlow;
    bool yellowBGlow;
    bool greenBGlow;

    private void Start()
    {
        redGlow.SetActive(false);
        blueGlow.SetActive(false);
        yellowGlow.SetActive(false);
        greenGlow.SetActive(false);
    }

    private void Update()
    {
        if (redBGlow && transform.position.x < 0)
        {
            redGlow.SetActive(true);
        }
        else
        {
            redGlow.SetActive(false);
        }
        if (blueBGlow && transform.position.x > 0)
        {
            blueGlow.SetActive(true);
        }
        else
        {
            blueGlow.SetActive(false);
        }
        if (yellowBGlow && transform.position.x < 0)
        {
            yellowGlow.SetActive(true);
        }
        else
        {
            yellowGlow.SetActive(false);
        }
        if (greenBGlow && transform.position.x > 0)
        {
            greenGlow.SetActive(true);
        }
        else
        {
            greenGlow.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Checks which player is hitting, which hitbox, and enables hitting
        // P1
        if (other.CompareTag("p1_Overhead"))
        {
            DisableEverything();
            PlayerMovement.strokeState = 0;
            PlayerMovement.strokePossible = 1;
            PlayerMovementTrain.strokeState = 0;
            PlayerMovementTrain.strokePossible = 1;
            BackgroundBot1.strokeState = 0;
            BackgroundBot1.strokePossible = 1;
            DubP1Move.strokeState = 0;
            DubP1Move.strokePossible = 1;

            redBGlow = true;
        }
        else if (other.CompareTag("p1_Drive"))
        {
            DisableEverything();
            PlayerMovement.strokeState = 1;
            PlayerMovement.strokePossible = 1;
            PlayerMovementTrain.strokeState = 1;
            PlayerMovementTrain.strokePossible = 1;
            BackgroundBot1.strokeState = 1;
            BackgroundBot1.strokePossible = 1;
            DubP1Move.strokeState = 1;
            DubP1Move.strokePossible = 1;

            redBGlow = true;
        }
        else if (other.CompareTag("p1_Lift"))
        {
            DisableEverything();
            PlayerMovement.strokeState = 2;
            PlayerMovement.strokePossible = 1;
            PlayerMovementTrain.strokeState = 2;
            PlayerMovementTrain.strokePossible = 1;
            BackgroundBot1.strokeState = 2;
            BackgroundBot1.strokePossible = 1;
            DubP1Move.strokeState = 2;
            DubP1Move.strokePossible = 1;

            redBGlow = true;
        }
        // P2
        else if (other.CompareTag("p2_Overhead"))
        {
            DisableEverything();
            BotMovement.strokeState = 0;
            BotMovement.strokePossible = 1;
            Coach.strokeState = 0;
            Coach.strokePossible = 1;
            BackgroundBot2.strokeState = 0;
            BackgroundBot2.strokePossible = 1;
            DubP2Move.strokeState = 0;
            DubP2Move.strokePossible = 1;

            blueBGlow = true;
        }
        else if (other.CompareTag("p2_Drive"))
        {
            DisableEverything();
            BotMovement.strokeState = 1;
            BotMovement.strokePossible = 1;
            Coach.strokeState = 1;
            Coach.strokePossible = 1;
            BackgroundBot2.strokeState = 1;
            BackgroundBot2.strokePossible = 1;
            DubP2Move.strokeState = 1;
            DubP2Move.strokePossible = 1;

            blueBGlow = true;
        }
        else if (other.CompareTag("p2_Lift"))
        {
            DisableEverything();
            BotMovement.strokeState = 2;
            BotMovement.strokePossible = 1;
            Coach.strokeState = 2;
            Coach.strokePossible = 1;
            BackgroundBot2.strokeState = 2;
            BackgroundBot2.strokePossible = 1;
            DubP2Move.strokeState = 2;
            DubP2Move.strokePossible = 1;

            blueBGlow = true;
        }
        // P3
        else if (other.CompareTag("p3_Overhead"))
        {
            DisableEverything();
            DubP3Move.strokeState = 0;
            DubP3Move.strokePossible = 1;

            yellowBGlow = true;
        }
        else if (other.CompareTag("p3_Drive"))
        {
            DisableEverything();
            DubP3Move.strokeState = 1;
            DubP3Move.strokePossible = 1;

            yellowBGlow = true;
        }
        else if (other.CompareTag("p3_Lift"))
        {
            DisableEverything();
            DubP3Move.strokeState = 2;
            DubP3Move.strokePossible = 1;

            yellowBGlow = true;
        }
        // P4
        else if (other.CompareTag("p4_Overhead"))
        {
            DisableEverything();
            DubP4Move.strokeState = 0;
            DubP4Move.strokePossible = 1;

            greenBGlow = true;
        }
        else if (other.CompareTag("p4_Drive"))
        {
            DisableEverything();
            DubP4Move.strokeState = 1;
            DubP4Move.strokePossible = 1;

            greenBGlow = true;
        }
        else if (other.CompareTag("p4_Lift"))
        {
            DisableEverything();
            DubP4Move.strokeState = 2;
            DubP4Move.strokePossible = 1;

            greenBGlow = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        DisableEverything();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Increments score of player who last hit it if shuttle is in, or the other player if it was out
        if (collision.transform.CompareTag("In") || (collision.transform.CompareTag("BotCollision")))
        {
            if (Points.gameStart)
            {
                Points.landedIn = true;
            }
            DisableEverything();
            ResetShuttle.landed = true;
            TrainingS.landed = 0;
        }
        else if (collision.transform.CompareTag("In2") || (collision.transform.CompareTag("PlayerCollision")))
        {
            if (Points.gameStart)
            {
                Points.landedIn2 = true;
            }
            DisableEverything();
            TrainingS.landed = 1;
            ResetShuttle.landed = true;
        }
        else if (collision.transform.CompareTag("Out"))
        {
            if (Points.gameStart)
            {
                Points.landedOut = true;
            }
            DisableEverything();
            TrainingS.landed = 2;
            ResetShuttle.landed = true;
        }
    }

    void DisableEverything()
    {
        // Always makes it impossible to hit it if it's not in the correct hitboxes
        PlayerMovement.strokeState = 0;
        PlayerMovement.strokePossible = 0;
        BotMovement.strokeState = 0;
        BotMovement.strokePossible = 0;
        PlayerMovementTrain.strokeState = 0;
        PlayerMovementTrain.strokePossible = 0;
        Coach.strokeState = 0;
        Coach.strokePossible = 0;
        BackgroundBot1.strokeState = 0;
        BackgroundBot1.strokePossible = 0;
        BackgroundBot2.strokeState = 0;
        BackgroundBot2.strokePossible = 0;
        DubP1Move.strokeState = 0;
        DubP1Move.strokePossible = 0;
        DubP2Move.strokeState = 0;
        DubP2Move.strokePossible = 0;
        DubP3Move.strokeState = 0;
        DubP3Move.strokePossible = 0;
        DubP4Move.strokeState = 0;
        DubP4Move.strokePossible = 0;
        redBGlow = false;
        blueBGlow = false;
        yellowBGlow = false;
        greenBGlow = false;
    }
}
