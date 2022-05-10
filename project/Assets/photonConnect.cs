using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class photonConnect : MonoBehaviour {


	public string versionName = "0.1";
	public GameObject View1, View2, View3;

	private void Awake(){
        PhotonNetwork.ConnectUsingSettings();

		Debug.Log("Connecting to photon...");
	}


	private void OnConnectedToMaster(){
		PhotonNetwork.JoinLobby(TypedLobby.Default);

		Debug.Log("We are connected to master");

	}

	private void OnJoinedLobby(){
		View1.SetActive(false);
		View2.SetActive(true);

		Debug.Log("On joined lobby");
	}

	private void OnDisconnectedFromPhoton(){
		if(View1.active)
			View1.SetActive(false);

		if(View2.active)
			View2.SetActive(false);

		View3.SetActive(true);

		Debug.Log("Disconnected from server lobby");
	}

	private void OnFailedToConnectToPhoton(){

		Debug.Log("No internet connection");
	}















	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
