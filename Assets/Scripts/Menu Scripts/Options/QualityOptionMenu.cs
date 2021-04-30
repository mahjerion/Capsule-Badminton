using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QualityOptionMenu : MonoBehaviour
{
    public static int qualityMenu;
    public TextMeshProUGUI qualityText;

    // Start is called before the first frame update
    void Start()
    {
        if (qualityMenu == 3)
        {
            qualityText.text = "QUALITY: ULTRA";
        }
        else if (qualityMenu == 2)
        {
            qualityText.text = "QUALITY: HIGH";
        }
        else if (qualityMenu == 1)
        {
            qualityText.text = "QUALITY: MEDIUM";
        }
        else
        {
            qualityText.text = "QUALITY: LOW";
        }
    }

    public void QualityModifier()
    {
        if (qualityMenu == 0)
        {
            qualityText.text = "QUALITY: MEDIUM";
            qualityMenu = 1;
        }
        else if (qualityMenu == 1)
        {
            qualityText.text = "QUALITY: HIGH";
            qualityMenu = 2;
        }
        else if (qualityMenu == 2)
        {
            qualityText.text = "QUALITY: ULTRA";
            qualityMenu = 3;
        }
        else if (qualityMenu == 3)
        {
            qualityText.text = "QUALITY: LOW";
            qualityMenu = 0;
        }
        QualitySettings.SetQualityLevel(qualityMenu);
        ES3.Save<int>("qualityLevel", qualityMenu);
    }
}
