using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundBot2 : MonoBehaviour
{
    CharacterController botTwo;

    public GameObject shuttleObject;

    // Allows us to reference and modify the transform of the shuttle
    public Transform shuttle;
    public Transform basePosition;

    float speed = 6f;
    float jumpSpeed = 4f;
    float gravity = 9.8f;
    float extraJumpPower = 1.5f;

    // Sets base shot to be in middle
    int targetX = 0;
    // Makes an array accessible by inspector for targets
    public Transform[] targets;

    // Sets initial movement to 0
    private Vector3 moveDirection = Vector3.zero;

    // Sets animator variable
    private Animator playerAnim;

    public static int strokeState;
    public static int strokePossible;

    // Assigns ShotManager so it can be edited
    ShotManager shotManager;
    Shot currentShot;

    // Audio source
    public AudioSource racquetSound;
    public AudioClip liftSound;
    public AudioClip smashSound;

    // Bot Bools
    public static bool allowMovement;
    public static bool delayStarted;
    private bool botJumpChanceRolled;

    public static float xTargetHitTo;
    public static float zTargetHitTo;

    bool botShot;

    // Start is called before the first frame update
    void Start()
    {
        strokeState = 0;
        strokePossible = 0;

        // Initialize and get component for players
        botTwo = GetComponent<CharacterController>();
        shotManager = GetComponent<ShotManager>();
        currentShot = shotManager.clear;

        // Animator Component Retrieval
        playerAnim = GetComponent<Animator>();

        // Movement initialize
        allowMovement = true;
        delayStarted = false;
        botJumpChanceRolled = false;
    }

    private void Update()
    {
        if (allowMovement == true)
        {
            if (delayStarted == true)
            {
                botShot = true;
            }
            if (botTwo.isGrounded)
            {
                if (BackgroundBot1.lastHitter == "bot1")
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
                            if (BackgroundBot1.xTargetHitTo <= 3)
                            {
                                moveDirection = new Vector3(BackgroundBot1.xTargetHitTo * 0.85f - transform.position.x,
                                    0, BackgroundBot1.zTargetHitTo * 0.95f - transform.position.z);
                            }
                            else
                            {
                                moveDirection = new Vector3(BackgroundBot1.xTargetHitTo * 0.8f - transform.position.x,
                                    0, BackgroundBot1.zTargetHitTo * 0.9f - transform.position.z);
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
                    float zMod = 0;
                    float xMod = 0;
                    if (targetX == 0 || targetX == 3 || targetX == 6)
                    {
                        zMod = -0.35f;
                    }
                    else if (targetX == 2 || targetX == 5 || targetX == 8)
                    {
                        zMod = 0.35f;
                    }
                    if (targetX <= 2)
                    {
                        xMod = 0.35f;
                    }
                    else if (targetX > 2 && targetX < 9)
                    {
                        xMod = -0.35f;
                    }
                    moveDirection = new Vector3(basePosition.position.x - transform.position.x + xMod, 0, basePosition.position.z - transform.position.z + zMod);
                }

                if (Vector3.Magnitude(moveDirection) > 0.5f)
                {
                    moveDirection.Normalize();
                }

                moveDirection *= speed;

                // checks to see if bot can jump and if he hasnt then he can for this shot instance
                if (BackgroundBot1.lastHitter == "bot1")
                {
                    // Checks to see if shuttle is on top of bot, if it is then jump
                    if (Mathf.Abs(shuttle.position.x - transform.position.x) <= 2f
                        && Mathf.Abs(shuttle.position.y - transform.position.y) >= 3.5f
                        && Mathf.Abs(shuttle.position.z - transform.position.z) <= 2f
                        && Mathf.Abs(shuttle.position.y - transform.position.y) <= 4.5f
                        && botJumpChanceRolled == false)
                    {
                        botJumpChanceRolled = true;
                        int randomJumpChance = Random.Range(0, 4);

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
        if (allowMovement == false && botTwo.isGrounded)
        {
            // Force of gravity applied per frame update - outside because gravity always affects
            moveDirection.x = 0.0f;
            moveDirection.z = 0.0f;
        }
        moveDirection.y -= gravity * Time.deltaTime;
        // Moves the player
        botTwo.Move(moveDirection * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (botShot)
        {
            botShot = false;
            BotShotSelection();
        }
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
                        botStrokeType = Random.Range(0, 5);
                        if (botStrokeType <= 1)
                        {
                            currentShot = shotManager.drop;
                        }
                        else if (botStrokeType <= 3)
                        {
                            currentShot = shotManager.drive;
                        }
                        else
                        {
                            currentShot = shotManager.clear;
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

    private void PlayerContact()
    {
        if (strokePossible == 1 && BackgroundBot1.lastHitter != "bot2")
        {
            PlayAnimation();
            StartCoroutine("FrameWait");
            ShuttleContact();

            // Sets the last hitter to the correct player
            BackgroundBot1.lastHitter = "bot2";
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
            xJumpPower = 7f * extraJumpPower;
            yJumpPower = 4.75f * extraJumpPower;
            zJumpPower = CrossCourtSmash();
            if (shuttle.position.x <= 0.65f)
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
            xJumpPower = 5.5f * extraJumpPower;
            yJumpPower = 3.25f * extraJumpPower;
            zJumpPower = CrossCourtSmash();
            if (shuttle.position.x <= 0.65f)
            {
                yJumpPower *= 3f;
                if (zJumpPower != 0f)
                {
                    xJumpPower *= 0.8f;
                }
            }
        }
        // corrects for drops
        else if (shuttle.position.x >= 2f && strokeState == 0 && currentShot == shotManager.drop)
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

        // Sets the force + direction onto the shuttle
        shuttle.GetComponent<Rigidbody>().velocity = (velocity * PowerDueToState()) * dir.normalized + new Vector3(-xJumpPower, -yJumpPower, zJumpPower);

        // Let's the bot jump again (one roll per "shot instance")
        botJumpChanceRolled = false;
    }

    // Corrects for the added forward velocity by adding some to the horizontal
    private float CrossCourtDrop()
    {
        float zPower;
        if (transform.position.z >= 2 && (targetX == 7 || targetX == 6))
        {
            zPower = -1f;
        }
        else if (transform.position.z <= -2 && (targetX == 7 || targetX == 8))
        {
            zPower = 1f;
        }
        else if (transform.position.z < 2 && transform.position.z > -2)
        {
            if (targetX == 8)
            {
                zPower = 0.5f;
            }
            else if (targetX == 6)
            {
                zPower = -0.5f;
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

    private Vector3 CheckHitDirection()
    {
        targetX = 0;

        // Checks to see the type of shot being hit, then the direction the player is hitting in
        // Clears
        if (currentShot == shotManager.clear)
        {
            targetX = Random.Range(0, 3);
            if (targetX == 1)
            {
                targetX = Random.Range(0, 2);
                if (targetX == 0)
                {
                    targetX = 0;
                }
                else
                {
                    targetX = 2;
                }
            }
        }
        // Drives
        else if (currentShot == shotManager.drive)
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
        // Drops
        else if (currentShot == shotManager.drop)
        {
            targetX = Random.Range(6, 9);
        }

        // Calculates the direction between the player and the target and modifies it by relative position of
        // shuttle to the player
        Vector3 dir = targets[targetX].position - shuttle.position;
        
        // Public xz coords for bot to move to
        xTargetHitTo = targets[targetX].position.x;
        zTargetHitTo = targets[targetX].position.z;
        return dir;
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
        yield return new WaitForSeconds((0.25f) * LagDueToState());
        allowMovement = true;
    }

    IEnumerator DelayBotReaction()
    {
        if (delayStarted == false)
        {
            yield return new WaitForSeconds(0.1f);
            delayStarted = true;
        }
        else
        {
            yield return null;
        }
    }
}
