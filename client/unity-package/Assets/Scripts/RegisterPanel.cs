using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class RegisterPanel : MonoBehaviour
{
    private GameManager _gameManger;
    public TMP_InputField Email, VeriCode, Username, Pw, ConfirmPw;
    public TextMeshProUGUI InvalidEmailFormatMsg, EmailSentMsg, CodeInvalidMsg, UsernameUsedMsg, UsernameInvalidFormatMsg, PwInvalidMsg, ConfirmPwInvalidMsg;

    //bool RegEmailFlag, RegUsernameFlag, RegPasswordFlag, VeriCodeFlag;
    // Start is called before the first frame update
    void Start()
    {
        _gameManger = GameObject.Find("Game Manager").GetComponent<GameManager>();
        Email.onEndEdit.AddListener(CheckEmail);
        VeriCode.onEndEdit.AddListener(CheckVeriCode);
        Username.onEndEdit.AddListener(CheckUsername);
        Pw.onEndEdit.AddListener(CheckPw);
        ConfirmPw.onEndEdit.AddListener(CheckConfirmPw);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CheckEmail(string Email)
    {
        _gameManger.CheckRegEmail(Email, InvalidEmailFormatMsg);
    }

    private void CheckVeriCode(string VeriCode)
    {
        _gameManger.CheckVeriCode(VeriCode, CodeInvalidMsg);
    }

    private void CheckUsername(string Username)
    {
        _gameManger.CheckRegUsername(Username, UsernameUsedMsg, UsernameInvalidFormatMsg);
    }

    private void CheckPw(string Pw)
    {
        _gameManger.CheckRegPassword(Pw, PwInvalidMsg);
    }

    private void CheckConfirmPw(string ConfirmPw)
    {
        _gameManger.CheckRegConfirmPw(Pw.text, ConfirmPw, ConfirmPwInvalidMsg);
    }
}
