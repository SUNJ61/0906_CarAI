using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetInOut : MonoBehaviour
{
    private string playerTag = "Player";
    public  bool isGetin = false;
    public GameObject fpsPlayer;
    public Camera mainCam;
    PlayerCar playerCar;
    void Start()
    {
        fpsPlayer = GameObject.FindWithTag(playerTag);
        mainCam  = Camera.main;
        playerCar = GetComponentInParent<PlayerCar>();
        AudioListener listener = mainCam.GetComponent<AudioListener>();
        listener.enabled = false;

    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag(playerTag))
        {
            isGetin = true;
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E)&&isGetin)
        {
            PlayerGetIn();
        }
        else if(Input.GetKeyDown(KeyCode.Q))
        {
            PlayerGetOut();
        }
    }
    private void PlayerGetIn()
    {
        isGetin = true;
        fpsPlayer.SetActive(false);
        mainCam.depth = 0f;
        mainCam.GetComponent<AudioListener>().enabled = true;
        
        fpsPlayer.transform.GetComponentInChildren<Camera>().depth = -1f;
        fpsPlayer.transform.GetComponentInChildren<AudioListener>().enabled = false;
        
     
    }
    void PlayerGetOut()
    {
        fpsPlayer.transform.position = transform.position;
        fpsPlayer.transform.position = new Vector3(fpsPlayer.transform.position.x - Random.Range (5f,10f), fpsPlayer.transform.position.y,
                           fpsPlayer.transform.position.z);
        mainCam.GetComponent<AudioListener>().enabled = false;
        fpsPlayer.transform.GetComponentInChildren<AudioListener>().enabled = true;
        fpsPlayer.SetActive(true); //FPs 컨트롤러 오브젝트를 잠시 오프한다
        
        playerCar.CarBreak();
    }
}
