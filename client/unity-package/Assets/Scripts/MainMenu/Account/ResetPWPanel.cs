using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UnityEngine.Rendering.DebugUI;
using System.Text.RegularExpressions;

public class ResetPWPanel : MonoBehaviour
{
    public TMP_InputField CurPWField, NewPwField, ConfirmPwField;
    protected bool _regPwFlag, _regConfirmPwFlag;

    public TextMeshProUGUI
        PwInvalidMsg, ConfirmPwInvalidMsg,
        SuccessMsg, ServerErrorMsg;

    // Start is called before the first frame update
    void Start()
    {
        NewPwField.onEndEdit.AddListener(CheckPw);
        ConfirmPwField.onEndEdit.AddListener(CheckConfirmPw);
    }

    protected void ClearMessages()
    {
        SuccessMsg.gameObject.SetActive(false);
        ServerErrorMsg.gameObject.SetActive(false);
    }

    protected void CheckPw(string Pw)
    {
        ClearMessages();
        string ValidFormat = @"^(?=.*[A-Z])[A-Za-z0-9]{8,16}$";
        bool IsValid = Regex.IsMatch(Pw, ValidFormat);
        if (!IsValid)
        {
            PwInvalidMsg.gameObject.SetActive(true);
            _regPwFlag = false;
        }
        else
        {
            PwInvalidMsg.gameObject.SetActive(false);
            _regPwFlag = true;
        }
        _regConfirmPwFlag = false;
    }

    protected void CheckConfirmPw(string ConfirmPw)
    {
        ClearMessages();
        if (NewPwField.text.Equals(ConfirmPw))
        {
            ConfirmPwInvalidMsg.gameObject.SetActive(false);
            _regConfirmPwFlag = true;
        }
        else
        {
            ConfirmPwInvalidMsg.gameObject.SetActive(true);
            _regConfirmPwFlag = false;
        }
    }
    public void ResetPW() {
        ServerErrorMsg.gameObject.SetActive(false);
        if (_regPwFlag == true && _regConfirmPwFlag == true)
        {
            StartCoroutine(GameServerApi.ResetPW(
                UserData.username,
                CurPWField.text,
                NewPwField.text,
                UserData.GetAccessToken(),
                (GameServerApi.ServerResponse resobj, bool result) =>
                {
                    Debug.Log("Reset Callback: " + resobj?.message);
                    if (result)
                    {
                        SuccessMsg.gameObject.SetActive(true);
                    }
                    else
                    {
                        ServerErrorMsg.SetText(resobj?.message);
                        ServerErrorMsg.gameObject.SetActive(true);
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
