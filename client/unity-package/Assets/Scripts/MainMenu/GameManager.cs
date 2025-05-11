using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class GameManager : MonoBehaviour
{
    public GameObject WelcomePanel, ForgetPasswordPanel, ActivationPanel, MainMenu, Settings, SoundPanel, VideoPanel, UserProfilePanel;
    public RegisterPanel RegisterPanel;
    public TMP_InputField LoginName, LoginPassword, ResetEmail;
    public TextMeshProUGUI 
        InvalidUserOrPWMessage, ServerErrorMsg, 
        ResetLinkMessage, ResetFailMessage, 
        ID, SettingsUsername;
    private bool _loginFlag = false;

    public string gameStageSceneName = "gameStageScene2";

    // Start is called before the first frame update
    void Start()
    {
        // ResetEmail.onEndEdit.AddListener(ForgetPassword);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SwitchWelcomePanel()
    {
        WelcomePanel.SetActive(false);
        ForgetPasswordPanel.SetActive(false);
        ActivationPanel.SetActive(false);
        MainMenu.SetActive(false);
        if (UserData.GetRefreshToken()?.Length > 0)
        {
            if (UserData.Activated)
            {
                ID.text = "Welcome, \n" + UserData.username;
                MainMenu.gameObject.SetActive(true);
            }
            else
            {
                ActivationPanel.SetActive(true);
            }
        }
        else {
            WelcomePanel.SetActive(true);
        }
    }

    public void SwitchMainMenu()
    {
        if (_loginFlag)
        {
            WelcomePanel.gameObject.SetActive(false);
            ActivationPanel.gameObject.SetActive(false);
            MainMenu.gameObject.SetActive(false);

            if (UserData.Activated)
            {
                ID.text = "Welcome, \n" + UserData.username;
                MainMenu.gameObject.SetActive(true);
            }
            else {
                ActivationPanel.SetActive(true);
            }
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
        SceneManager.LoadScene(sceneName: gameStageSceneName);
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
    public void Login()
    {
        ServerErrorMsg.gameObject.SetActive(false);
        string username = LoginName.text;
        string password = LoginPassword.text;

        string validUsernameFormat = @"^[0-9a-zA-Z._()]{4,20}$"; // chagne ValidFormat to put acceptable characters
        bool isValidUsername = Regex.IsMatch(username, validUsernameFormat);
        string validPWFormat = @"^(?=.*[A-Z])[A-Za-z0-9]{8,16}$";
        bool isValidPW = Regex.IsMatch(password, validPWFormat);

        if (!isValidUsername || !isValidPW) {
            InvalidUserOrPWMessage.gameObject.SetActive(true);
            return;
        }
        InvalidUserOrPWMessage.gameObject.SetActive(false);

        StartCoroutine(GameServerApi.Login(
                username,
                password,
                (GameServerApi.ServerResponse resobj, bool result) =>
                {
                    Debug.Log("Login Callback: " + resobj?.message);
                    if (result)
                    {
                        UserData.username = resobj.username;
                        UserData.SetAccessToken(resobj.accessToken);
                        UserData.SetRefreshToken(resobj.refreshToken);
                        if (resobj.activated.Equals("true")) {
                            UserData.Activate();
                        }
                        _loginFlag = true;
                        SwitchMainMenu();
                    }
                    else
                    {
                        ServerErrorMsg.SetText(resobj?.message);
                        ServerErrorMsg.gameObject.SetActive(true);
                        _loginFlag = false;
                    }
                }
            ));
        Debug.Log("LoginID text: " + LoginName.text);
        Debug.Log("Login Button Pressed");

        
        return;
    }
    /*
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
    }*/

    public void Back()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
