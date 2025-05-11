using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logout : MonoBehaviour
{
    // Start is called before the first frame update
    public GameManager gameManager;

    public void UserLogout() {
        StartCoroutine(GameServerApi.Logout(
            UserData.username, UserData.GetRefreshToken(),
            (GameServerApi.ServerResponse resobj, bool result) => {
                UserData.SetRefreshToken("");
                UserData.SetAccessToken("");
                UserData.username = null;
                UserData.ResetGameStat();
                gameManager.SwitchWelcomePanel();
            }
        ));
    }
}
