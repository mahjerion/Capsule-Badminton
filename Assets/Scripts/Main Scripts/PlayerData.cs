using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int level;
    public float exp;
    public int points;
    public int spdBST;
    public int accBST;
    public int pwrBST;
    public int nrgBST;
    public bool musicBool;
    public bool firstBool;

    public PlayerData ()
    {
        level = PlayerMovement.playerLevel;
        exp = PlayerMovement.playerEXP;
        points = PlayerMovement.playerPoints;
        spdBST = PlayerMovement.speedBoost;
        accBST = PlayerMovement.accuracyBoost;
        pwrBST = PlayerMovement.smashPowerBoost;
        nrgBST = PlayerMovement.energyBoost;
    }
}
