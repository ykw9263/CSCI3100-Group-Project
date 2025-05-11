using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ActivationPanel : MonoBehaviour
{
    public TMP_InputField LicenseKeyField;
    private GameManager gameManager;

    public TextMeshProUGUI ServerErrorMsg;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    public void CheckLicenseKey(){
        StartCoroutine(GameServerApi.Activate(UserData.username, LicenseKeyField.text, UserData.GetAccessToken(),
            (GameServerApi.ServerResponse resobj, bool result) => {
                Debug.Log("VerifyCode Callback: " + resobj?.message);
                if (result)
                {
                    UserData.Activate();
                    gameManager.SwitchMainMenu();
                }
                else
                {
                    ServerErrorMsg.SetText(resobj?.message);
                    ServerErrorMsg.gameObject.SetActive(true);
                }
            }
        ));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
