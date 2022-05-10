using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class menuLogic : MonoBehaviour {


	public GameObject connectedMenu;

	public void disableMenuUI(){
		PhotonNetwork.LoadLevel("MapScene");
	}


}
