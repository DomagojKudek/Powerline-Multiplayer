using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonRoom : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    public MapGenerator mapGenerator;//GET COMPONENT IZ OBJEKTA MAP


    public static PhotonRoom room;
    private PhotonView PV;

    public bool isGameLoaded;
    public int currentScene;

    public int trigger = 0;

    private Player[] photonPlayers;
    public int playersInRoom;
    public int myNumberInRoom;

    public int playersInGame;



    private bool readyToWaitForPlayers;
    private bool readyToStartGame;
    public float WaitTimeForPlayers;
    private float currentWaitTimeForPlayers;
    private float currentWaitTimeToStartGameWhenRoomIsFull;
    private float timeToStart;


    private void Awake()
    {


        if (PhotonRoom.room == null)
        {
            PhotonRoom.room = this;
        }
        else
        {
            if (PhotonRoom.room != this)
            {
                Destroy(PhotonRoom.room.gameObject);
                PhotonRoom.room = this;
            }
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnSceneFinishedLoading;
    }

    private void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.buildIndex;
        if (currentScene == MultiplayerSettings.multiplayerSettings.multiplayerScene)
        {
            isGameLoaded = true;
            if (MultiplayerSettings.multiplayerSettings.delayStart)
            {
                PV.RPC("RPC_LoadedGameScene", RpcTarget.MasterClient);
            }
            else
            {
                StartCoroutine(RPC_CreatePLayer());
            }
        }
    }

    void start()
    {
        PV = GetComponent<PhotonView>();
        readyToWaitForPlayers = false;
        readyToStartGame = false;
        currentWaitTimeForPlayers = WaitTimeForPlayers;
        currentWaitTimeToStartGameWhenRoomIsFull = 6;
        timeToStart = WaitTimeForPlayers;
    }


    void Update()
    {
        if (playersInRoom == 2 && trigger==0)
        {
            trigger = 1;
            finalize();
        }
        if (MultiplayerSettings.multiplayerSettings.delayStart)
        {
            if (playersInRoom == 1)
            {
                RestartTimer();
            }
            if (!isGameLoaded)
            {
                if (readyToStartGame)
                {
                    currentWaitTimeToStartGameWhenRoomIsFull -= Time.deltaTime;
                    currentWaitTimeForPlayers = currentWaitTimeToStartGameWhenRoomIsFull;
                    timeToStart = currentWaitTimeToStartGameWhenRoomIsFull;
                }
                else if (readyToWaitForPlayers)
                {
                    currentWaitTimeForPlayers -= Time.deltaTime;
                    timeToStart = currentWaitTimeForPlayers;
                }
                Debug.Log("Display time to start to the players" + timeToStart);
                if (timeToStart <= 0)
                {
                    StartGame();
                }
            }
        }
    }

    void RestartTimer()
    {
        currentWaitTimeForPlayers = WaitTimeForPlayers;
        timeToStart = WaitTimeForPlayers;
        currentWaitTimeToStartGameWhenRoomIsFull = 6;
        readyToWaitForPlayers = false;
        readyToStartGame = false;
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("player joined room");
        photonPlayers = PhotonNetwork.PlayerList;
        playersInRoom = photonPlayers.Length;
        myNumberInRoom = playersInRoom;
        PhotonNetwork.NickName = myNumberInRoom.ToString();
        if (MultiplayerSettings.multiplayerSettings.delayStart)
        {
            Debug.Log("Displayer players in room out of max players possible (" + playersInRoom + ":" + MultiplayerSettings.multiplayerSettings.maxPlayers + ")");
            if (playersInRoom > 1)
            {
                readyToWaitForPlayers = true;
            }
            if (playersInRoom == MultiplayerSettings.multiplayerSettings.maxPlayers)
            {
                readyToStartGame = true;
                if (!PhotonNetwork.IsMasterClient)
                    return;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }
        else
        {
            StartGame();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("A new player has joined the room");
        photonPlayers = PhotonNetwork.PlayerList;
        playersInRoom++;
        if (MultiplayerSettings.multiplayerSettings.delayStart)
        {
            Debug.Log("Displayer players in room out of max players possible (" + playersInRoom + ":" + MultiplayerSettings.multiplayerSettings.maxPlayers + ")");
            if (playersInRoom > 1)
            {
                readyToWaitForPlayers = true;
            }
            if (playersInRoom == MultiplayerSettings.multiplayerSettings.maxPlayers)
            {
                readyToStartGame = true;
                if (!PhotonNetwork.IsMasterClient)
                    return;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }
    }

      void StartGame()
    {
        Debug.Log("START GAME");
        //isGameLoaded = true;
        if (!PhotonNetwork.IsMasterClient)
            return;
        if (MultiplayerSettings.multiplayerSettings.delayStart)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
        PhotonNetwork.LoadLevel(MultiplayerSettings.multiplayerSettings.multiplayerScene);
        //setObjectsPlayer1();// SAMO ZA PROBU
    }

    [PunRPC]
    private void RPC_LoadedGameScene()
    {
        playersInGame++;
        if (playersInGame == PhotonNetwork.PlayerList.Length)
        {
            PV.RPC("RPC_CreatePlayer", RpcTarget.All);
        }
    }


    IEnumerator RPC_CreatePLayer()
    {
        yield return new WaitForSecondsRealtime(1);
        RPC_CreatePLayer2();
    }

    [PunRPC]
    private void RPC_CreatePLayer2()
    {
        //STVARANJE PLAYERA
        GameObject myPlayer = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonNetworkPlayer"), transform.position, Quaternion.identity, 0);
        //DOHVAT MAPGENERATOR
        while (mapGenerator == null)
        {

            GameObject search = GameObject.FindGameObjectWithTag("searching");
            mapGenerator = search.GetComponent<MapGenerator>();
        }


        for (int i = 0; i < mapGenerator.mapSize.x; i += 1)
        {
            if (i % 2 == 0)
            {
                for (int j = 0; j < mapGenerator.mapSize.y; j += 2)
                {
                    //Debug.Log(mapGenerator.blockMatrix[i, j].physicalBlock)/*.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[0])*/;
                    //myPlayer.GetComponent<PhotonView>().Owner
                    if (mapGenerator.blockMatrix[i, j].Rotator == true && mapGenerator.blockMatrix[i, j] != null)
                    {
                        mapGenerator.blockMatrix[i, j].physicalBlock.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[0]);
                    }
                }
            }
            else if (i % 2 != 0)
            {
                for (int j = 1; j < mapGenerator.mapSize.y; j += 2)
                {
                    if (mapGenerator.blockMatrix[i, j].Rotator == true && mapGenerator.blockMatrix[i, j] != null)
                    {
                        mapGenerator.blockMatrix[i, j].physicalBlock.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[0]);
                    }
                }
            }
        }
        
    }


    void finalize()
    {
        for (int i = 0; i < mapGenerator.mapSize.x; i += 1)
        {
            if (i % 2 == 0)
            {
                for (int j = 1; j < mapGenerator.mapSize.y; j += 2)
                {
                    if (mapGenerator.blockMatrix[i, j].Rotator == true && mapGenerator.blockMatrix[i, j] != null)
                    {
                        mapGenerator.blockMatrix[i, j].physicalBlock.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[1]);
                    }
                }
            }
            else if (i % 2 != 0)
            {
                for (int j = 0; j < mapGenerator.mapSize.y; j += 2)
                {
                    if (mapGenerator.blockMatrix[i, j].Rotator == true && mapGenerator.blockMatrix[i, j] != null)
                    {
                        mapGenerator.blockMatrix[i, j].physicalBlock.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.PlayerList[1]);
                    }
                }
            }
        }
    }
}