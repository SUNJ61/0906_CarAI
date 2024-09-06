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
        //���� ���� ����
         // �ڵ����� ������ȭ ó�� �ϵ��� ���� �Ѵ�.
        //PhotonNetwork.AutomaticallySyncScene = true;
        
      

        if (!PhotonNetwork.IsConnected)
        {  
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("������ ������ ����...");
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
        Debug.Log("�κ� ���� �Ϸ�");
        //PhotonNetwork.JoinRandomRoom();
        //�ƹ����̳� ���� 
      
    }
    
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(" ������ ���� ���� ");
        PhotonNetwork.CreateRoom("CarRoom",new RoomOptions { MaxPlayers = 4 });
    }
    public override void OnCreatedRoom()
    {
        Debug.Log(" ����� ����!!");
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("�� ���� ����");
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
        Debug.Log("�����ڵ�" + returnCode.ToString());
        Debug.Log("����� ���� : " + message);
    }
    public void OnClickJoinRandomRoom() //>>���η������ư ������ �����±�� ����
    {
        PhotonNetwork.NickName = userId.text;
        PlayerPrefs.SetString("USER_ID", userId.text);
        //�÷��̾� �̸��� ����
        PhotonNetwork.JoinRandomRoom();
    }
    private void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.NetworkClientState.ToString());
                        //�����Ʈ��ũ���� Ŭ���̾�Ʈ ���� ������ �˷��ش�.
    }
}
