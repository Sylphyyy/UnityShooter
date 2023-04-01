using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using TMPro;
using Photon.Realtime;
using Random = UnityEngine.Random;
using UnityEngine.UIElements;
using System.Linq;

public class Launcher : MonoBehaviourPunCallbacks
{


    public static Launcher Instance;
    List<RoomInfo> fullRoomList = new List<RoomInfo>();
    List<RoomListItem> roomListItems = new List<RoomListItem>();

    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] Transform PlayerListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] GameObject PlayerListItemPrefab;
    [SerializeField] GameObject startGameButton;

    private void Awake()
    {
        Instance = this;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // Тут короче коннект к серверу фотона организован

    // Коннектимся к серверу используя пареметры настроеные в файле Assets>Photon>PhotonUnityNetworking>Resources>PhotonServerSettings
    void Start()
    {
        Debug.Log("Connecting to Master");
        PhotonNetwork.ConnectUsingSettings();
    }

    // Подключаемся к лобби
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Проверяем подключение 
    public override void OnJoinedLobby()
    {
        MenuManager.Instance.OpenMenu("title");
        Debug.Log("Joined lobby");
    }

    // Создаем комнатку
    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
        PhotonNetwork.CreateRoom(roomNameInputField.text);
        MenuManager.Instance.OpenMenu("loading");
    }

    // Удалось создать и подключиться, чек на создателя
    public override void OnJoinedRoom()
    {
        MenuManager.Instance.OpenMenu("room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        Player[] players = PhotonNetwork.PlayerList;

        foreach(Transform child in PlayerListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Count(); i++)
        {
            Instantiate(PlayerListItemPrefab, PlayerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    // Если создатель вышел, то передаем права другому юзеруууу
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    // Лох, нихуя не удалось
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        MenuManager.Instance.OpenMenu("error");
    }

    // Выход из комнаты
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("loading");
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("tile");
    }

    // Создаем инстансы
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        RoomInfo newRoom = null;
        foreach (RoomInfo updatedRoom in roomList)
        {
            RoomInfo existingRoom = fullRoomList.Find(x => x.Name.Equals(updatedRoom.Name)); // Check to see if we have that room already
            if (existingRoom == null) // WE DO NOT HAVE IT
            {
                fullRoomList.Add(updatedRoom); // Add the room to the full room list
                if (newRoom == null)
                {
                    newRoom = updatedRoom;
                }
            }
            else if (updatedRoom.RemovedFromList || updatedRoom.PlayerCount == 0) // WE DO HAVE IT, so check if it has been removed
            {
                fullRoomList.Remove(existingRoom); // Remove it from our full room list
            }
        }
        RenderRoomList();

        if (newRoom != null && !PhotonNetwork.InRoom)
        {
            newRoom.CustomProperties.TryGetValue("ver", out object version);
            if (version != null && (string)version == Application.version)
            {
                JoinRoom(newRoom);
            }
        }
    }

    void RenderRoomList()
    {
        RemoveRoomList();
        foreach (RoomInfo roomInfo in fullRoomList)
        {
            if (roomInfo.PlayerCount == 0 || roomInfo.RemovedFromList)
                continue;
            RoomListItem roomListItem = Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>();
            roomListItem.SetUp(roomInfo);
            roomListItems.Add(roomListItem);
        }
    }

    void RemoveRoomList()
    {
        foreach (RoomListItem roomListItem in roomListItems)
        {
            Destroy(roomListItem.gameObject);
        }
        roomListItems.Clear();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(PlayerListItemPrefab, PlayerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }

    // Стартуем каточку
    public void StartGame()
    {
        PhotonNetwork.LoadLevel(1);
    }

}
