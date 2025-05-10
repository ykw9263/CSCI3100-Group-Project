using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class RegisterPanel : MonoBehaviour
{
    private GameManager _gameManger;
    public TMP_InputField 
        EmailField, 
        UsernameField, 
        PwField, ConfirmPwField, 
        VeriCodeField;

    public TextMeshProUGUI 
        InvalidEmailFormatMsg, 
        UsernameInvalidFormatMsg, 
        PwInvalidMsg, ConfirmPwInvalidMsg,
        EmailSentMsg, CodeInvalidMsg, SuccessMsg, ServerErrorMsg;


    protected bool _regEmailFlag, _veriCodeFlag, _regUsernameFlag, _regPwFlag, _regConfirmPwFlag;

    //bool RegEmailFlag, RegUsernameFlag, RegPasswordFlag, VeriCodeFlag;
    // Start is called before the first frame update
    void Start()
    {
        _gameManger = GameObject.Find("Game Manager").GetComponent<GameManager>();
        EmailField.onEndEdit.AddListener(CheckEmail);
        UsernameField.onEndEdit.AddListener(CheckUsername);
        PwField.onEndEdit.AddListener(CheckPw);
        ConfirmPwField.onEndEdit.AddListener(CheckConfirmPw);
        VeriCodeField.onEndEdit.AddListener(CheckVeriCode);
    }

    protected void ClearMessages()
    {
        EmailSentMsg.gameObject.SetActive(false);
        CodeInvalidMsg.gameObject.SetActive(false);
        SuccessMsg.gameObject.SetActive(false);
        ServerErrorMsg.gameObject.SetActive(false);
    }

    public void SendCode() {
        ClearMessages();
        if (!_regUsernameFlag || !_regEmailFlag) { return; }
        Debug.Log("sending VerifyCode: ");

        StartCoroutine(GameServerApi.VerifyEmail(UsernameField.text, EmailField.text,
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

    protected void CheckEmail(string email)
    {
        ClearMessages();
        string ValidPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        bool IsValid = Regex.IsMatch(email, ValidPattern);
        if (IsValid)
        {
            InvalidEmailFormatMsg.gameObject.SetActive(false);
            _regEmailFlag = true;
        }
        else
        {
            InvalidEmailFormatMsg.gameObject.SetActive(true);
            _regEmailFlag = false;
        }
    }

    protected void CheckVeriCode(string veriCode)
    {
        ClearMessages();
        Debug.Log("Checking VerifyCode: ");
        if (veriCode.Length != 6) {
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

                    Register();
                }
                else
                {
                    CodeInvalidMsg.gameObject.SetActive(true);
                    _veriCodeFlag = false;
                }
            }
        ));
    }

    protected void CheckUsername(string username)
    {
        ClearMessages();
        string ValidFormat = @"^[0-9a-zA-Z._()]{4,20}$"; // chagne ValidFormat to put acceptable characters
        bool IsValid = Regex.IsMatch(username, ValidFormat);

        if (!IsValid)
        {
            UsernameInvalidFormatMsg.gameObject.SetActive(true);
            _regUsernameFlag = false;
        }
        else
        {
            UsernameInvalidFormatMsg.gameObject.SetActive(false);
            _regUsernameFlag = true;
        }
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
        if (PwField.text.Equals(ConfirmPw))
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



    private void Register()
    {
        ServerErrorMsg.gameObject.SetActive(false);
        Debug.Log("_regEmailFlag, _veriCodeFlag, _regUsernameFlag, _regPwFlag, _regConfirmPwFlag: " + _regEmailFlag + _veriCodeFlag + _regUsernameFlag + _regPwFlag + _regConfirmPwFlag);
        if (_regEmailFlag == true && _veriCodeFlag == true && _regUsernameFlag == true && _regPwFlag == true && _regConfirmPwFlag == true)
        {
            StartCoroutine(GameServerApi.Register(
                UsernameField.text,
                PwField.text,
                EmailField.text,
                UserData.GetAccessToken(),
                (GameServerApi.ServerResponse resobj, bool result) =>
                {
                    Debug.Log("Register Callback: " + resobj?.message);
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
