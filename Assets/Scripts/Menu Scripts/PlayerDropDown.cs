using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDropDown : MonoBehaviour
{
    // Checks for P1
    public void DropdownIndexChangeRed(int index)
    {
        PlayerMovement.playerType = index;
    }

    // Checks for P2
    public void DropdownIndexChangeBlue(int index)
    {
        BotMovement.playerType = index;
    }

    // Checks for P1 Racquet
    public void DropdownIndexRedRacquetType(int index)
    {
        PlayerMovement.racquetType = index;
    }

    // Checks for P2 Racquet
    public void DropdownIndexBlueRacquetType(int index)
    {
        BotMovement.racquetType = index;
    }

    // Doubles stuff
    public void DubsDropdownIndexChangeP1(int index)
    {
        DubP1Move.playerType = index;
    }
    public void DubsDropdownIndexChangeP2(int index)
    {
        DubP2Move.playerType = index;
    }
    public void DubsDropdownIndexChangeP3(int index)
    {
        DubP3Move.playerType = index;
    }
    public void DubsDropdownIndexChangeP4(int index)
    {
        DubP4Move.playerType = index;
    }

    // Checks for doubles Racquet
    public void DubsDropdownIndexRedRacquetTypeP1(int index)
    {
        DubP1Move.racquetType = index;
    }
    public void DubsDropdownIndexRedRacquetTypeP2(int index)
    {
        DubP2Move.racquetType = index;
    }
    public void DubsDropdownIndexRedRacquetTypeP3(int index)
    {
        DubP3Move.racquetType = index;
    }
    public void DubsDropdownIndexRedRacquetTypeP4(int index)
    {
        DubP4Move.racquetType = index;
    }
}
