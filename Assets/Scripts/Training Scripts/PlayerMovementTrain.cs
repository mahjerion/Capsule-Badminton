using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class PlayerMovementTrain : MonoBehaviour
{
    CharacterController playerOne;
    public AudioSource clickSound;

    public Text pointsText;
    public static int points;

    public Transform training;
    public Transform shuttleTrans;
    public Transform[] targets;

    private Vector3 moveDirection = Vector3.zero;
    private Animator playerAnim;

    // Variables to store if it's possible to hit shuttle (if it's in hitbox) and which hitbox it's coming from
    public static int strokeState;
    public static int strokePossible;

    // Sets base shot to be in middle
    private int targetX;

    // Audio source
    public AudioSource racquetSound;
    public AudioClip liftSound;
    public AudioClip smashSound;

    // Basic stat init.
    private float jumpSpeed = 4f;
    private float gravity = 9.8f;
    private float speed = 3.1f;
    private float inaccuracy = 0.3f;
    [SerializeField]
    private float extraJumpPower = 1.1f;

    // Allow movement bool where if it's true they can do stuff. Is set to false whenever they
    // hit a key
    private bool allowMovement;

    // Assigns ShotManager so it can be edited
    ShotManager shotManager;
    Shot currentShot;

    // Rewired stuff
    private Player player;

    // camera variables
    float horizontal;
    float vertical;

    // test for fixed update
    bool drop;
    bool drive;
    bool clear;
    bool jump;
    bool detectHit;

    // Start is called before the first frame update
    void Start()
    {
        // rewired init
        player = ReInput.players.GetPlayer(0);

        // movement training
        if (TrainingS.trainingType == "move")
        {
            points = 0;
            SetPointText();
        }
        else
        {
            pointsText.text = "";
        }

        // Initialize and get component for players
        strokePossible = 0;
        strokeState = 0;
        playerOne = GetComponent<CharacterController>();
        shotManager = GetComponent<ShotManager>();
        currentShot = shotManager.clear;

        // Animator Component Retrieval
        playerAnim = GetComponent<Animator>();

        // Movement initialize
        allowMovement = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (SinglePlayerCamera.altCamera)
        {
            horizontal = player.GetAxis("Move Vertical");
            vertical = -player.GetAxis("Move Horizontal");
        }
        else
        {
            horizontal = player.GetAxis("Move Horizontal");
            vertical = player.GetAxis("Move Vertical");
        }

        if (allowMovement == true && TrainingS.startTraining == true)
        {
            if (TrainingS.trainingType == "move" && TrainingS.timerOn == false)
            {
                moveDirection.x = 0.0f;
                moveDirection.z = 0.0f;
            }
            else
            {
                if (player.GetButtonDown("Clear"))
                {
                    clear = true;
                    StartCoroutine("InputBuffer");
                }
                if (player.GetButtonDown("Drive"))
                {
                    drive = true;
                    StartCoroutine("InputBuffer");
                }
                if (player.GetButtonDown("Drop"))
                {
                    drop = true;
                    StartCoroutine("InputBuffer");
                }
                // Checks to see if player is grounded, then checks for input for movement
                if (playerOne.isGrounded)
                {
                    // for some reason the horizontal axis is reversed when using the joystick, code aims to fix that
                    moveDirection = new Vector3(vertical, 0.0f, horizontal);

                    moveDirection *= speed;

                    // Jump key = jump
                    if (player.GetButton("Jump"))
                    {
                        moveDirection.x /= 2;
                        moveDirection.z /= 2;
                        moveDirection.y = jumpSpeed;
                    }
                }
            }
        }
        if (allowMovement == false && playerOne.isGrounded)
        {
            // Force of gravity applied per frame update - outside because gravity always affects
            moveDirection.x = 0.0f;
            moveDirection.z = 0.0f;
        }
        moveDirection.y -= gravity * Time.deltaTime;
        // Moves the player
        playerOne.Move(moveDirection * Time.deltaTime);

        if (TrainingS.landed == 5)
        {
            transform.position = targets[9].position;
            moveDirection = new Vector3(0, 0, 0);
            StartCoroutine("Receivewait");
        }
    }

    private void FixedUpdate()
    {
        if (detectHit)
        {
            if (strokePossible == 1 && shuttleTrans.position.x < 0f)
            {
                if (clear)
                {
                    clear = false;
                    currentShot = shotManager.clear;
                    PlayerContact();
                }
                if (drive)
                {
                    drive = false;
                    currentShot = shotManager.drive;
                    PlayerContact();
                }
                if (drop)
                {
                    drop = false;
                    currentShot = shotManager.drop;
                    PlayerContact();
                }
            }
        }
    }

    IEnumerator InputBuffer()
    {
        detectHit = true;
        playerAnim.Play("Ready Position");
        StartCoroutine("FrameWait");
        yield return new WaitForSeconds(0.2f);
        clear = false;
        drive = false;
        drop = false;
        detectHit = false;
    }

    private void PlayerContact()
    {
        if (training.GetComponent<TrainingS>().lastHitter != "Player1")
        {
            PlayAnimation();
            ShuttleContact();

            // Sets the last hitter to the correct player
            training.GetComponent<TrainingS>().lastHitter = "Player1";
            TrainingS.shuttleNotHitYet = false;
        }
    }
    private void ShuttleContact()
    {
        // Plays the sound
        RacquetSound();

        // Gets the direction
        Vector3 dir = CheckHitDirection();

        // Physics
        float height = dir.y; //get height diff
        dir.y = 0;
        float dist = dir.magnitude;
        float a = currentShot.yForce;
        // Angle for drop shots
        if (currentShot == shotManager.drop && strokeState == 2 && shuttleTrans.position.x >= -2f)
        {
            a = currentShot.xForce;
        }
        // angle for jump overhead shots that aren't smashes
        else if (currentShot == shotManager.drop && strokeState <= 1 && shuttleTrans.position.x >= -2f)
        {
            a -= 5f;
        }
        else if (shuttleTrans.position.y > 3.5f && strokeState == 0 && currentShot == shotManager.drop)
        {
            a -= 10f;
        }
        else if (shuttleTrans.position.y > 3.5f && strokeState == 0 && currentShot == shotManager.clear)
        {
            a -= 15f;
        }
        a *= Mathf.Deg2Rad;
        dir.y = dist * Mathf.Tan(a);
        dist += height / Mathf.Tan(a);

        float velocity = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * a));

        // Checks jump state, and if it's true, adds a lot of x power and reduces y power
        float xJumpPower = 0;
        float yJumpPower = 0;
        float zJumpPower = 0;

        if (currentShot == shotManager.drive && strokeState == 0 && shuttleTrans.position.y > 3.5f)
        {
            xJumpPower = 8f * extraJumpPower;
            yJumpPower = 4.5f * extraJumpPower;
            zJumpPower = CrossCourtSmash();
            if (shuttleTrans.position.x >= -0.65f)
            {
                yJumpPower *= 3f;
                if (zJumpPower != 0f)
                {
                    xJumpPower *= 0.8f;
                }
            }
        }
        else if (currentShot == shotManager.drive && strokeState == 0)
        {
            xJumpPower = 6f * extraJumpPower;
            yJumpPower = 3f * extraJumpPower;
            zJumpPower = CrossCourtSmash();
            if (shuttleTrans.position.x >= -0.65f)
            {
                yJumpPower *= 3f;
                if (zJumpPower != 0f)
                {
                    xJumpPower *= 0.8f;
                }
            }
        }
        // corrects for drops
        else if (shuttleTrans.position.x <= -2f && strokeState == 0 && currentShot == shotManager.drop)
        {
            xJumpPower = 3f;
            yJumpPower = 3f;
            zJumpPower = CrossCourtDrop();
            // jump shot correction
            if (shuttleTrans.position.y > 3.5f)
            {
                xJumpPower += 2f;
            }
        }

        // Sets the force + direction onto the shuttle. Speed is modified by state(hitbox) and
        // whether you're jump SMASHING or not (does not affect other jump shots
        shuttleTrans.GetComponent<Rigidbody>().velocity = (velocity * PowerDueToState()) * dir.normalized + new Vector3(xJumpPower, -yJumpPower, zJumpPower);
    }

    // Corrects for the added forward velocity by adding some to the horizontal
    private float CrossCourtDrop()
    {
        float zPower;
        if (transform.position.z >= 2 && (targetX == 7 || targetX == 8))
        {
            zPower = -1f;
        }
        else if (transform.position.z <= -2 && (targetX == 7 || targetX == 6))
        {
            zPower = 1f;
        }
        else if (transform.position.z < 2 && transform.position.z > -2)
        {
            if (targetX == 8)
            {
                zPower = -0.5f;
            }
            else if (targetX == 6)
            {
                zPower = 0.5f;
            }
            else
            {
                zPower = 0f;
            }
        }
        else
        {
            zPower = 0f;
        }
        if (shuttleTrans.position.y > 3.5f)
        {
            zPower *= 2f;
        }
        return zPower;
    }

    // Corrects for the added forward velocity by adding some to the horizontal
    private float CrossCourtSmash()
    {
        float zPower;
        if (transform.position.z >= 2 && (targetX == 4 || targetX == 5))
        {
            zPower = -2f * extraJumpPower;
        }
        else if (transform.position.z <= -2 && (targetX == 4 || targetX == 3))
        {
            zPower = 2f * extraJumpPower;
        }
        else if (transform.position.z < 2 && transform.position.z > -2)
        {
            if (targetX == 5)
            {
                zPower = -1f * extraJumpPower;
            }
            else if (targetX == 3)
            {
                zPower = 1f * extraJumpPower;
            }
            else
            {
                zPower = 0f;
            }
        }
        else
        {
            zPower = 0f;
        }
        if (shuttleTrans.position.y > 3.5f)
        {
            zPower *= 1.75f;
        }
        return zPower;
    }

    public Vector3 CheckHitDirection()
    {
        targetX = 0;

        if (currentShot == shotManager.clear)
        {
            if (horizontal > 0)
            {
                targetX = 0;
            }
            else if (horizontal < 0)
            {
                targetX = 2;
            }
            else
            {
                targetX = 1;
            }
        }
        // Drives
        else if (currentShot == shotManager.drive)
        {
            if (horizontal > 0)
            {
                targetX = 3;
            }
            else if (horizontal < 0)
            {
                targetX = 5;
            }
            else
            {
                targetX = 4;
            }
        }
        // Drops
        else if (currentShot == shotManager.drop)
        {
            if (horizontal > 0)
            {
                targetX = 6;
            }
            else if (horizontal < 0)
            {
                targetX = 8;
            }
            else
            {
                targetX = 7;
            }
        }
        // Services
        else if (currentShot == shotManager.shortServe)
        {
            if (transform.position.z < 0)
            {
                targetX = 9;
            }
            else
            {
                targetX = 11;
            }
        }
        else if (currentShot == shotManager.longServe)
        {
            if (transform.position.z < 0)
            {
                targetX = 10;
            }
            else
            {
                targetX = 12;
            }
        }

        // Calculates the direction between the player and the target and modifies it by relative position of
        // shuttle to the player. Returns vector of x and z + (x and z again * inaccuracy)
        Vector3 dir = targets[targetX].position - shuttleTrans.position;

        // If the player hits a shot past the service line it's more accurate
        float accMod = 1.5f;
        if (shuttleTrans.position.x >= -2f)
        {
            accMod /= 2;
        }
        if (currentShot == shotManager.clear)
        {
            accMod /= 2;
        }
        float newXInacc = inaccuracy * Random.Range(0.95f, 1.05f);
        float newZInacc = inaccuracy * Random.Range(0.9f, 1.1f);
        dir += new Vector3((shuttleTrans.position.x - transform.position.x) * newXInacc * accMod,
            0,
            (shuttleTrans.position.z - transform.position.z) * newZInacc * 2 * accMod);

        // Public xz coords for bot to move to
        return dir;
    }

    // Changes power based on position of shuttle in hitbox
    private float PowerDueToState()
    {
        float power = 1;

        if (strokeState == 1)
        {
            power -= 0.025f;
        }
        else if (strokeState == 2)
        {
            power -= 0.05f;
        }

        if (shuttleTrans.position.x < transform.position.x)
        {
            power *= 0.97f;
        }

        return power;
    }
    
    private void PlayAnimation()
    {
        if (strokeState == 1)
        {
            playerAnim.Play("Drive Swing");
        }
        else if (strokeState == 2)
        {
            playerAnim.Play("Life Swing");
        }
        else
        {
            playerAnim.Play("Overhead Swing");
        }
    }

    private void RacquetSound()
    {
        racquetSound.volume = 0.75f;
        racquetSound.pitch = 1f;
        if (strokeState == 0 && shuttleTrans.position.y > 3.5f && currentShot == shotManager.drive)
        {
            racquetSound.clip = smashSound;
            racquetSound.volume = 1f;
            racquetSound.pitch = 0.9f;
        }
        else if (strokeState == 0 && shuttleTrans.position.y > 3.5f && currentShot == shotManager.drop)
        {
            racquetSound.clip = smashSound;
            racquetSound.volume = 0.9f;
            racquetSound.pitch = 1.2f;
        }
        else if (strokeState <= 1 && currentShot == shotManager.drop || currentShot == shotManager.shortServe)
        {
            racquetSound.clip = liftSound;
            racquetSound.volume = 0.9f;
            racquetSound.pitch = 1.25f;
        }
        else if (strokeState <= 1 || (strokeState == 2 && currentShot == shotManager.clear) || currentShot == shotManager.longServe)
        {
            racquetSound.clip = smashSound;
        }
        else
        {
            racquetSound.clip = liftSound;
        }
        racquetSound.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("MovePoint"))
        {
            clickSound.Play();
            other.gameObject.SetActive(false);
            points += 10;
            SetPointText();
            TrainingS.pointTaken = false;
        }
    }

    void SetPointText()
    {
        pointsText.text = "Points: " + points;
    }

    float LagDueToState()
    {
        if (strokeState == 1)
        {
            return 1.325f;
        }
        else if (strokeState == 2)
        {
            return 1.75f;
        }
        else
        {
            return 1f;
        }
    }

    // COROUTINES------------------------
    IEnumerator FrameWait()
    {
        allowMovement = false;
        yield return new WaitForSeconds(.45f * LagDueToState());
        allowMovement = true;
    }
    IEnumerator Receivewait()
    {
        allowMovement = false;
        yield return new WaitForSeconds(1f);
        allowMovement = true;
    }
}
