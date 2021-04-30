using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Photon.Pun;

public class GameSetupController : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        CreatePlayer();
    }

    // Update is called once per frame
    void CreatePlayer()
    {
        Debug.Log("Creating Player");
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonPlayer1"), new Vector3(-5f, 0.8f, 0f), Quaternion.identity);
        }
        else
        {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonPlayer2"), new Vector3(5f, 0.8f, 0f), Quaternion.identity);
        }
    }
}
