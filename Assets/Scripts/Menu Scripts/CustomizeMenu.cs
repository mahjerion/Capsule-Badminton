using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CustomizeMenu : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI spdText;
    public TextMeshProUGUI accText;
    public TextMeshProUGUI pwrText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI nrgText;

    private void Start()
    {
        levelText.text = "Level: " + PlayerMovement.playerLevel;
        expText.text = "XP: " + Mathf.Round((PlayerMovement.playerEXP / (5 * Mathf.Pow(2.5f, Mathf.Sqrt(PlayerMovement.playerLevel - 1))) * 100)) + "%";
        pointsText.text = "Points: " + PlayerMovement.playerPoints;
        spdText.text = "Movement: +" + PlayerMovement.speedBoost + "%";
        accText.text = "Accuracy: +" + (PlayerMovement.accuracyBoost * 2) + "%";
        pwrText.text = "Power: +" + PlayerMovement.smashPowerBoost + "%";
        nrgText.text = "Stamina: +" + PlayerMovement.energyBoost + "%";
    }

    public void IncreaseSpeed()
    {
        if (PlayerMovement.playerPoints > 0 && PlayerMovement.speedBoost < 50)
        {
            PlayerMovement.playerPoints--;
            PlayerMovement.speedBoost++;
        }
        PointsUpdate();
    }
    public void IncreaseAccuracy()
    {
        if (PlayerMovement.playerPoints > 0 && PlayerMovement.accuracyBoost < 45)
        {
            PlayerMovement.playerPoints--;
            PlayerMovement.accuracyBoost++;
        }
        PointsUpdate();
    }
    public void IncreaseSmashPower()
    {
        if (PlayerMovement.playerPoints > 0 && PlayerMovement.smashPowerBoost < 50)
        {
            PlayerMovement.playerPoints--;
            PlayerMovement.smashPowerBoost++;
        }
        PointsUpdate();
    }
    public void IncreaseEnergy()
    {
        if (PlayerMovement.playerPoints > 0 && PlayerMovement.energyBoost < 50)
        {
            PlayerMovement.playerPoints--;
            PlayerMovement.energyBoost++;
        }
        PointsUpdate();
    }
    public void ResetStatPoints()
    {
        PlayerMovement.speedBoost = 0;
        PlayerMovement.accuracyBoost = 0;
        PlayerMovement.smashPowerBoost = 0;
        PlayerMovement.energyBoost = 0;

        PlayerMovement.playerPoints = PlayerMovement.playerLevel * 5 - 5;

        PointsUpdate();
    }

    private void PointsUpdate()
    {
        spdText.text = "Movement: +" + PlayerMovement.speedBoost + "%";
        accText.text = "Accuracy: +" + (PlayerMovement.accuracyBoost * 2) + "%";
        pwrText.text = "Power: +" + PlayerMovement.smashPowerBoost + "%";
        nrgText.text = "Stamina: +" + PlayerMovement.energyBoost + "%";
        pointsText.text = "Points: " + PlayerMovement.playerPoints;
    }
    public void DropdownIndexRacquetType (int index)
    {
        PlayerMovement.racquetType = index;
    }
}
