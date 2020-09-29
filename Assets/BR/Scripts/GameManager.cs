using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPun
{
    [Header("Players")]
    public string playerPrefabLocation;
    public PlayerController[] players;
    public Transform[] spawnPoints;
    public int alivePlayers;
    private int playersInGame;


    [Header("GameState")]
    public float postGameTime;



    // instance
    public static GameManager instance;

    #region Game Startup
    void Awake()
    {
        instance = this;
    }


    void Start()
    {
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        alivePlayers = players.Length;
        photonView.RPC("ImInGame", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void ImInGame()
    {
        playersInGame++;
        if (PhotonNetwork.IsMasterClient && playersInGame == PhotonNetwork.PlayerList.Length)
            photonView.RPC("SpawnPlayer", RpcTarget.All);
    }

    [PunRPC]
    void SpawnPlayer()
    {
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);
        // initialize the player for all other players
        playerObj.GetComponent<PlayerController>().photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }

    #endregion Game Startup





    #region GetPlayer

    [PunRPC]
    public PlayerController GetPlayer(int playerId)
    {
        foreach (PlayerController player in players)
        {//RIP LINQ
            if (player != null && player.id == playerId)
            {
                return player;
            }
        }
        return null;
    }

    [PunRPC]
    public PlayerController GetPlayer(GameObject playerGO)
    {
        foreach (PlayerController player in players)
        {//RIP LINQ
            if (player != null && player.gameObject == playerGO)
            {
                return player;
            }
        }
        return null;
    }

    #endregion GetPlayer







    #region GameState

    public void CheckWinCondition()
    {
        if (alivePlayers == 1)
        {
            photonView.RPC("WinGame", RpcTarget.All, players.First(x => !x.dead).id);
        }
    }


    [PunRPC]
    void WinGame(int winningPlayer)
    {
        //Set win UI
        GameUI.instance.SetWinText(GetPlayer(winningPlayer).photonPlayer.NickName);

        Invoke("GoBackToMenu", postGameTime);

    }

    void GoBackToMenu()
    {
        NetworkManager.instance.ChangeScene("Menu");
    }

    #endregion GameState

}
