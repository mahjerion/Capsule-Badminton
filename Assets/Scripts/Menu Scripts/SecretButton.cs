using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretButton : MonoBehaviour
{
    public GameObject secretLevel;

    private void Start()
    {
        if (PlayerMovement.playerLevel >= 25)
        {
            secretLevel.SetActive(true);
        }
        else
        {
            secretLevel.SetActive(false);
        }
    }
}
