using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Countdown : MonoBehaviour
{


    public MapGenerator mapGenerator;
    [SerializeField]
    private Text uiText;
    [SerializeField]
    private Button uiGameOver;
    [SerializeField]
    private Button uiYouWin;
    [SerializeField]
    private float mainTimer;

    private float timer;
    private bool canCount = true;
    private bool doOnce = false;



    void Start()
    {
        if (mapGenerator == null)
        {
            mapGenerator = GetComponentInParent<MapGenerator>();
        }
        timer = mainTimer;
    }

    void Update()
    {

        if (mapGenerator.youWin)
        {
            uiYouWin.gameObject.SetActive(true);
            uiText.transform.gameObject.SetActive(false);
        }
        if (timer > 0.0f && canCount)
        {
            timer -= Time.deltaTime / 10;
            uiText.text = timer.ToString("F");
        }
        else if (timer <= 0.0f && !doOnce)
        {

            if (!mapGenerator.youWin)
            {
                FindObjectOfType<AudioManager>().TurnOff("backgroundMusic");
                FindObjectOfType<AudioManager>().Play("gameOverSound");
                uiGameOver.gameObject.SetActive(true);
                uiText.transform.gameObject.SetActive(false);
            }
            canCount = false;
            doOnce = true;
            uiText.text = "0.00";
            timer = 0.0f;

        }
    }

    public void ResetBtn()
    {
        timer = mainTimer;
        canCount = true;
        doOnce = false;
    }

}
