using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndGamePanel : MonoBehaviour
{
    public GameObject GameCanva, UICanva;
    public Text text, winText;  
    // Start is called before the first frame update
    public void EndGame(string endTime, string winMessage = "You Win!") {

        winText.text = winMessage;
        gameObject.SetActive(true);
        Debug.Log(endTime);
        string userDataJson = JsonUtility.ToJson(UserData.GetGameStat());
        StartCoroutine(GameServerApi.SyncData(UserData.username, userDataJson, UserData.GetAccessToken()));

        text.text =   endTime;
        GameCanva.SetActive(false);
        UICanva.SetActive(false);
    }
    public void ExitButton() {
        SceneManager.LoadScene(sceneName: "MainMenu");
    }
}
