using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    GameObject MainMenu;
    [SerializeField]
    GameObject Searching;

    void Start()
    {
        Debug.Log("Start running from LobbyManager");
        Searching.SetActive(false);
        MainMenu.SetActive(false);
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Start finished from LobbyManager");
    }

    //On connection to server reveals the play button to allow players to search for matches
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to photon on " + PhotonNetwork.CloudRegion + " server.");
        PhotonNetwork.AutomaticallySyncScene = true;
        MainMenu.SetActive(true);
    }

    public void FindMatch()
    {
        Searching.SetActive(true);
        MainMenu.SetActive(false);
        PhotonNetwork.JoinRandomRoom();
        Debug.Log("Searching for a game");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Couldn't find room. Creating own room");
        MakeRoom();
    }

    //Method to create a room
    public void MakeRoom()
    {
        int randomRoomName = Random.Range(0, 5000);

        //Makes a room for two players that can be joined by anyone
        RoomOptions roomOptions =
        new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 2
        };
        PhotonNetwork.CreateRoom("RoomName_" + randomRoomName, roomOptions);
        Debug.Log("Room created. Waiting for another player...");
    }

    //Starts the match once another player enters the room
    public override void OnPlayerEnteredRoom(Player secondPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Starting match");
            PhotonNetwork.LoadLevel(1);
        }
    }

    public void StopSearch()
    {
        Searching.SetActive(false);
        MainMenu.SetActive(true);
        PhotonNetwork.LeaveRoom();
        Debug.Log("Search Stopped");
    }
}
