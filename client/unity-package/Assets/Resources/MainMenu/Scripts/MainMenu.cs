using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void PlayButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Login");
    }
}
