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
        string timeText = "Conquer the World in " + endTime ;
        Debug.Log(timeText) ;
        text.text =   timeText ;
        GameCanva.SetActive(false);
    }
    public void ExitButton() {
        SceneManager.LoadScene("Assets/Scenes/MainMenu2.unity") ;
    }
}
