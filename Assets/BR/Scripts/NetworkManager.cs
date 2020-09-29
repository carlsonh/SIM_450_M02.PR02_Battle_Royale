using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class NetworkManager : MonoBehaviourPunCallbacks
{
    public int maxPlayers = 10;
    public static NetworkManager instance;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        if (instance != null && instance != this)
        {///Is there an existing NetMan that's not this one
            gameObject.SetActive(false);///If so, this one isn't needed
        }
        else
        {
            //This instance should become the NetMan
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }



    #region Lobby Management

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Director server");
        PhotonNetwork.JoinLobby();
    }

    public void CreateRoom(string roomName)
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = (byte)maxPlayers;

        PhotonNetwork.CreateRoom(roomName, options);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    [PunRPC] //Complained to be added, necessary?
    public void ChangeScene(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }

    #endregion Lobby Management




    #region Player Disconnects

    public override void OnDisconnected(DisconnectCause disconnectCause)
    {
        PhotonNetwork.LoadLevel("MenuScene");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GameManager.instance.alivePlayers--;
        GameUI.instance.UpdatePlayerInfoText();

        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.instance.CheckWinCondition();
        }
    }

    #endregion Player Disconnects

}
