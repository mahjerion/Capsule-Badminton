using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMusic : MonoBehaviour
{
    private AudioSource menuAudio;

    private void Start()
    {
        menuAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (MusicOptionMenu.menuMusic == false)
        {
            menuAudio.Stop();
        }
        else if (!menuAudio.isPlaying)
        {
            menuAudio.Play();
        }
    }
}
