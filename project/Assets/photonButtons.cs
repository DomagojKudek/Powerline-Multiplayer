using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class photonButtons : MonoBehaviour {

	public photonHandler pHandler;
	public InputField createRoomInput, joinRoomInput;



	public void OnClickCreateRoom(){
		pHandler.createNewRoom();
	
	}


	public void OnClickJoinRoom(){
		pHandler.joinOrCreateRoom();
	}




}
