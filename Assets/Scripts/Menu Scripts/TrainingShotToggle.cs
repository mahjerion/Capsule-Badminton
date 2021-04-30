using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingShotToggle : MonoBehaviour
{
    private void Start()
    {
        Coach.enabledClears = true;
        Coach.enabledSmashes = true;
        Coach.enabledDrops = true;
    }

    public void ClearsToggle()
    {
        if (Coach.enabledClears == true)
        {
            Coach.enabledClears = false;
        }
        else if (Coach.enabledClears == false)
        {
            Coach.enabledClears = true;
        }
    }

    public void SmashToggle()
    {
        if (Coach.enabledSmashes == true)
        {
            Coach.enabledSmashes = false;
        }
        else if (Coach.enabledSmashes == false)
        {
            Coach.enabledSmashes = true;
        }
    }
    public void DropToggle()
    {
        if (Coach.enabledDrops == true)
        {
            Coach.enabledDrops = false;
        }
        else if (Coach.enabledDrops == false)
        {
            Coach.enabledDrops = true;
        }
    }
}
