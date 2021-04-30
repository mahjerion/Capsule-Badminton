using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BackgroundOptionMenu : MonoBehaviour
{
    public TextMeshProUGUI backgroundText;
    public static bool staticBackground;
    public GameObject backgroundObject;
    public GameObject backSceneObject;

    // Start is called before the first frame update
    void Start()
    {
        if (staticBackground == true)
        {
            backgroundText.text = "STATIC BACKGROUND";
        }
        else
        {
            backgroundText.text = "DYNAMIC BACKGROUND";
        }
    }

    public void BkrndModifier()
    {
        if (staticBackground == true)
        {
            backgroundText.text = "DYNAMIC BACKGROUND";
            staticBackground = false;
            backgroundObject.SetActive(false);
            backSceneObject.SetActive(true);
            BackgroundBot1.delayStarted = true;
            BackgroundBot2.delayStarted = true;
            BackgroundBot1.allowMovement = true;
            BackgroundBot2.allowMovement = true;
        }
        else
        {
            backgroundText.text = "STATIC BACKGROUND";
            staticBackground = true;
            backgroundObject.SetActive(true);
            backSceneObject.SetActive(false);
            BackgroundBot1.delayStarted = true;
            BackgroundBot2.delayStarted = true;
            BackgroundBot1.allowMovement = true;
            BackgroundBot2.allowMovement = true;
        }
        ES3.Save<bool>("staticBack", staticBackground);
    }
}
