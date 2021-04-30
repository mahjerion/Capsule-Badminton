using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraOptionMenu : MonoBehaviour
{
    public TextMeshProUGUI cameraText;

    private void Start()
    {
        if (SinglePlayerCamera.altCamera == true)
        {
            cameraText.text = "CAMERA: SIDE VIEW";
        }
        else
        {
            cameraText.text = "CAMERA: BACK VIEW";
        }
    }

    public void CameraModifier()
    {
        if (SinglePlayerCamera.altCamera == true)
        {
            cameraText.text = "CAMERA: BACK VIEW";
            SinglePlayerCamera.altCamera = false;
        }
        else
        {
            cameraText.text = "CAMERA: SIDE VIEW";
            SinglePlayerCamera.altCamera = true;
        }
        ES3.Save<bool>("cameraOn", SinglePlayerCamera.altCamera);
    }
}
