using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndGamePanel : MonoBehaviour
{
    public GameObject GameCanva;
    public Text text;  
    // Start is called before the first frame update
    public void EndGame(string endTime) {

        gameObject.SetActive(true);
        Debug.Log(endTime);
        string userDataJson = JsonUtility.ToJson(UserData.GetGameStat());
        StartCoroutine(GameServerApi.SyncData(UserData.username, userDataJson, UserData.GetAccessToken()));

        text.text =   endTime;
        GameCanva.SetActive(false);
    }
    public void ExitButton() {
        SceneManager.LoadScene("Assets/Scenes/MainMenu2.unity") ;
    }
}
