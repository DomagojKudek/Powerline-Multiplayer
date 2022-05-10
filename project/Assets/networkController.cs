using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class networkController : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); //Photon master server   
    }


    public override void OnConnectedToMaster()
    {
        Debug.Log("konekcija je uspostavljena");
    }


}
