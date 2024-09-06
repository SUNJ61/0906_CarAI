using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{

    int PlayerClamp;
    int Count;
    void Awake()
    {

        PlayerClamp = PhotonNetwork.CurrentRoom.MaxPlayers;
                        //최대 플레이어 수
        Count = PhotonNetwork.CurrentRoom.PlayerCount;
        if (PlayerClamp > 5) return;
        CreateCars(Count);
        PhotonNetwork.IsMessageQueueRunning = true;
    }
  

    void CreateCars(int playerCount)
    {
       
        switch (playerCount)
        {
            case 1:
                Vector3 SpawnPos1 = new Vector3(46f, 0.5f, 0.88f);
                Quaternion spawnRot = Quaternion.Euler(0f, -88f, 0f);
                PhotonNetwork.Instantiate("PlayerCar", SpawnPos1, spawnRot);
                break;
            case 2:
                Vector3 SpawnPos2 = new Vector3(45.98f, 0.5f, -3.48f);
                Quaternion spawnRot2 = Quaternion.Euler(0f, -88f, 0f);
                PhotonNetwork.Instantiate("PlayerCar", SpawnPos2, spawnRot2);
                break;
            case 3:
                Vector3 SpawnPos3 = new Vector3(45.98f, 0.5f, -10.55f);
                Quaternion spawnRot3 = Quaternion.Euler(0f, -88f, 0f);
                PhotonNetwork.Instantiate("PlayerCar", SpawnPos3, spawnRot3);
                break;
            case 4:
                Vector3 SpawnPos4 = new Vector3(46.29f, 0.5f, 9.96f);
                Quaternion spawnRot4 = Quaternion.Euler(0f, -88f, 0f);
                PhotonNetwork.Instantiate("PlayerCar", SpawnPos4, spawnRot4);
                break;
           
            
        }
    }

   
}
