using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;


public class CameraSetup : MonoBehaviourPun
{
    
    void Start()
    {

        if (photonView.IsMine) //포톤뷰가 나의 것이면 로컬
        {
            CinemachineVirtualCamera virtualCamera = FindObjectOfType<CinemachineVirtualCamera>(); 
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;
        }
        
    }

    
}
