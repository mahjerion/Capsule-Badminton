using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileAssembler : MonoBehaviour
{
    public GameObject double4PButton;
    public GameObject controlsButton;
    private void Start()
    {
        // Variable for turning on or off mobile mode
        SinglePlayerCamera.isMobile = true;

        if (SinglePlayerCamera.isMobile == true)
        {
            double4PButton.SetActive(false);
            controlsButton.SetActive(false);
        }
    }
}
