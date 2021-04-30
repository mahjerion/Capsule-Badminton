using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coach : MonoBehaviour
{
    CharacterController playerTwo;

    public Transform training;
    public Transform[] targets;
    public Transform shuttleTrans;

    private Animator playerAnim;

    // Variables to store if it's possible to hit shuttle (if it's in hitbox) and which hitbox it's coming from
    public static int strokeState;
    public static int strokePossible;

    // Audio source
    public AudioSource racquetSound;
    public AudioClip liftSound;
    public AudioClip smashSound;

    // Basic stat init.
    private float inaccuracy = 0f;
    private float extraJumpPower = 1.25f;

    // Assigns ShotManager so it can be edited
    ShotManager shotManager;
    Shot currentShot;

    //private bool botJumpChanceRolled;

    public static bool enabledClears;
    public static bool enabledSmashes;
    public static bool enabledDrops;

    int targetX;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize and get component for players
        strokePossible = 0;
        strokeState = 0;
        playerTwo = GetComponent<CharacterController>();
        shotManager = GetComponent<ShotManager>();
        currentShot = shotManager.clear;

        // Animator Component Retrieval
        playerAnim = GetComponent<Animator>();

        // Prevents coach from doing nothing
        if (enabledClears == false && enabledSmashes == false && enabledDrops == false)
        {
            enabledClears = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (TrainingS.shuttleNotHitYet == true)
        {
            CoachShotSelection();
        }
    }

    private void PlayerContact()
    {
        if (strokePossible == 1 && training.GetComponent<TrainingS>().lastHitter != "Player2")
        {
            PlayAnimation();
            ShuttleContact();

            // Sets the last hitter to the correct player
            training.GetComponent<TrainingS>().lastHitter = "Player2";
        }
    }

    private Vector3 CheckHitDirection()
    {
        // Sets base shot to be in middle
        targetX = 0;

        if (currentShot == shotManager.clear)
        {
            targetX = Random.Range(0, 3);
        }
        // Drives
        else if (currentShot == shotManager.drive)
        {
            targetX = Random.Range(3, 6);
        }
        // Drops
        else if (currentShot == shotManager.drop)
        {
            targetX = Random.Range(6, 9);
        }

        // Calculates the direction between the player and the target and modifies it by relative position of
        // shuttle to the player
        Vector3 dir = targets[targetX].position - shuttleTrans.position;

        // If the player hits a shot past the service line it's more accurate
        float accMod = 1f;
        if (shuttleTrans.position.x <= 2f)
        {
            accMod = 0.5f;
        }
        dir += new Vector3((shuttleTrans.position.x - transform.position.x) * inaccuracy * accMod,
            0,
            (shuttleTrans.position.z - transform.position.z) * inaccuracy * 2 * accMod);
        return dir;
    }

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
        if (currentShot == shotManager.drop && strokeState == 2 && shuttleTrans.position.x <= 2f)
        {
            a = currentShot.xForce;
        }
        // angle for jump overhead shots that aren't smashes
        else if (currentShot == shotManager.drop && strokeState <= 1 && shuttleTrans.position.x <= 2f)
        {
            a -= 5f;
        }
        else if (shuttleTrans.position.y > 3.5f && strokeState == 0 && currentShot == shotManager.drop)
        {
            a -= 10f;
        }
        else if (shuttleTrans.position.y > 3.5f && strokeState == 0 && currentShot == shotManager.clear)
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

        if (currentShot == shotManager.drive && strokeState == 0 && shuttleTrans.position.y > 3.5f)
        {
            xJumpPower = 7f * extraJumpPower;
            yJumpPower = 4.75f * extraJumpPower;
            zJumpPower = CrossCourtSmash();
            if (shuttleTrans.position.x <= 0.65f)
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
            if (shuttleTrans.position.x <= 0.65f)
            {
                yJumpPower *= 3f;
                if (zJumpPower != 0f)
                {
                    xJumpPower *= 0.8f;
                }
            }
        }
        // corrects for drops
        else if (shuttleTrans.position.x >= 2f && strokeState == 0 && currentShot == shotManager.drop)
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
        // Sets the force + direction onto the shuttle
        shuttleTrans.GetComponent<Rigidbody>().velocity = (velocity * PowerDueToState()) * dir.normalized + new Vector3(-xJumpPower, -yJumpPower, zJumpPower);
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
            racquetSound.volume = 1.25f;
            racquetSound.pitch = 0.9f;
        }
        else if (strokeState == 0 && shuttleTrans.position.y > 3.5f && currentShot == shotManager.drop)
        {
            racquetSound.clip = smashSound;
            racquetSound.volume = 1f;
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

    private void CoachShotSelection()
    {
        if (Mathf.Abs(shuttleTrans.position.z) >= 3.5f || shuttleTrans.position.x >= 8.95f)
        {

        }
        else
        {
            if (strokePossible == 1)
            {
                int botStrokeType;
                bool coachShotHit = false;
                botStrokeType = Random.Range(0, 3);
                while (coachShotHit == false)
                {
                    if (botStrokeType == 0 && enabledClears == true)
                    {
                        currentShot = shotManager.clear;
                        coachShotHit = true;
                    }
                    else if (botStrokeType == 1 && enabledSmashes == true)
                    {
                        currentShot = shotManager.drive;
                        coachShotHit = true;
                    }
                    else if (botStrokeType == 2 && enabledDrops == true)
                    {
                        currentShot = shotManager.drop;
                        coachShotHit = true;
                    }
                    else
                    {
                        botStrokeType = Random.Range(0, 3);
                    }
                }
                PlayerContact();
            }
        }
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
        if (shuttleTrans.position.y > 3.5f)
        {
            zPower *= 1.75f;
        }
        return zPower;
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
        if (shuttleTrans.position.y > 3.5f)
        {
            zPower *= 2;
        }
        return zPower;
    }
}
