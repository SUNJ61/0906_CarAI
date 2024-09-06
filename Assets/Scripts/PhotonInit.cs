using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class PhotonInit : MonoBehaviourPunCallbacks
{
    public string Version = "Car1.0";
    public InputField userId;

    private void Awake()
    {
        PhotonNetwork.GameVersion = Version;
        //게임 버전 설정
         // 자동으로 씬동기화 처리 하도록 설정 한다.
        //PhotonNetwork.AutomaticallySyncScene = true;
        
      

        if (!PhotonNetwork.IsConnected)
        {  
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("마스터 서버에 접속...");
        PhotonNetwork.JoinLobby();
        userId.text = GetUserID();
    }
    string GetUserID()
    {
        string userID = PlayerPrefs.GetString("USER_ID");
        if (string.IsNullOrEmpty(userID))
        {
            userID = "USER_" + Random.Range(0, 999).ToString("000");
        }
        return userID;
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("로비 접속 완료");
        //PhotonNetwork.JoinRandomRoom();
        //아무방이나 접속 
      
    }
    
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(" 랜덤룸 참가 실패 ");
        PhotonNetwork.CreateRoom("CarRoom",new RoomOptions { MaxPlayers = 4 });
    }
    public override void OnCreatedRoom()
    {
        Debug.Log(" 방생성 성공!!");
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("방 참가 성공");
        StartCoroutine(LoadPlayScene());
    }
    IEnumerator LoadPlayScene()
    {
        PhotonNetwork.IsMessageQueueRunning = false;
        AsyncOperation ao = SceneManager.LoadSceneAsync("F1TrackDisplayScene");
        yield return ao;
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("오류코드" + returnCode.ToString());
        Debug.Log("방생성 실패 : " + message);
    }
    public void OnClickJoinRandomRoom() //>>조인랜덤룸버튼 누르면 나오는기능 로직
    {
        PhotonNetwork.NickName = userId.text;
        PlayerPrefs.SetString("USER_ID", userId.text);
        //플레이어 이름을 저장
        PhotonNetwork.JoinRandomRoom();
    }
    private void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.NetworkClientState.ToString());
                        //포톤네트워크에서 클라이언트 상태 정보를 알려준다.
    }
}
