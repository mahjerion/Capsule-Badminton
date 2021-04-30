using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Photon.Pun;

public class NetworkPlayer : MonoBehaviourPunCallbacks
{
    CharacterController playerOne;

    public GameObject shuttleObject;

    // Public variables to be accessed in inspector - don't change!
    public float jumpSpeed;
    public float gravity;

    // Sets base shot to be in middle
    int targetX = 0;

    // Stat variables
    public float speed;
    // Value that causes the shuttle to go off course based on relative position to shuttle. (0.0 - 1.0)
    public float inaccuracy;
    // Value that adds more horizontal power and increases angle on jump shots (1.0 - 2.0)
    public float extraJumpPower;
    // Value that reduces animation delay
    public float swingSpeed;

    // PLAYER STATS--------------
    // These following variables are the stat-changing variables for single player
    public static int playerLevel = 1;
    public static float playerEXP = 0;
    public static int playerPoints = 0;
    public static int speedBoost = 0;
    public static int accuracyBoost = 0;
    public static int smashPowerBoost = 0;
    public static float swingSpeedBoost = 0;
    public static float smashRacquetBonus = 0;

    // Character selection
    public static int playerType;
    public static int racquetType;

    // Allows us to reference and modify the transform of the shuttle
    public Transform shuttle;
    public Transform points;

    // Makes an array accessible by inspector for targets
    public Transform[] targets;

    // Sets initial movement to 0
    private Vector3 moveDirection = Vector3.zero;

    // Sets animator variable
    private Animator playerAnim;

    // Variables to store if it's possible to hit shuttle (if it's in hitbox) and which hitbox it's coming from
    public static int strokeState;
    public static int strokePossible;
    // Checks where player hit it to so bot can follow up
    public static float xTargetHitTo;
    public static float zTargetHitTo;

    // Audio source
    public AudioSource racquetSound;
    public AudioClip liftSound;
    public AudioClip smashSound;

    // Allow movement bool where if it's true they can do stuff. Is set to false whenever they
    // hit a key
    private bool allowMovement;

    // Assigns ShotManager so it can be edited
    ShotManager shotManager;
    Shot currentShot;

    private Player player;

    // camera variables
    float horizontal;
    float vertical;

    // test for fixed update
    bool shortServe;
    bool longServe;
    bool drop;
    bool drive;
    bool clear;

    // for network instantiating
    private void Awake()
    {
        Debug.Log(NetworkController.networkMode);
        if (NetworkController.networkMode)
        {
            // prefab init
            shuttleObject = GameObject.FindWithTag("Shuttle");
            shuttle = GameObject.Find("Shuttle").transform;
            points = GameObject.Find("Points").transform;
            targets[0] = GameObject.Find("Back Forehandp").transform;
            targets[1] = GameObject.Find("Back Middlep").transform;
            targets[2] = GameObject.Find("Back Backhandp").transform;
            targets[3] = GameObject.Find("Middle Forehandp").transform;
            targets[4] = GameObject.Find("Middle Middlep").transform;
            targets[5] = GameObject.Find("Middle Backhandp").transform;
            targets[6] = GameObject.Find("Front Forehandp").transform;
            targets[7] = GameObject.Find("Front Middlep").transform;
            targets[8] = GameObject.Find("Front Backhandp").transform;
            targets[9] = GameObject.Find("Short Serve From Rightp").transform;
            targets[10] = GameObject.Find("Long Serve From Rightp").transform;
            targets[11] = GameObject.Find("Short Serve From Leftp").transform;
            targets[12] = GameObject.Find("Long Serve From Leftp").transform;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        player = ReInput.players.GetPlayer(0);

        // Initialize and get component for players
        strokePossible = 0;
        strokeState = 0;
        playerOne = GetComponent<CharacterController>();
        shotManager = GetComponent<ShotManager>();
        currentShot = shotManager.clear;

        // Animator Component Retrieval
        playerAnim = GetComponent<Animator>();

        // Character select initialization

        CharacterSelect();

        SinglePlayerInit();

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

        if (allowMovement == true)
        {
            // If the game hasn't started yet and p1 serves then:
            if (Points.gameStart == false && Points.server == 1)
            {
                playerAnim.Play("Serve Ready");
                if (player.GetButtonDown("Drop"))
                {
                    shortServe = true;
                }
                if (player.GetButtonDown("Clear"))
                {
                    longServe = true;
                }
            }
            // Only goes through this code if game mode is true
            else if (Points.gameStart == true)
            {
                if (player.GetButtonDown("Clear"))
                {
                    clear = true;
                }
                if (player.GetButtonDown("Drive"))
                {
                    drive = true;
                }
                if (player.GetButtonDown("Drop"))
                {
                    drop = true;
                }

                // Checks to see if player is grounded, then checks for input for movement
                if (playerOne.isGrounded)
                {
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
        if ((allowMovement == false && playerOne.isGrounded) || Points.gameStart == false)
        {
            // Force of gravity applied per frame update - outside because gravity always affects
            moveDirection.x = 0.0f;
            moveDirection.z = 0.0f;
        }
        moveDirection.y -= gravity * Time.deltaTime;
        // Moves the player
        playerOne.Move(moveDirection * Time.deltaTime);
    }
    private void FixedUpdate()
    {
        if (shortServe)
        {
            shortServe = false;
            currentShot = shotManager.shortServe;
            ServiceContact();
        }
        if (longServe)
        {
            longServe = false;
            currentShot = shotManager.longServe;
            ServiceContact();
        }
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

    private void PlayerContact()
    {
        PlayAnimation();
        StartCoroutine("FrameWait");
        if (strokePossible == 1 && points.GetComponent<Points>().lastHitter != "Player1" && shuttle.position.x < 0f)
        {
            ShuttleContact();

            // Sets the last hitter to the correct player
            points.GetComponent<Points>().lastHitter = "Player1";
        }
    }
    private void ServiceContact()
    {
        // Recreates shuttle
        shuttleObject.SetActive(true);
        shuttle.position = transform.position + new Vector3(0.5f, 0.5f, 0);

        playerAnim.Play("Serve");
        StartCoroutine("FrameWait");
        ShuttleContact();

        // Sets the last hitter to the correct player
        points.GetComponent<Points>().lastHitter = "Player1";

        // Sets game mode to true
        Points.gameStart = true;
        playerOne.Move(new Vector3(0, 0, 0));
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
        Vector3 dir = targets[targetX].position - shuttle.position;

        // If the player hits a shot past the service line it's more accurate
        float accMod = 1.5f;
        if (shuttle.position.x >= -2f)
        {
            accMod /= 2;
        }
        if (currentShot == shotManager.clear)
        {
            accMod /= 2;
        }
        dir += new Vector3((shuttle.position.x - transform.position.x) * inaccuracy * accMod,
            0,
            (shuttle.position.z - transform.position.z) * inaccuracy * 2 * accMod);

        // Public xz coords for bot to move to
        xTargetHitTo = targets[targetX].position.x + ((shuttle.position.x - transform.position.x) * inaccuracy * accMod);
        zTargetHitTo = targets[targetX].position.z + ((shuttle.position.z - transform.position.z) * inaccuracy * 2 * accMod);
        return dir;
    }

    // ------------------------------------------------------
    // ------------------------------------------------------
    // ------------------------------------------------------

    // Function for actual physics calculations
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
        if (currentShot == shotManager.drop && strokeState == 2 && shuttle.position.x >= -2f)
        {
            a = currentShot.xForce;
        }
        // angle for jump overhead shots that aren't smashes
        else if (currentShot == shotManager.drop && strokeState <= 1 && shuttle.position.x >= -2f)
        {
            a -= 5f;
        }
        else if (shuttle.position.y > 3.5f && strokeState == 0 && currentShot != shotManager.drive)
        {
            a -= 15f;
        }
        a *= Mathf.Deg2Rad;
        dir.y = dist * Mathf.Tan(a);
        dist += height / Mathf.Tan(a);

        float velocity = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * a));

        // Checks jump state, and if it's true, adds a lot of x power and reduces y power
        float xJumpPower = 0f;
        float yJumpPower = 0f;
        float zJumpPower = 0f;

        if (currentShot == shotManager.drive && strokeState == 0 && shuttle.position.y > 3.5f)
        {
            xJumpPower = 8f * extraJumpPower;
            yJumpPower = 5f * extraJumpPower;
            zJumpPower = CrossCourtSmash();
        }
        else if (currentShot == shotManager.drive && strokeState == 0)
        {
            xJumpPower = 5f * extraJumpPower;
            yJumpPower = 3f * extraJumpPower;
            zJumpPower = CrossCourtSmash();
        }
        // corrects for jump drops
        else if (shuttle.position.y > 3.5f && strokeState == 0 && currentShot == shotManager.drop)
        {
            xJumpPower = 3f;
        }

        // Sets the force + direction onto the shuttle. Speed is modified by state(hitbox) and
        // whether you're jump SMASHING or not (does not affect other jump shots
        shuttle.GetComponent<Rigidbody>().velocity = (velocity * PowerDueToState()) * dir.normalized + new Vector3(xJumpPower, -yJumpPower, zJumpPower);
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
        if (shuttle.position.y > 3.5f)
        {
            zPower *= 2;
        }
        return zPower;
    }

    // Changes power based on position of shuttle in hitbox
    private float PowerDueToState()
    {
        if (strokeState == 1)
        {
            return 0.966f;
        }
        else if (strokeState == 2)
        {
            return 0.933f;
        }
        else
        {
            return 1.0f;
        }
    }

    // ------------------------------------------------------
    // ------------------------------------------------------
    // ------------------------------------------------------*/
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

    // Checks for character selection and makes appropriate changes to stats
    private void CharacterSelect()
    {
        // playerType
        if (BotMovement.amIABot == true)
        {
            speed = 2.5f;
            inaccuracy = 0.5f;
            extraJumpPower = 0.5f;
        }
        else if (playerType == 1)
        {
            speed = 3f;
            inaccuracy = 0.4f;
            extraJumpPower = 1.2f;
        }
        else if (playerType == 2)
        {
            speed = 3.3f;
            inaccuracy = 0.4f;
            extraJumpPower = 0.9f;
        }
        else if (playerType == 3)
        {
            speed = 3f;
            inaccuracy = 0.1f;
            extraJumpPower = 0.9f;
        }
        else
        {
            speed = 3.1f;
            inaccuracy = 0.3f;
            extraJumpPower = 1f;
        }

        // racquetType
        if (racquetType == 1)
        {
            swingSpeedBoost = 0.1f;
            smashRacquetBonus = 0f;
        }
        else if (racquetType == 2)
        {
            swingSpeedBoost = 0f;
            smashRacquetBonus = 0.1f;
        }
        else
        {
            swingSpeedBoost = 0.05f;
            smashRacquetBonus = 0.05f;
        }
    }

    void SinglePlayerInit()
    {
        speed *= 1 + ((speedBoost * 0.01f) / 2);
        inaccuracy -= (accuracyBoost * 0.01f);
        extraJumpPower = extraJumpPower + (smashPowerBoost * 0.01f) + smashRacquetBonus;
        swingSpeed += swingSpeedBoost;
    }

    // Audio function, plays sound depending on which shot was hit from what position
    private void RacquetSound()
    {
        racquetSound.volume = 0.75f;
        racquetSound.pitch = 1f;
        if (strokeState == 0 && shuttle.position.y > 3.5f && currentShot == shotManager.drive)
        {
            racquetSound.clip = smashSound;
            racquetSound.volume = 1f;
            racquetSound.pitch = 0.9f;
        }
        else if (strokeState == 0 && shuttle.position.y > 3.5f && currentShot == shotManager.drop)
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
        yield return new WaitForSeconds((0.5f - swingSpeed) * LagDueToState());
        allowMovement = true;
    }
}
