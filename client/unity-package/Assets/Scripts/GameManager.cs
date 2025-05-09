using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Net;

public class GameManager : MonoBehaviour
{
    public GameObject WelcomePanel, ForgetPasswordPanel, RegisterPanel, MainMenu, Settings, SoundPanel, VideoPanel, UserProfilePanel;
    public TMP_InputField LoginName, LoginPassword, ResetEmail;
    public TextMeshProUGUI InvalidNameMessage, InvalidPasswordMessage, ResetLinkMessage, ResetFailMessage, RegSuccessMessage, ID, SettingsUsername;
    private bool _regEmailFlag, _veriCodeFlag, _regUsernameFlag, _regPwFlag, _regConfirmPwFlag;

    // Start is called before the first frame update
    void Start()
    {
        ResetEmail.onEndEdit.AddListener(ForgetPassword);
        _regEmailFlag = _regUsernameFlag = _regPwFlag = _veriCodeFlag = _regConfirmPwFlag = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchMainMenu()
    {
        if (Login())
        {
            ID.text = "Welcome, \n" + LoginName.text;
            WelcomePanel.gameObject.SetActive(false);
            MainMenu.gameObject.SetActive(true);
        }
    }

    public void SwitchForgetPassword()
    {
        WelcomePanel.gameObject.SetActive(false);
        ForgetPasswordPanel.gameObject.SetActive(true);
    }

    public void SwitchRegister()
    {
        WelcomePanel.gameObject.SetActive(false);
        RegisterPanel.gameObject.SetActive(true);
    }

    public void Play()
    {
        //Load Game Scene
        return;
    }

    public void SwitchSettings()
    {
        SettingsUsername.text = LoginName.text;
        MainMenu.gameObject.SetActive(false);
        Settings.gameObject.SetActive(true);
    }

    public void SettingsSwitchMM()
    {
        Settings.gameObject.SetActive(false);
        MainMenu.gameObject.SetActive(true);
    }

    public void SwitchSound()
    {
        SoundPanel.gameObject.SetActive(true);
        VideoPanel.gameObject.SetActive(false);
        UserProfilePanel.gameObject.SetActive(false);
    }

    public void SwitchVideo()
    {
        SoundPanel.gameObject.SetActive(false);
        VideoPanel.gameObject.SetActive(true);
        UserProfilePanel.gameObject.SetActive(false);
    }

    public void SwitchUserProfile()
    {
        SoundPanel.gameObject.SetActive(false);
        VideoPanel.gameObject.SetActive(false);
        UserProfilePanel.gameObject.SetActive(true);
    }
    public void Exit()
    {
        Application.Quit();
    }
    public bool Login()
    {
        bool LoginFlag = false;
        if (LoginName.text.Equals("wrongID"))
        {
            InvalidNameMessage.gameObject.SetActive(true);
        }else if (LoginPassword.text.Equals("wrongpassword"))
        {
            InvalidNameMessage.gameObject.SetActive(false);
            InvalidPasswordMessage.gameObject.SetActive(true);
        }
        else
        {
            LoginFlag = true;
        }
        Debug.Log("LoginID text: " + LoginName.text);
        Debug.Log("Login Button Pressed");
        return LoginFlag;
    }
    
    public void ForgetPassword(string ResetEmail)
    {
        if (ResetEmail.Equals("sent"))
        {
            ResetLinkMessage.gameObject.SetActive(true);
            ResetFailMessage.gameObject.SetActive(false);
        }
        else
        {
            ResetLinkMessage.gameObject.SetActive(false);
            ResetFailMessage.gameObject.SetActive(true);
        }
        Debug.Log("Input text: " + ResetEmail);
    }

    public void Register()
    {
        Debug.Log("_regEmailFlag, _veriCodeFlag, _regUsernameFlag, _regPwFlag, _regConfirmPwFlag: " + _regEmailFlag + _veriCodeFlag + _regUsernameFlag + _regPwFlag + _regConfirmPwFlag);
        if(_regEmailFlag == true && _veriCodeFlag == true && _regUsernameFlag == true && _regPwFlag == true && _regConfirmPwFlag == true)
        {
            //register account to database and send successful message
            RegSuccessMessage.gameObject.SetActive(true);
        }
        else
        {
            RegSuccessMessage.gameObject.SetActive(false);
        }
    }

    public void CheckRegEmail(string Email, TextMeshProUGUI InvalidEmailFormatMsg)
    {
        string ValidPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        bool IsValid = Regex.IsMatch(Email, ValidPattern);
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
        Register();
    }
    public void CheckVeriCode(string VeriCode, TextMeshProUGUI CodeInvalidMsg)
    {
        if (VeriCode.Equals("true")) // change condition to match email veri code
        {
            CodeInvalidMsg.gameObject.SetActive(false);
            _veriCodeFlag = true;
        }
        else
        {
            CodeInvalidMsg.gameObject.SetActive(true);
            _veriCodeFlag = false;
        }
        Register();
    }
    public void CheckRegUsername(string Username, TextMeshProUGUI UsernameUsedMsg, TextMeshProUGUI UsernameInvalidFormatMsg)
    {
        string ValidFormat = @"^[0-9a-zA-Z._()]{4,20}$"; // chagne ValidFormat to put acceptable characters
        bool IsUsed = Username.Equals("used");
        bool IsValid = Regex.IsMatch(Username, ValidFormat);
        if (IsUsed) // chagne condition
        {
            UsernameUsedMsg.gameObject.SetActive(true);
            UsernameInvalidFormatMsg.gameObject.SetActive(false);
            _regUsernameFlag = false;
        }else if (!IsValid)
        {
            UsernameUsedMsg.gameObject.SetActive(false);
            UsernameInvalidFormatMsg.gameObject.SetActive(true);
            _regUsernameFlag = false;
        }
        else
        {
            UsernameUsedMsg.gameObject.SetActive(false);
            UsernameInvalidFormatMsg.gameObject.SetActive(false);
            _regUsernameFlag = true;
        }
        Register();
    }

    public void CheckRegPassword(string Pw, TextMeshProUGUI PwInvalidMsg)
    {
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
        Register();
    }

    public void CheckRegConfirmPw(string Pw, string ConfirmPw, TextMeshProUGUI ConfirmPwInvalidMsg)
    {
        if (Pw.Equals(ConfirmPw))
        {
            ConfirmPwInvalidMsg.gameObject.SetActive(false);
            _regConfirmPwFlag = true;
        }
        else
        {
            ConfirmPwInvalidMsg.gameObject.SetActive(true);
            _regConfirmPwFlag = false;
        }
        Register();
    }

    public void Back()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
