using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetShuttle : MonoBehaviour
{
    public static bool landed = false;
    public Transform shuttle;
    public Transform bot1;
    public GameObject shuttleObject;

    // Update is called once per frame
    void Update()
    {
        if (landed)
        {
            landed = false;
            shuttleObject.SetActive(false);
            BackgroundBot1.lastHitter = "bot2";
            BackgroundBot1.delayStarted = true;
            shuttle.position = bot1.position + new Vector3(0, 3f, 0f);
            shuttle.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            shuttleObject.SetActive(true);
        }
    }
}
