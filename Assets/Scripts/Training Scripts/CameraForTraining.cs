using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraForTraining : MonoBehaviour
{
    public GameObject p1SPJoystick;
    private Transform mainCamTrans;

    // Start is called before the first frame update
    void Start()
    {
        mainCamTrans = GetComponent<Transform>();

        if (SinglePlayerCamera.altCamera)
        {
            mainCamTrans.SetPositionAndRotation(new Vector3(0f, 7.5f, -10f), Quaternion.Euler(40f, 0f, 0f));
        }
        else
        {
            mainCamTrans.SetPositionAndRotation(new Vector3(-15f, 5f, 0f), Quaternion.Euler(15f, 90f, 0f));
        }

        if (SinglePlayerCamera.isMobile == true)
        {
            p1SPJoystick.SetActive(true);
        }
    }
}
