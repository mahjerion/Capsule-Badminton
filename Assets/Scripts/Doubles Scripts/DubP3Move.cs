using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class DubP3Move : MonoBehaviour
{
    CharacterController player;

    public GameObject shuttleObject;

    // Public variables to be accessed in inspector - don't change!
    public float jumpSpeed = 4f;
    public float gravity = 9.8f;

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

    // new variables modified by energy levels
    float newSpd;
    float newInacc;
    float newJump;

    // energy variables
    public static float energyMax = 100;
    public static float energyCurr;
    float energyRegen = 1;
    public Image energyBar;

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

    private Player playerD;

    // camera variables
    float horizontal;
    float vertical;

    // bot variables
    private bool botCanServe;
    private bool coroutineStarted;
    private bool delayStarted;
    private bool botJumpChanceRolled;
    // bot delay
    private float botDelaySpeed;
    // bot positions 0 = front, 1 = back, 2 = right, 3 = left
    public Transform[] basePosition;
    public Transform doublesPosition;
    public Transform p1Position;
    // Bot variable - used to store the last type of hit by bots so they can maintain relatively decent
    // positioning/rotation, 0 = defensive, 1 = offensive
    public static int redBotLastShotType;

    public static bool amIABot;

    // test for fixed update
    bool shortServe;
    bool longServe;
    bool drop;
    bool drive;
    bool clear;
    bool botServe;
    bool botShot;
    bool detectHit;

    // Start is called before the first frame update
    void Start()
    {
        if (BotMovement.amIABot)
        {
            playerD = ReInput.players.GetPlayer(1);
        }
        else
        {
            playerD = ReInput.players.GetPlayer(2);
        }

        // Initialize and get component for players
        strokePossible = 0;
        strokeState = 0;
        player = GetComponent<CharacterController>();
        shotManager = GetComponent<ShotManager>();
        currentShot = shotManager.clear;

        // Animator Component Retrieval
        playerAnim = GetComponent<Animator>();

        // Character select initialization
        // stats
        CharacterSelect();

        // needs to be after singleplayer and characterselect
        energyCurr = energyMax;

        // Movement initialize
        allowMovement = true;
        // bots
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
            horizontal = playerD.GetAxis("Move Vertical");
            vertical = -playerD.GetAxis("Move Horizontal");
        }
        else
        {
            horizontal = playerD.GetAxis("Move Horizontal");
            vertical = playerD.GetAxis("Move Vertical");
        }

        if (allowMovement == true)
        {
            // If the game hasn't started yet and p3 serves then:
            if (Points.gameStart == false && Points.server == 3)
            {
                if (amIABot == false)
                {
                    playerAnim.Play("Serve Ready");
                    if (playerD.GetButtonDown("Drop"))
                    {
                        shortServe = true;
                        StartCoroutine("InputBuffer");
                    }
                    if (playerD.GetButtonDown("Clear"))
                    {
                        longServe = true;
                        StartCoroutine("InputBuffer");
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
                    if (playerD.GetButtonDown("Clear"))
                    {
                        clear = true;
                        StartCoroutine("InputBuffer");
                    }
                    if (playerD.GetButtonDown("Drive"))
                    {
                        drive = true;
                        StartCoroutine("InputBuffer");
                    }
                    if (playerD.GetButtonDown("Drop"))
                    {
                        drop = true;
                        StartCoroutine("InputBuffer");
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
                if (player.isGrounded)
                {
                    if (amIABot == false)
                    {
                        moveDirection = new Vector3(vertical, 0.0f, horizontal);
                        moveDirection *= newSpd;

                        // Jump key = jump
                        if (playerD.GetButton("Jump"))
                        {
                            EnergyOnJump();
                            moveDirection.x /= 2;
                            moveDirection.z /= 2;
                            moveDirection.y = jumpSpeed;
                        }
                    }
                    else
                    {
                        MoveBot();
                    }
                }
            }
        }
        if ((allowMovement == false && player.isGrounded) || Points.gameStart == false)
        {
            // Force of gravity applied per frame update - outside because gravity always affects
            moveDirection.x = 0.0f;
            moveDirection.z = 0.0f;
        }
        moveDirection.y -= gravity * Time.deltaTime;
        // Moves the player
        player.Move(moveDirection * Time.deltaTime);
        if (Points.gameStart)
        {
            EnergyRegen();
        }
        EnergyLevels();
    }

    private void FixedUpdate()
    {
        if (detectHit)
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
            if (strokePossible == 1 && shuttle.position.x < 0f)
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
            if (botServe)
            {
                botServe = false;
                ServiceContact();
        }
        if (strokePossible == 1 && shuttle.position.x < 0f && points.GetComponent<Points>().lastHitter != "Player1")
        {
            if (botShot)
            {
                EnergyFirstPress();
                botShot = false;
                BotShotSelection();
            }
        }
    }
    IEnumerator InputBuffer()
    {
        detectHit = true;
        EnergyFirstPress();
        playerAnim.Play("Ready Position");
        StartCoroutine("FrameWait");
        yield return new WaitForSeconds(0.2f);
        shortServe = false;
        longServe = false;
        clear = false;
        drive = false;
        drop = false;
        detectHit = false;
    }

    private void PlayerContact()
    {
        if (points.GetComponent<Points>().lastHitter != "Player3" 
            && Points.doublesReceiver != 1)
        {
            PlayAnimation();
            EnergyOnHit();
            ShuttleContact();

            // Sets the last hitter to the correct player
            points.GetComponent<Points>().lastHitter = "Player3";

            if (Points.doublesReceiver == 3)
            {
                Points.doublesReceiver = 0;
            }
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
        shuttle.position = transform.position + new Vector3(0.5f, 0.5f, 0);

        playerAnim.Play("Life Swing");
        StartCoroutine("FrameWait");

        // Sets game mode to true
        StartCoroutine("delayGameStart");
        player.Move(new Vector3(0, 0, 0));
        // Sets the last hitter to the correct player
        points.GetComponent<Points>().lastHitter = "Player3";

        ShuttleContact();
    }

    IEnumerator delayGameStart()
    {
        yield return new WaitForSeconds(0.1f);
        Points.gameStart = true;
    }

    public Vector3 CheckHitDirection()
    {
        targetX = 0;

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
                if (targetX == 4)
                {
                    targetX = Random.Range(0, 2);
                    if (targetX == 0)
                    {
                        targetX = 3;
                    }
                    else
                    {
                        targetX = 5;
                    }
                }
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
        dir += new Vector3((shuttle.position.x - transform.position.x) * newInacc * accMod,
            0,
            (shuttle.position.z - transform.position.z) * newInacc * 2 * accMod);

        // Public xz coords for bot to move to
        DubP1Move.xTargetHitTo = targets[targetX].position.x + ((shuttle.position.x - transform.position.x) * newInacc * accMod);
        DubP1Move.zTargetHitTo = targets[targetX].position.z + ((shuttle.position.z - transform.position.z) * newInacc * 2 * accMod);
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
        else if (shuttle.position.y > 3.5f && strokeState == 0 && currentShot == shotManager.drop)
        {
            a -= 10f;
        }
        else if (shuttle.position.y > 3.5f && strokeState == 0 && currentShot == shotManager.clear)
        {
            a -= 20f;
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
            xJumpPower = 7f * newJump;
            yJumpPower = 4.75f * newJump;
            zJumpPower = CrossCourtSmash();
            if (shuttle.position.x >= -0.65f)
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
            xJumpPower = 5.5f * newJump;
            yJumpPower = 3.25f * newJump;
            zJumpPower = CrossCourtSmash();
            if (shuttle.position.x >= -0.65f)
            {
                yJumpPower *= 3f;
                if (zJumpPower != 0f)
                {
                    xJumpPower *= 0.8f;
                }
            }
        }
        // corrects for drops
        else if (shuttle.position.x <= -2f && strokeState == 0 && currentShot == shotManager.drop)
        {
            xJumpPower = 3f;
            yJumpPower = 3f;
            zJumpPower = CrossCourtDrop();
            // jump shot correction
            if (shuttle.position.y > 3.5f)
            {
                xJumpPower += 2f;
            }
        }

        // Sets the force + direction onto the shuttle. Speed is modified by state(hitbox) and
        // whether you're jump SMASHING or not (does not affect other jump shots
        shuttle.GetComponent<Rigidbody>().velocity = (velocity * PowerDueToState()) * dir.normalized + new Vector3(xJumpPower, -yJumpPower, zJumpPower);

        botJumpChanceRolled = false;
        CheckLastShotType();

        Points.redHits++;
        Points.bluHits = 0;
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
        if (shuttle.position.y > 3.5f)
        {
            zPower *= 2;
        }
        return zPower;
    }

    // Corrects for the added forward velocity by adding some to the horizontal
    private float CrossCourtSmash()
    {
        float zPower;
        if (transform.position.z >= 2 && (targetX == 4 || targetX == 5))
        {
            zPower = -2f * newJump;
        }
        else if (transform.position.z <= -2 && (targetX == 4 || targetX == 3))
        {
            zPower = 2f * newJump;
        }
        else if (transform.position.z < 2 && transform.position.z > -2)
        {
            if (targetX == 5)
            {
                zPower = -1f * newJump;
            }
            else if (targetX == 3)
            {
                zPower = 1f * newJump;
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
            zPower *= 1.75f;
        }
        return zPower;
    }

    // Changes power based on position of shuttle in hitbox
    private float PowerDueToState()
    {
        if (strokeState == 1)
        {
            return 0.975f;
        }
        else if (strokeState == 2)
        {
            return 0.95f;
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
        if (amIABot == false)
        {
            // playerType
            if (playerType == 1)
            {
                speed = 3f;
                inaccuracy = 0.4f;
                extraJumpPower = 1.45f;
                energyRegen = 1.25f;
            }
            else if (playerType == 2)
            {
                speed = 3.3f;
                inaccuracy = 0.4f;
                extraJumpPower = 1.15f;
                energyRegen = 1.25f;
            }
            else if (playerType == 3)
            {
                speed = 3f;
                inaccuracy = 0.1f;
                extraJumpPower = 1.15f;
                energyRegen = 1.25f;
            }
            else
            {
                speed = 3.1f;
                inaccuracy = 0.3f;
                extraJumpPower = 1.25f;
                energyRegen = 1.25f;
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
        else
        {
            BotDifficulty(PlayerMovement.playerLevel);
        }
    }

    // Audio function, plays sound depending on which shot was hit from what position
    private void RacquetSound()
    {
        racquetSound.volume = 0.75f;
        racquetSound.pitch = 1f;
        if (strokeState == 0 && shuttle.position.y > 3.5f && currentShot == shotManager.drive)
        {
            racquetSound.clip = smashSound;
            racquetSound.volume = 1.1f;
            racquetSound.pitch = 0.95f;
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
            return 1.25f;
        }
        else if (strokeState == 2)
        {
            return 1.5f;
        }
        else
        {
            return 0.8f;
        }
    }

    // Bot Functions-------------------

    // Adjusts difficulty of bot based on player level
    void BotDifficulty(int levelOfBot)
    {
        speed = Mathf.Min(2.5f + 0.025f * levelOfBot);
        inaccuracy = Mathf.Max(0.5f - 0.01f * levelOfBot, 0.05f);
        extraJumpPower = 1f + 0.02f * levelOfBot;
        swingSpeed = 0.005f * levelOfBot;
        botDelaySpeed = 0.01f * levelOfBot;
        energyRegen = Mathf.Min(1 + (0.1f * levelOfBot), 1.5f);
    }

    private void BotShotSelection()
    {
        if (Mathf.Abs(shuttle.position.z) >= 4.15f || shuttle.position.x >= 8.95f)
        {

        }
        else
        {
            if (strokePossible == 1 && shuttle.position.x < 0f)
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
                    if (shuttle.position.x <= 0.5f && (strokeState == 0 || strokeState == 1))
                    {
                        currentShot = shotManager.drive;
                    }
                    else if (shuttle.position.x <= 2f && strokeState == 0)
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

    void MoveBot()
    {
        if (((points.GetComponent<Points>().lastHitter == "Player2" ||
            points.GetComponent<Points>().lastHitter == "Player4") && ThisBotIsCloser() && Points.doublesReceiver != 1)
            || Points.doublesReceiver == 3)
        {
            // Checks to see if shuttle is nearby, if it is move towards it, otherwise move towards
            // general landing location
            if (Mathf.Abs(shuttle.position.x - transform.position.x) <= 1f
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
                    if (DubP2Move.xTargetHitTo >= -3)
                    {
                        moveDirection = new Vector3(DubP2Move.xTargetHitTo * 0.85f - transform.position.x,
                            0, DubP2Move.zTargetHitTo * 0.95f - transform.position.z);
                    }
                    else
                    {
                        moveDirection = new Vector3(DubP2Move.xTargetHitTo * 0.8f - transform.position.x,
                            0, DubP2Move.zTargetHitTo * 0.9f - transform.position.z);
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
            moveDirection = new Vector3(doublesPosition.position.x - transform.position.x, 0, doublesPosition.position.z - transform.position.z);
        }

        if (Vector3.Magnitude(moveDirection) > 0.5f)
        {
            moveDirection.Normalize();
        }

        moveDirection *= newSpd;

        // checks to see if bot can jump and if he hasnt then he can for this shot instance
        if (points.GetComponent<Points>().lastHitter == "Player2" ||
            points.GetComponent<Points>().lastHitter == "Player4")
        {
            // Checks to see if shuttle is on top of bot, if it is then jump
            if (Mathf.Abs(shuttle.position.x - transform.position.x) <= (0.5f + PlayerMovement.playerLevel / 10)
                && Mathf.Abs(shuttle.position.y - transform.position.y) >= 3.5f
                && Mathf.Abs(shuttle.position.z - transform.position.z) <= (0.5f + PlayerMovement.playerLevel / 10)
                && Mathf.Abs(shuttle.position.y - transform.position.y) <= 4.5f
                && botJumpChanceRolled == false)
            {
                botJumpChanceRolled = true;
                int randomJumpChance = Random.Range(0, 5);

                if (randomJumpChance >= 1)
                {
                    EnergyOnJump();
                    moveDirection.x /= 2;
                    moveDirection.z /= 2;
                    moveDirection.y = jumpSpeed;
                }
            }
        }
    }

    void CheckLastShotType()
    {
        if (currentShot == shotManager.clear || currentShot == shotManager.longServe)
        {
            redBotLastShotType = 0;
        }
        else
        {
            redBotLastShotType = 1;
        }

        if (redBotLastShotType == 0)
        {
            if (transform.position.z > p1Position.position.z)
            {
                doublesPosition.position = basePosition[3].position;
            }
            else
            {
                doublesPosition.position = basePosition[2].position;
            }
        }
        else if (transform.position.x < p1Position.position.x)
        {
            doublesPosition.position = basePosition[1].position;
        }
        else
        {
            doublesPosition.position = basePosition[0].position;
        }
    }

    bool ThisBotIsCloser()
    {
        Vector3 distanceToShuttle;
        distanceToShuttle.x = DubP2Move.xTargetHitTo;
        distanceToShuttle.y = 0f;
        distanceToShuttle.z = DubP2Move.zTargetHitTo;

        float distanceThis = Vector3.Distance(transform.position, distanceToShuttle);
        float distanceThem = Vector3.Distance(p1Position.transform.position, distanceToShuttle);

        if (distanceThem > distanceThis)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // ENERGY--------------------
    void EnergyFirstPress()
    {
        energyCurr = Mathf.Max(0, energyCurr - 1);
    }

    void EnergyOnHit()
    {
        if (strokeState == 0 && currentShot == shotManager.drive)
        {
            energyCurr = Mathf.Max(0, energyCurr - 12);
        }
        else
        {
            energyCurr = Mathf.Max(0, energyCurr - 6);
        }
    }

    void EnergyOnJump()
    {
        energyCurr = Mathf.Max(0, energyCurr - 7);
    }

    void EnergyRegen()
    {
        if (energyCurr < energyMax)
        {
            energyCurr += (0.05f * energyRegen);
        }
    }

    void EnergyLevels()
    {
        if (energyCurr <= 1)
        {
            newSpd = speed * 0.8f;
            newInacc = inaccuracy + 0.20f;
            newJump = extraJumpPower * 0.8f;
        }
        if (energyCurr <= 25)
        {
            newSpd = speed * 0.85f;
            newInacc = inaccuracy + 0.15f;
            newJump = extraJumpPower * 0.85f;
        }
        else if (energyCurr <= 50)
        {
            newSpd = speed * 0.9f;
            newInacc = inaccuracy + 0.1f;
            newJump = extraJumpPower * 0.9f;
        }
        else if (energyCurr <= 75)
        {
            newSpd = speed * 0.95f;
            newInacc = inaccuracy + 0.05f;
            newJump = extraJumpPower * 0.95f;
        }
        else
        {
            newSpd = speed;
            newInacc = inaccuracy;
            newJump = extraJumpPower;
        }
        energyBar.color = new Color(1, energyCurr * 0.01f, 0);
        energyBar.fillAmount = energyCurr / energyMax;
    }

    // COROUTINES------------------------

    IEnumerator FrameWait()
    {
        allowMovement = false;
        yield return new WaitForSeconds((0.5f - swingSpeed) * LagDueToState());
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
            yield return new WaitForSeconds(0.4f - Random.Range(0f, botDelaySpeed));
            delayStarted = true;
        }
        else
        {
            yield return null;
        }
    }
}