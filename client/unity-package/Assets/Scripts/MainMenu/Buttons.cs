using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buttons : MonoBehaviour
{
    private GameManager _gameManager;
    private Button button;
    private AudioManager _audioManager;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        _gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        _audioManager = GameObject.Find("Audio Manager").GetComponent<AudioManager>();
        button.onClick.AddListener(_panelSelect);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void _panelSelect()
    {
        _audioManager.PlaySFX(_audioManager.ButtonClick);
        switch (button.name)
        {
            case "Login Button":
                _gameManager.Login();
                break;
            case "Forget Password Button":
                _gameManager.SwitchForgetPassword();
                break;
            case "Register Button":
                _gameManager.SwitchRegister();
                break;
            case "Login Back Button":
                _gameManager.Back();
                break;
            case "Play Button":
                _gameManager.Play();
                break;
            case "Settings Button":
                _gameManager.SwitchSettings();
                break;
            case "Achievements Button":
                _gameManager.SwitchAchievements();
                break;
            case "Achievements Back MM Button":
                _gameManager.AchievementsSwitchMM();
                break;
            case "Exit Button":
                _gameManager.Exit();
                break;
            case "MM Back Button":
                _gameManager.SettingsSwitchMM();
                break;
            case "Sound Button":
                _gameManager.SwitchSound();
                break;
            case "Video Button":
                _gameManager.SwitchVideo();
                break;
            case "User Profile Button":
                _gameManager.SwitchUserProfile();
                break;
            case "ResetPW Button":
                _gameManager.SwitchResetPW();
                break;
            case "GamePlay Button":
                _gameManager.SwitchGamePlaySetting();
                break;
            default:
                break;
        }
    }
}
