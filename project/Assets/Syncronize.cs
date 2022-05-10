using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class Syncronize : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }


    void setObjectsPlayer1()
    {
        if (PhotonNetwork.PlayerList.Length == 1)
        {
            Debug.LogWarning("IGRA IMA SAMO JEDNOG IGRACA");
        }
    }


}


