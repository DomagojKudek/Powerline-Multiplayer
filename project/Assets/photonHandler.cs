using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class photonHandler : MonoBehaviour {

	public photonButtons photonB;
	public GameObject mainPlayer;

	private void Awake(){
		DontDestroyOnLoad (this.transform);

		PhotonNetwork.SendRate = 30;
       // PhotonNetwork.sendRateOnSerialize = 20;

		SceneManager.sceneLoaded += OnSceneFinishedLoading;
	}

	public void moveScene(){
		PhotonNetwork.LoadLevel("MapScene");
	}


	public void createNewRoom(){
		PhotonNetwork.CreateRoom(photonB.createRoomInput.text, new RoomOptions() { MaxPlayers = 20}, null);
	}


	public void joinOrCreateRoom(){
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers = 20;
		PhotonNetwork.JoinOrCreateRoom(photonB.joinRoomInput.text, roomOptions, TypedLobby.Default);

	}


	private void OnJoinedRoom(){
		moveScene();
		Debug.Log ("Connected to the room");
	}


	private void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode){

		if(scene.name == "SampleScene"){
			spawnPlayer();
		}
	}

	private void spawnPlayer(){
		PhotonNetwork.Instantiate(mainPlayer.name, mainPlayer.transform.position, mainPlayer.transform.rotation,0);
	}

}
