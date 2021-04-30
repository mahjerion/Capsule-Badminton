using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SinglePlayerCamera : MonoBehaviour
{
    private Camera mainCam;
    private Transform mainCamTrans;
    public GameObject secondCam;
    public Camera secondCamCamera;
    public Transform secondCamTrans;

    public GameObject p1SPJoystick;
    public GameObject p1MPJoystick;
    public GameObject p2MPJoystick;
    
    // in game ui for mobile
    public RectTransform p1ScoreTrans;
    public RectTransform p2ScoreTrans;
    public RectTransform p1GamesTrans;
    public RectTransform p2GamesTrans;
    public RectTransform info;
    public RectTransform gameMenu;
    public Text p1GamesText;

    // Testing purposes for mobile
    public static bool isMobile;
    public static bool altCamera;
    public static bool coopBots;
    public static bool twoPlayerCam;

    // Start is called before the first frame update
    void Start()
    {
        p1SPJoystick.SetActive(false);
        p1MPJoystick.SetActive(false);
        p2MPJoystick.SetActive(false);

        // Gets components
        mainCam = GetComponent<Camera>();
        mainCamTrans = GetComponent<Transform>();

        if (altCamera)
        {
            mainCamTrans.SetPositionAndRotation(new Vector3(0f, 7f, -9f), Quaternion.Euler(25f, 0f, 0f));
            mainCam.fieldOfView = 90f;
            secondCamTrans.SetPositionAndRotation(new Vector3(0f, 7f, -9f), Quaternion.Euler(25f, 0f, 0f));
            secondCamCamera.fieldOfView = 90f;
        }
        else
        {
            mainCamTrans.SetPositionAndRotation(new Vector3(-15f, 5f, 0f), Quaternion.Euler(15f, 90f, 0f));
            mainCam.fieldOfView = 60f;
            secondCamTrans.SetPositionAndRotation(new Vector3(15f, 5f, 0f), Quaternion.Euler(15f, 270f, 0f));
            secondCamCamera.fieldOfView = 60f;
        }

        if (twoPlayerCam == false)
        {
            mainCam.rect = new Rect(0f, 0f, 1f, 1f);
            secondCam.SetActive(false);
        }

        if (isMobile && twoPlayerCam)
        {
            if (Points.doublesOn && altCamera == false)
            {
                secondCamTrans.SetPositionAndRotation(new Vector3(-15f, 5f, 0f), Quaternion.Euler(15f, 90f, 0f));
            }

            mainCam.rect = new Rect(0f, 0f, 0.5f, 1f);
            secondCam.SetActive(true);

            p1ScoreTrans.anchoredPosition = new Vector2(-70, -55);
            p1ScoreTrans.anchorMin = new Vector2(0.5f, 1f);
            p1ScoreTrans.anchorMax = new Vector2(0.5f, 1f);
            p1ScoreTrans.pivot = new Vector2(0.5f, 1f);

            p2ScoreTrans.anchoredPosition = new Vector2(70, -55);
            p2ScoreTrans.anchorMin = new Vector2(0.5f, 1f);
            p2ScoreTrans.anchorMax = new Vector2(0.5f, 1f);
            p2ScoreTrans.pivot = new Vector2(0.5f, 1f);

            p1GamesTrans.anchoredPosition = new Vector2(-70, 50);
            p1GamesTrans.anchorMin = new Vector2(0.5f, 0);
            p1GamesTrans.anchorMax = new Vector2(0.5f, 0);
            p1GamesTrans.pivot = new Vector2(0.5f, 0);
            p1GamesTrans.rotation *= Quaternion.Euler(0, 0, 180);
            p1GamesText.alignment = TextAnchor.UpperLeft;

            p2GamesTrans.anchoredPosition = new Vector2(70, 50);
            p2GamesTrans.anchorMin = new Vector2(0.5f, 0);
            p2GamesTrans.anchorMax = new Vector2(0.5f, 0);
            p2GamesTrans.pivot = new Vector2(0.5f, 0);
            p2GamesTrans.rotation *= Quaternion.Euler(0, 0, 180);

            info.anchoredPosition = new Vector2(0, -80);

            gameMenu.anchoredPosition = new Vector2(0, 0);
            gameMenu.anchorMin = new Vector2(0.5f, 0.5f);
            gameMenu.anchorMax = new Vector2(0.5f, 0.5f);
            gameMenu.pivot = new Vector2(0.5f, 0.5f);
            gameMenu.rotation *= Quaternion.Euler(0, 0, -90);

            // set cam rotation
            mainCamTrans.rotation *= Quaternion.Euler(0, 0, 90);
            secondCamTrans.rotation *= Quaternion.Euler(0, 0, -90);

            p1MPJoystick.SetActive(true);
            p2MPJoystick.SetActive(true);
        }
        else if (isMobile && twoPlayerCam == false)
        {
            p1SPJoystick.SetActive(true);
        }
    }
}
