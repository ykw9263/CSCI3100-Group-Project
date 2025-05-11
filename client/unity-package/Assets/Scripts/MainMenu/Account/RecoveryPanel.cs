using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class RecoveryPanel : RegisterPanel
{
    public GameObject requestPanel, resetPanel;

    void Start()
    {
        EmailField.onEndEdit.AddListener(CheckEmail);
        UsernameField.onEndEdit.AddListener(CheckUsername);
        PwField.onEndEdit.AddListener(CheckPw);
        ConfirmPwField.onEndEdit.AddListener(CheckConfirmPw);
        VeriCodeField.onEndEdit.AddListener(CheckVeriCode);
    }

    private void OnEnable()
    {
        requestPanel.SetActive(true);
        resetPanel.SetActive(false);
    }

    public new void SendCode()
    {
        ClearMessages();
        if (!_regUsernameFlag || !_regEmailFlag) { return; }
        Debug.Log("sending VerifyCode: ");

        StartCoroutine(GameServerApi.RequestRestore(UsernameField.text, EmailField.text,
            (GameServerApi.ServerResponse resobj, bool result) => {
                Debug.Log("VerifyCode Callback: " + resobj?.message);
                if (result)
                {
                    EmailSentMsg.gameObject.SetActive(true);
                }
                else
                {
                    ServerErrorMsg.SetText(resobj?.message);
                    ServerErrorMsg.gameObject.SetActive(true);
                }
            }
        ));
    }

    protected new void CheckVeriCode(string veriCode)
    {
        ClearMessages();
        Debug.Log("Checking VerifyCode: ");
        if (veriCode.Length != 6)
        {
            CodeInvalidMsg.gameObject.SetActive(true);
            return;
        }
        StartCoroutine(GameServerApi.VerifyCode(UsernameField.text, veriCode,
            (GameServerApi.ServerResponse resobj, bool result) => {
                Debug.Log("VerifyCode Callback: " + resobj?.message);
                if (result)
                {
                    UserData.SetAccessToken(resobj.accessToken);
                    _veriCodeFlag = true;
                    requestPanel.SetActive(false);
                    resetPanel.SetActive(true);
                }
                else
                {
                    CodeInvalidMsg.gameObject.SetActive(true);
                    _veriCodeFlag = false;
                }
            }
        ));
    }


    public void RestorePW() {
        ServerErrorMsg.gameObject.SetActive(false);
        if (_regPwFlag == true && _regConfirmPwFlag == true)
        {
            StartCoroutine(GameServerApi.FinishRestore(
                UsernameField.text,
                PwField.text,
                UserData.GetAccessToken(),
                (GameServerApi.ServerResponse resobj, bool result) =>
                {
                    Debug.Log("Recover Callback: " + resobj?.message);
                    if (result)
                    {
                        SuccessMsg.gameObject.SetActive(true);
                    }
                    else
                    {
                        ServerErrorMsg.SetText(resobj?.message);
                        ServerErrorMsg.gameObject.SetActive(true);
                        _veriCodeFlag = false;
                    }
                }
            ));
        }
        else
        {
            SuccessMsg.gameObject.SetActive(false);
        }
    }

}
