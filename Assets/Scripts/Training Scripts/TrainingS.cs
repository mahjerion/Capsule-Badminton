using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TrainingS : MonoBehaviour
{
    public static int landed;
    public string lastHitter;
    public Text winText;

    public static bool startTraining = false;
    public static string trainingType;
    public static bool shuttleNotHitYet = true;

    public Transform[] feedPoints;
    public Transform[] movePoints;
    public Transform coachTrans;
    public Transform shuttleTrans;
    public Transform playerTrans;
    public GameObject shuttleObject;

    public GameObject pointsObject;
    public Text timerText;
    public float timer;
    public static bool pointTaken = false;
    private bool isThisBase = false;
    public Transform modPosition;
    public static bool timerOn = false;

    public static bool weTraining;

    private bool coroutineRan = false;
    private bool movementCor = false;

    // Start is called before the first frame update
    void Start()
    {
        if (trainingType == "move")
        {
            timerText.text = "";
            timerOn = false;
            isThisBase = false;
            pointTaken = false;
        }
        landed = 5;
        startTraining = false;
        weTraining = true;
        coroutineRan = false;
        movementCor = false;
        StartCoroutine("Initialization");
    }

    // Update is called once per frame
    void Update()
    {
        if (trainingType == "move")
        {
            if (movementCor == false)
            {
                StartCoroutine("TimerOn");
            }
            if (pointTaken == false && timerOn == true)
            {
                pointTaken = true;
                if (isThisBase == false)
                {
                    int position = Random.Range(0, 8);
                    modPosition.position = new Vector3(movePoints[position].position.x, 0.5f, movePoints[position].position.z);
                    Instantiate(pointsObject, modPosition);
                    isThisBase = true;
                }
                else
                {
                    modPosition.position = new Vector3(movePoints[8].position.x, 0.5f, movePoints[8].position.z);
                    Instantiate(pointsObject, modPosition);
                    isThisBase = false;
                }
            }
        }
        else // RACQUET PART
        {
            if (landed == 0)
            {
                winText.text = "Nice shot!";
                if (coroutineRan == false)
                {
                    StartCoroutine("ResetPlayer");
                }
            }
            else if (landed == 1)
            {
                winText.text = "Get to the shuttle!";
                if (coroutineRan == false)
                {
                    StartCoroutine("ResetPlayer");
                }
            }
            else if (landed == 2)
            {
                winText.text = "Position yourself better!";
                if (coroutineRan == false)
                {
                    StartCoroutine("ResetPlayer");
                }
            }

            if (startTraining == true && trainingType == "racquet")
            {
                if (landed == 3)
                {
                    landed = 4;
                    StartCoroutine("SpawnShuttle");
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (timerOn == true)
        {
            timer--;
            timerText.text = "" + Mathf.Round(timer/60);

            if (timer == 0)
            {
                timerOn = false;
                StartCoroutine("EndFootwork");
            }
        }
    }

    IEnumerator ResetPlayer()
    {
        coroutineRan = true;
        landed = 5;
        shuttleTrans.position = new Vector3(0, 5, 0);
        shuttleObject.SetActive(false);
        lastHitter = "Player1";
        yield return new WaitForSeconds(1f);
        shuttleNotHitYet = true;
        landed = 3;
        coroutineRan = false;
    }

    IEnumerator Initialization()
    {
        if (trainingType == "move")
        {
            winText.text = "Collect the cubes! Make sure to return to base position!";
        }
        else if (trainingType == "racquet")
        {
            winText.text = "Return the shots - keep them in!";
        }
        yield return new WaitForSeconds(2f);
        landed = 3;
        startTraining = true;
    }

    IEnumerator SpawnShuttle()
    {
        int randomPos = Random.Range(0, 3);
        coachTrans.position = feedPoints[randomPos].position + new Vector3(0, 0.8f, 0);
        yield return new WaitForSeconds(0.25f);
        shuttleObject.SetActive(true);
        shuttleTrans.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        shuttleTrans.position = coachTrans.position + new Vector3(0, 5f, 0);
    }

    IEnumerator TimerOn()
    {
        movementCor = true;
        yield return new WaitForSecondsRealtime(1f);
        timerText.text = "3";
        yield return new WaitForSecondsRealtime(1f);
        timerText.text = "2";
        yield return new WaitForSecondsRealtime(1f);
        timerText.text = "1";
        yield return new WaitForSecondsRealtime(1f);
        timerText.text = "GO!";
        yield return new WaitForSecondsRealtime(1f);
        timer = 1800f;
        timerOn = true;
    }

    IEnumerator EndFootwork()
    {
        winText.text = "Nice job! " + PlayerMovementTrain.points + " points!";
        yield return new WaitForSecondsRealtime(3f);
        weTraining = false;
        timerOn = false;
        timerText.text = "";
        SceneManager.LoadScene(0);
    }
}
