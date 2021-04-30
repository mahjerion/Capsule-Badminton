using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicOptionGame : MonoBehaviour
{
    private AudioSource gameAudio;

    private void Start()
    {
        gameAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (MusicOptionMenu.menuMusic == false)
        {
            gameAudio.Stop();
        }
        else if (!gameAudio.isPlaying)
        {
            gameAudio.Play();
        }
    }
}
