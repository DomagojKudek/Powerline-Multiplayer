using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class rotation : MonoBehaviour
{

    public MapGenerator mapGenerator;

    void Start()
    {
        if (mapGenerator == null)
        {
            mapGenerator = GetComponentInParent<MapGenerator>();
        }
    }

    void Update()
    {
        if (checkWinCondition() == mapGenerator.winMap.Length)
        {
            FindObjectOfType<AudioManager>().TurnOff("backgroundMusic");
            mapGenerator.youWin = true;
            Debug.LogWarning("-----YOU WIN!-----");
            FindObjectOfType<AudioManager>().Play("winwin");
        }
    }


    void OnMouseDown()
    {
        if (GetComponent<PhotonView>().IsMine==true)
        {
            transform.Rotate(0, 90, 0);
            FindObjectOfType<AudioManager>().Play("rotateSound");
            checkWinCondition();
        }
    }


    int checkWinCondition()
    {
        /*Debug.Log("Winpath = " + string.Join(" ",
         new List<int>(mapGenerator.winMap)
        .ConvertAll(i => i.ToString())
         .ToArray()));*/
        int checkCounter = 0;
        for (int i = 0; i < mapGenerator.winMap.Length; i++)
        {
            if (mapGenerator.winMap[i] == 2)
            {
                checkCounter++;
                //Debug.Log(checkCounter);
            }

        }

        return checkCounter;
    }
}
