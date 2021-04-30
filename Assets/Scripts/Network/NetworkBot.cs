using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Photon.Pun;

public class NetworkBot : MonoBehaviour
{
    CharacterController playerTwo;

    public GameObject shuttleObject;

    // Public variables to be accessed in inspector
    public float speed;
    public float jumpSpeed;
    public float gravity;
    public float inaccuracy;
    public float extraJumpPower;

    public static int playerType;

    // Sets base shot to be in middle
    int targetX = 0;

    // Racquet variables
    public static int racquetType;
    public static float smashRacquetBonus = 0;

    // Allows us to reference and modify the transform of the shuttle
    public Transform shuttle;
    public Transform points;
    public Transform basePosition;

    // Makes an array accessible by inspector for targets
    public Transform[] targets;

    // Sets initial movement to 0
    private Vector3 moveDirection = Vector3.zero;

    // Sets animator variable
    private Animator playerAnim;

    public static int strokeState;
    public static int strokePossible;

    // Variables for bot initialization, difficulty 0-2, 0 = easy
    public static bool amIABot;
    public static int difficultyLevel;
    private float botDelaySpeed;
    private float botSwingSpeed;

    // Assigns ShotManager so it can be edited
    ShotManager shotManager;
    Shot currentShot;

    // Audio source
    public AudioSource racquetSound;
    public AudioClip liftSound;
    public AudioClip smashSound;

    // Bot Bools
    private bool allowMovement;
    private bool botCanServe;
    private bool coroutineStarted;
    private bool delayStarted;
    private bool botJumpChanceRolled;

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
    bool botServe;
    bool botShot;

    private void Awake()
    {
        if (NetworkController.networkMode)
        {
            // prefab init
            shuttleObject = GameObject.FindWithTag("Shuttle");
            shuttle = GameObject.Find("Shuttle").transform;
            points = GameObject.Find("Points").transform;
            basePosition = GameObject.Find("Base Position").transform;
            targets[0] = GameObject.Find("Back Forehandb").transform;
            targets[1] = GameObject.Find("Back Middleb").transform;
            targets[2] = GameObject.Find("Back Backhandb").transform;
            targets[3] = GameObject.Find("Middle Forehandb").transform;
            targets[4] = GameObject.Find("Middle Middleb").transform;
            targets[5] = GameObject.Find("Middle Backhandb").transform;
            targets[6] = GameObject.Find("Front Forehandb").transform;
            targets[7] = GameObject.Find("Front Middleb").transform;
            targets[8] = GameObject.Find("Front Backhandb").transform;
            targets[9] = GameObject.Find("Short Serve From Rightb").transform;
            targets[10] = GameObject.Find("Long Serve From Rightb").transform;
            targets[11] = GameObject.Find("Short Serve From Leftb").transform;
            targets[12] = GameObject.Find("Long Serve From Leftb").transform;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkController.networkMode)
        {
            player = ReInput.players.GetPlayer(0);
        }
        else
        {
            // rewired init
            player = ReInput.players.GetPlayer(1);
        }

        // Initialize and get component for players
        strokePossible = 0;
        strokeState = 0;
        playerTwo = GetComponent<CharacterController>();
        shotManager = GetComponent<ShotManager>();
        currentShot = shotManager.clear;

        // Animator Component Retrieval
        playerAnim = GetComponent<Animator>();

        // Character select initialization

        CharacterSelect();

        // Movement initialize
        allowMovement = true;
        botCanServe = false;
        coroutineStarted = false;
        delayStarted = false;
        botJumpChanceRolled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (SinglePlayerCamera.altCamera)
        {
            horizontal = -player.GetAxis("Move Vertical");
            vertical = player.GetAxis("Move Horizontal");
        }
        else
        {
            horizontal = player.GetAxis("Move Horizontal");
            vertical = player.GetAxis("Move Vertical");
        }

        if (allowMovement == true)
        {
            // If the game hasn't started yet and p1 serves then:
            if (Points.gameStart == false && Points.server == 2)
            {
                if (amIABot == false)
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
                else
                {
                    playerAnim.Play("Serve Ready");
                    if (botCanServe == false && coroutineStarted == false)
                    {
                        StartCoroutine("BotServeWait");
                    }
                    else if (botCanServe == true && coroutineStarted == false)
                    {
                        botServe = true;
                    }
                }
            }
            // Only goes through this code if game mode is true
            else if (Points.gameStart == true)
            {
                if (amIABot == false)
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
                }
                else
                {
                    if (delayStarted == true)
                    {
                        botShot = true;
                    }
                }

                // Checks to see if player is grounded, then checks for input for movement
                if (playerTwo.isGrounded)
                {
                    // Checks to see if bot. If it is, moves to shuttle if last hitter was player and shuttle x >= -1
                    // otherwise move to base position. If it's human get input keys
                    if (amIABot == false)
                    {
                        moveDirection = new Vector3(-vertical, 0.0f, -horizontal);
                        moveDirection *= speed;

                        // Jump key = jump
                        if (player.GetButton("Jump"))
                        {
                            moveDirection.x /= 2;
                            moveDirection.z /= 2;
                            moveDirection.y = jumpSpeed;
                        }
                    }
                    else
                    {
                        if (points.GetComponent<Points>().lastHitter == "Player1")
                        {
                            // Checks to see if shuttle is nearby, if it is move towards it, otherwise move towards
                            // general landing location
                            if (Mathf.Abs(shuttle.position.x - transform.position.x) <= 1.5f
                                && Mathf.Abs(shuttle.position.z - transform.position.z) <= 1f)
                            {
                                moveDirection = new Vector3(shuttle.position.x - transform.position.x, 0, shuttle.position.z - transform.position.z);
                            }
                            else
                            {
                                // adds a delay before bot reacts, if the counter goes through then they can "hit"
                                StartCoroutine("DelayBotReaction");
                                if (delayStarted == true)
                                {
                                    if (PlayerMovement.xTargetHitTo <= 3)
                                    {
                                        moveDirection = new Vector3(PlayerMovement.xTargetHitTo * 0.95f - transform.position.x,
                                            0, PlayerMovement.zTargetHitTo * 1.05f - transform.position.z);
                                    }
                                    else
                                    {
                                        moveDirection = new Vector3(PlayerMovement.xTargetHitTo * 0.75f - transform.position.x,
                                            0, PlayerMovement.zTargetHitTo * 0.9f - transform.position.z);
                                    }
                                }
                                else
                                {
                                    moveDirection = Vector3.zero;
                                }
                            }
                        }
                        // Otherwise move back to middle of court
                        else
                        {
                            delayStarted = false;
                            moveDirection = new Vector3(basePosition.position.x - transform.position.x, 0, basePosition.position.z - transform.position.z);
                        }

                        if (Vector3.Magnitude(moveDirection) > 0.5f)
                        {
                            moveDirection.Normalize();
                        }

                        moveDirection *= speed;

                        // checks to see if bot can jump and if he hasnt then he can for this shot instance
                        if (points.GetComponent<Points>().lastHitter == "Player1" && difficultyLevel >= 1)
                        {
                            // Checks to see if shuttle is on top of bot, if it is then jump
                            if (Mathf.Abs(shuttle.position.x - transform.position.x) <= (1f + difficultyLevel / 2)
                && Mathf.Abs(shuttle.position.y - transform.position.y) >= 3.5f
                && Mathf.Abs(shuttle.position.z - transform.position.z) <= (1f + difficultyLevel / 2)
                && Mathf.Abs(shuttle.position.y - transform.position.y) <= 5.5f
                && botJumpChanceRolled == false)
                            {
                                botJumpChanceRolled = true;
                                int randomJumpChance = Random.Range(0, 3);

                                if (randomJumpChance >= 1)
                                {
                                    moveDirection.x /= 2;
                                    moveDirection.z /= 2;
                                    moveDirection.y = jumpSpeed;
                                }
                            }
                        }
                    }
                }
            }
        }
        if ((allowMovement == false && playerTwo.isGrounded) || Points.gameStart == false)
        {
            // Force of gravity applied per frame update - outside because gravity always affects
            moveDirection.x = 0.0f;
            moveDirection.z = 0.0f;
        }
        moveDirection.y -= gravity * Time.deltaTime;
        // Moves the player
        playerTwo.Move(moveDirection * Time.deltaTime);
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
        if (botServe)
        {
            botServe = false;
            ServiceContact();
        }
        if (botShot)
        {
            botShot = false;
            BotShotSelection();
        }
    }

    private void PlayerContact()
    {
        PlayAnimation();
        StartCoroutine("FrameWait");
        if (strokePossible == 1 && points.GetComponent<Points>().lastHitter != "Player2" && shuttle.position.x > 0f)
        {
            ShuttleContact();

            // Sets the last hitter to the correct player
            points.GetComponent<Points>().lastHitter = "Player2";
        }
    }
    private void ServiceContact()
    {
        if (amIABot == true)
        {
            int whatServe = Random.Range(0, 2);
            if (whatServe == 1)
            {
                currentShot = shotManager.longServe;
            }
            else
            {
                currentShot = shotManager.shortServe;
            }
            botCanServe = false;
        }

        // Recreates shuttle
        shuttleObject.SetActive(true);
        shuttle.position = transform.position + new Vector3(-0.5f, 0.5f, 0);

        playerAnim.Play("Serve");
        StartCoroutine("FrameWait");
        ShuttleContact();

        // Sets the last hitter to the correct player
        points.GetComponent<Points>().lastHitter = "Player2";

        // Sets game mode to true
        Points.gameStart = true;

        playerTwo.Move(new Vector3(0, 0, 0));
    }

    private Vector3 CheckHitDirection()
    {
        targetX = 0;

        // Checks to see the type of shot being hit, then the direction the player is hitting in
        // Clears
        if (currentShot == shotManager.clear)
        {
            if (amIABot == false)
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
            else
            {
                targetX = Random.Range(0, 3);
            }
        }
        // Drives
        else if (currentShot == shotManager.drive)
        {
            if (amIABot == false)
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
            else
            {
                targetX = Random.Range(3, 6);
            }
        }
        // Drops
        else if (currentShot == shotManager.drop)
        {
            if (amIABot == false)
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
            else
            {
                targetX = Random.Range(6, 9);
            }
        }
        // Services
        else if (currentShot == shotManager.shortServe)
        {
            if (transform.position.z > 0)
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
            if (transform.position.z > 0)
            {
                targetX = 10;
            }
            else
            {
                targetX = 12;
            }
        }

        // Calculates the direction between the player and the target and modifies it by relative position of
        // shuttle to the player
        Vector3 dir = targets[targetX].position - shuttle.position;

        // If the player hits a shot past the service line it's more accurate
        float accMod = 1.5f;
        if (shuttle.position.x <= 2f)
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
        return dir;
    }

    // Function for actual physics calculations
    private void ShuttleContact()
    {
        RacquetSound();

        // Gets the direction
        Vector3 dir = CheckHitDirection();

        // Physics
        float height = dir.y; //get height diff
        dir.y = 0;
        float dist = dir.magnitude;
        float a = currentShot.yForce;
        // Angle for drop shots
        if (currentShot == shotManager.drop && strokeState == 2 && shuttle.position.x <= 2f)
        {
            a = currentShot.xForce;
        }
        // angle for jump overhead shots that aren't smashes
        else if (currentShot == shotManager.drop && strokeState <= 1 && shuttle.position.x <= 2f)
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

        // Sets the force + direction onto the shuttle
        shuttle.GetComponent<Rigidbody>().velocity = (velocity * PowerDueToState()) * dir.normalized + new Vector3(-xJumpPower, -yJumpPower, zJumpPower);

        // Let's the bot jump again (one roll per "shot instance")
        botJumpChanceRolled = false;
    }

    // Corrects for the added forward velocity by adding some to the horizontal
    private float CrossCourtSmash()
    {
        float zPower;
        if (transform.position.z >= 2 && (targetX == 4 || targetX == 3))
        {
            zPower = -2f * extraJumpPower;
        }
        else if (transform.position.z <= -2 && (targetX == 4 || targetX == 5))
        {
            zPower = 2f * extraJumpPower;
        }
        else if (transform.position.z < 2 && transform.position.z > -2)
        {
            if (targetX == 5)
            {
                zPower = 1f * extraJumpPower;
            }
            else if (targetX == 3)
            {
                zPower = -1f * extraJumpPower;
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

    private void CharacterSelect()
    {
        if (amIABot == false)
        {
            if (playerType == 1)
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
                botSwingSpeed = 0.1f;
                smashRacquetBonus = 0f;
            }
            else if (racquetType == 2)
            {
                botSwingSpeed = 0f;
                smashRacquetBonus = 0.1f;
            }
            else
            {
                botSwingSpeed = 0.05f;
                smashRacquetBonus = 0.05f;
            }
            // Same as singleplayerinit()
            extraJumpPower += smashRacquetBonus;
        }
        else
        {
            BotDifficulty(difficultyLevel);
        }
    }

    // Adjusts difficulty of bot
    void BotDifficulty(int levelOfBot)
    {
        speed = Mathf.Min(2.5f + 0.25f * levelOfBot);
        inaccuracy = Mathf.Max(0.5f - 0.1f * levelOfBot, 0.05f);
        extraJumpPower = 0.5f + 0.25f * levelOfBot;
        botDelaySpeed = 0f + 0.07f * levelOfBot;
        botSwingSpeed = 0f + 0.05f * levelOfBot;
    }

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

    private void BotShotSelection()
    {
        if (Mathf.Abs(shuttle.position.z) >= 3.5f || shuttle.position.x >= 8.95f)
        {

        }
        else
        {
            if (strokePossible == 1)
            {
                if (shuttle.position.y > 3.5f)
                {
                    int botJumpType = Random.Range(0, 5);
                    if (botJumpType == 0)
                    {
                        currentShot = shotManager.drop;
                    }
                    else if (botJumpType == 1)
                    {
                        currentShot = shotManager.clear;
                    }
                    else
                    {
                        currentShot = shotManager.drive;
                    }
                }
                else
                {
                    int botStrokeType;

                    // If the bot is at the front of the court and his racquet is up he won't lift he'll kill it or drop it back
                    if (shuttle.position.x <= 2f && strokeState == 0)
                    {
                        botStrokeType = Random.Range(0, 2);
                        if (botStrokeType == 0)
                        {
                            currentShot = shotManager.drop;
                        }
                        else if (botStrokeType == 1)
                        {
                            currentShot = shotManager.drive;
                        }
                    }
                    else
                    {
                        botStrokeType = Random.Range(0, 3);
                        if (botStrokeType == 2)
                        {
                            currentShot = shotManager.drop;
                        }
                        else if (botStrokeType == 1 && strokeState != 2)
                        {
                            currentShot = shotManager.drive;
                        }
                        else
                        {
                            currentShot = shotManager.clear;
                        }
                    }
                }
                PlayerContact();
            }
        }
    }

    float LagDueToState()
    {
        if (strokeState == 1)
        {
            return 1.25f;
        }
        else if (strokeState == 2)
        {
            return 1.5f;
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
        yield return new WaitForSeconds((0.5f - botSwingSpeed) * LagDueToState());
        allowMovement = true;
    }
    IEnumerator BotServeWait()
    {
        coroutineStarted = true;
        yield return new WaitForSeconds(2f);
        botCanServe = true;
        coroutineStarted = false;
    }
    IEnumerator DelayBotReaction()
    {
        if (delayStarted == false)
        {
            yield return new WaitForSeconds(0.5f - botDelaySpeed);
            delayStarted = true;
        }
        else
        {
            yield return null;
        }
    }
}
