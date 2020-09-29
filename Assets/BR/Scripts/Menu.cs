using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class Menu : MonoBehaviourPunCallbacks, ILobbyCallbacks
{
    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject createRoomScreen;
    public GameObject lobbyScreen;
    public GameObject lobbyBrowserScreen;

    [Header("Main Screen")]
    public Button createRoomButton;
    public Button findRoomButton;

    [Header("Lobby")]
    public TextMeshProUGUI playerListText;
    public TextMeshProUGUI roomInfoText;
    public Button startGameButton;

    [Header("Lobby Browser")]
    public RectTransform roomListContainer;
    public GameObject roomButtonPrefab;
    private List<GameObject> roomButtons = new List<GameObject>();
    private List<RoomInfo> roomList = new List<RoomInfo>();



    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        //Disable menu buttons at start
        createRoomButton.interactable = false;
        findRoomButton.interactable = false;

        // enable the cursor since we hide it when we play the game
        Cursor.lockState = CursorLockMode.None;

        // are we in a game?
        if (PhotonNetwork.InRoom)
        {
            // go to the lobby
            // make the room visible
            PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
        //SetScreen(lobbyScreen);
        //UpdateLobbyUI();


    }

    // changes the currently visible screen
    void SetScreen(GameObject screen)
    {
        // disable all other screens
        mainScreen.SetActive(false);
        createRoomScreen.SetActive(false);
        lobbyScreen.SetActive(false);
        lobbyBrowserScreen.SetActive(false);
        // activate the requested screen
        screen.SetActive(true);

        if (screen == lobbyBrowserScreen)
            UpdateLobbyBrowserUI();
    }

    public void OnPlayerNameValueChanged(TMP_InputField playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text;
    }


    public override void OnConnectedToMaster()
    {
        // enable the menu buttons once we connect to the server
        createRoomButton.interactable = true;
        findRoomButton.interactable = true;
    }


    #region UI Buttons

    //Main Screen
    public void OnCreateRoomButton()
    {
        SetScreen(createRoomScreen);
    }

    public void OnFindRoomButton()
    {
        SetScreen(lobbyBrowserScreen);
    }



    //Create Room Screen
    public void OnBackButton()
    {
        SetScreen(mainScreen);
    }

    public void OnCreateButton(TMP_InputField roomNameInput)
    {
        Debug.Log("Trying to create room: " + roomNameInput.text);
        NetworkManager.instance.CreateRoom(roomNameInput.text);
        Debug.Log("Created room: " + roomNameInput.text);
    }


    //In-lobby, game start screen

    public void OnStartGameButton()
    {
        // hide the room
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        // tell everyone to load the game scene
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "GameScene");
    }

    public void OnLeaveLobbyButton()
    {
        PhotonNetwork.LeaveRoom();
        SetScreen(mainScreen);
    }


    //Lobby Browser Screen

    GameObject CreateRoomButton()
    {
        GameObject buttonObj = Instantiate(roomButtonPrefab, roomListContainer.transform)
       ;
        roomButtons.Add(buttonObj);
        return buttonObj;
    }

    public void OnJoinRoomButton(string roomName)
    {//Clicked on a room's join button
        NetworkManager.instance.JoinRoom(roomName);
    }

    public void OnRefreshButton()
    {
        UpdateLobbyBrowserUI();
    }

    #endregion




    #region Lobby UI Events

    public override void OnJoinedRoom()
    {
        SetScreen(lobbyScreen);
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }


    [PunRPC]
    void UpdateLobbyUI()
    {
        // enable or disable the start game button depending on if we're the host
        startGameButton.interactable = PhotonNetwork.IsMasterClient;
        // display all the players
        playerListText.text = "";
        foreach (Player player in PhotonNetwork.PlayerList)
            playerListText.text += player.NickName + "\n";
        // set the room info text
        roomInfoText.text = "<b>Room Name</b>\n" + PhotonNetwork.CurrentRoom.Name;
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyUI();
    }



    void UpdateLobbyBrowserUI()
    {
        // disable all room buttons
        foreach (GameObject button in roomButtons)
            button.SetActive(false);
        // display all current rooms in the master server
        for (int x = 0; x < roomList.Count; ++x)
        {
            // get or create the button object
            GameObject button = x >= roomButtons.Count ? CreateRoomButton() : roomButtons
           [x];
            button.SetActive(true);
            // set the room name and player count texts
            button.transform.Find("RoomNameText").GetComponent<TextMeshProUGUI>().text =
           roomList[x].Name;
            button.transform.Find("PlayerCountText").GetComponent<TextMeshProUGUI>().text
            = roomList[x].PlayerCount + " / " + roomList[x].MaxPlayers;

            // set the button OnClick event
            Button buttonComp = button.GetComponent<Button>();
            string roomName = roomList[x].Name;
            buttonComp.onClick.RemoveAllListeners();
            buttonComp.onClick.AddListener(() => { OnJoinRoomButton(roomName); });
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> allRooms)
    {
        roomList = allRooms;
    }



    #endregion

}
