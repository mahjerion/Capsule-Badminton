using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MusicOptionMenu : MonoBehaviour
{
    public static bool menuMusic;
    public TextMeshProUGUI musicText;

    private void Start()
    {
        if (menuMusic == true)
        {
            musicText.text = "MUSIC (ENABLED)";
        }
        else
        {
            musicText.text = "MUSIC (DISABLED)";
        }
    }

    public void MusicModifier()
    {
        if (menuMusic == true)
        {
            musicText.text = "MUSIC (DISABLED)";
            menuMusic = false;
        }
        else
        {
            musicText.text = "MUSIC (ENABLED)";
            menuMusic = true;
        }
        ES3.Save<bool>("musicOn", menuMusic);
    }
}
