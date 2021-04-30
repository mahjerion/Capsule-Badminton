using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsDropDown : MonoBehaviour
{
    // Checks for Games
    public void DropdownIndexChangeGames(int index)
    {
        if (index == 1)
        {
            Points.gamesToWin = 2;
        }
        else if (index == 2)
        {
            Points.gamesToWin = 3;
        }
        else
        {
            Points.gamesToWin = 1;
        }
    }

    // Checks for Points
    public void DropdownIndexChangePoints(int index)
    {
        if (index == 1)
        {
            Points.pointsToWin = 11;
        }
        else if (index == 2)
        {
            Points.pointsToWin = 21;
        }
        else
        {
            Points.pointsToWin = 7;
        }
    }
}
