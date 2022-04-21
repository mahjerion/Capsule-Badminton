using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;

public class DubP1Move : MonoBehaviour
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
    int playerLevel = 1;
    int speedBoost = 0;
    int accuracyBoost = 0;
    int smashPowerBoost = 0;
    float swingSpeedBoost = 0;
    float smashRacquetBonus = 0;
    public static int energyBoost = 0;

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

    // Rewired stuff
    private Player playerD;

    // bot stuff
    public Transform[] basePosition;
    public Transform p3DoublesPosition;
    public Transform p3Position;

    // camera variables
    float horizontal;
    float vertical;

    // test for fixed update
    bool shortServe;
    bool longServe;
    bool drop;
    bool drive;
    bool clear;
    bool detectHit;

    // Start is called before the first frame update
    void Start()
    {
        // rewired init
        playerD = ReInput.players.GetPlayer(0);
        
        // Initialize and get component for players
        strokePossible = 0;
        strokeState = 0;
        player = GetComponent<CharacterController>();
        shotManager = GetComponent<ShotManager>();
        currentShot = shotManager.clear;

        // Animator Component Retrieval
        playerAnim = GetComponent<Animator>();
        
        SingleStatInit();

        // Character select initialization
        // stats
        CharacterSelect();
        // initialize stat boosts from racquet
        if (DubP3Move.amIABot)
        {
            SinglePlayerInit();
        }

        // needs to be after singleplayer and characterselect
        energyCurr = energyMax;

        // Movement initialize
        allowMovement = true;
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
            // If the game hasn't started yet and p1 serves then:
            if (Points.gameStart == false && Points.server == 1)
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
            // Only goes through this code if game mode is true
            else if (Points.gameStart == true)
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

                // Checks to see if player is grounded, then checks for input for movement
                if (player.isGrounded)
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

    private void PlayerContact()
    {
        if (points.GetComponent<Points>().lastHitter != "Player1" 
            && Points.doublesReceiver != 3)
        {
            PlayAnimation();
            EnergyOnHit();
            ShuttleContact();

            // Sets the last hitter to the correct player
            points.GetComponent<Points>().lastHitter = "Player1";

            if (Points.doublesReceiver == 1)
            {
                Points.doublesReceiver = 0;
            }
        }
    }

    IEnumerator InputBuffer()
    {
        detectHit = true;
        EnergyFirstPress();
        playerAnim.Play("Ready Position");
        StartCoroutine("FrameWait");
        yield return new WaitForSeconds(0.1f);
        shortServe = false;
        longServe = false;
        clear = false;
        drive = false;
        drop = false;
        detectHit = false;
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
    }

    private void ServiceContact()
    {
        // Recreates shuttle
        shuttleObject.SetActive(true);
        shuttle.position = transform.position + new Vector3(0.5f, 0.5f, 0);

        playerAnim.Play("Life Swing");
        StartCoroutine("FrameWait");

        // Sets game mode to true
        StartCoroutine("delayGameStart");
        player.Move(new Vector3(0, 0, 0));
        // Sets the last hitter to the correct player
        points.GetComponent<Points>().lastHitter = "Player1";
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
        float newXInacc = newInacc * Random.Range(0.975f, 1.025f);
        float newZInacc = newInacc * Random.Range(0.95f, 1.05f);
        dir += new Vector3((shuttle.position.x - transform.position.x) * newXInacc * accMod,
            0,
            (shuttle.position.z - transform.position.z) * newZInacc * 2 * accMod);

        // Public xz coords for bot to move to
        xTargetHitTo = targets[targetX].position.x + ((shuttle.position.x - transform.position.x) * newXInacc * accMod);
        zTargetHitTo = targets[targetX].position.z + ((shuttle.position.z - transform.position.z) * newZInacc * 2 * accMod);
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
        float power = 1;

        if (strokeState == 1)
        {
            power -= 0.025f;
        }
        else if (strokeState == 2)
        {
            power -= 0.05f;
        }

        if (shuttle.position.x < transform.position.x)
        {
            power *= 0.97f;
        }

        return power;
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
        if (DubP3Move.amIABot == true)
        {
            speed = 2.5f;
            inaccuracy = 0.5f;
            extraJumpPower = 1f;
            energyRegen = 1f;
        }
        else if (playerType == 1)
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

    void SinglePlayerInit()
    {
        speed *= (1 + ((speedBoost * 0.01f) / 2));
        inaccuracy -= (accuracyBoost * 0.01f);
        extraJumpPower = extraJumpPower + (smashPowerBoost * 0.01f) + smashRacquetBonus;
        swingSpeed += swingSpeedBoost;
        energyRegen *= (1 + energyBoost * 0.01f);
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

    void SingleStatInit()
    {
        if (DubP3Move.amIABot == true)
        {
            playerLevel = PlayerMovement.playerLevel;
            speedBoost = PlayerMovement.speedBoost;
            accuracyBoost = PlayerMovement.accuracyBoost;
            smashPowerBoost = PlayerMovement.smashPowerBoost;
            energyBoost = PlayerMovement.energyBoost;
        }
        else
        {
            playerLevel = 1;
            speedBoost = 0;
            accuracyBoost = 0;
            smashPowerBoost = 0;
            energyBoost = 0;
        }
    }

    void CheckLastShotType()
    {
        if (currentShot == shotManager.clear || currentShot == shotManager.longServe)
        {
            DubP3Move.redBotLastShotType = 0;
        }
        else
        {
            DubP3Move.redBotLastShotType = 1;
        }

        if (DubP3Move.redBotLastShotType == 0)
        {
            if (transform.position.z > p3Position.position.z)
            {
                p3DoublesPosition.position = basePosition[2].position;
            }
            else
            {
                p3DoublesPosition.position = basePosition[3].position;
            }
        }
        else if (transform.position.x < p3Position.position.x)
        {
            p3DoublesPosition.position = basePosition[0].position;
        }
        else
        {
            p3DoublesPosition.position = basePosition[1].position;
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
}
