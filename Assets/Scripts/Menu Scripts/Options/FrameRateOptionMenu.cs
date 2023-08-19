using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FrameRateOptionMenu : MonoBehaviour
{
    public static int frameMenu;
    public TextMeshProUGUI frameText;

    // Start is called before the first frame update
    void Start()
    {
        if (frameMenu == 120)
        {
            frameText.text = "FRAMERATE: 120";
        }
        else if (frameMenu == 60)
        {
            frameText.text = "FRAMERATE: 60";
        }
        else if (frameMenu == 90)
        {
            frameText.text = "FRAMERATE: 90";
        }
        else
        {
            frameText.text = "FRAMERATE: 30";
        }
    }

    public void FrameModifier()
    {
        if (frameMenu == 30)
        {
            frameText.text = "FRAMERATE: 60";
            frameMenu = 60;
        }
        else if (frameMenu == 60)
        {
            frameText.text = "FRAMERATE: 90";
            frameMenu = 90;
        }
        else if (frameMenu == 90)
        {
            frameText.text = "FRAMERATE: 120";
            frameMenu = 120;
        }
        else if (frameMenu == 120)
        {
            frameText.text = "FRAMERATE: 30";
            frameMenu = 30;
        }
        Application.targetFrameRate = frameMenu;
        ES3.Save<int>("frameRate", frameMenu);
    }
}
