using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.Audio;

public class Settings : MonoBehaviour
{
    [SerializeField] private AudioMixer Mixer;
    [SerializeField] private Slider MusicSlider;
    [SerializeField] private Slider SFXSlider;
    public TMP_Dropdown ResolutionDropdown;
    public TextMeshProUGUI UsernameUsedMsg, UsernameInvalidFormatMsg;
    public TMP_InputField NewUsername;
    Resolution[] Resolutions;

    private GameManager _gameManager;
    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();

        Resolutions = Screen.resolutions;
        ResolutionDropdown.ClearOptions();
        List<string> Options = new List<string>();
        int CurrentResolutionIndex = 0;
        for (int i = 0;i < Resolutions.Length; i++)
        {
            string Option = Resolutions[i].width + " x " + Resolutions[i].height;
            Options.Add(Option);
            if (Resolutions[i].width == Screen.currentResolution.width && Resolutions[i].height == Screen.currentResolution.width)
            {
                CurrentResolutionIndex = i;
            }
        }
        ResolutionDropdown.AddOptions(Options);
        ResolutionDropdown.value = CurrentResolutionIndex;
        ResolutionDropdown.RefreshShownValue();

        NewUsername.onEndEdit.AddListener(EditUsername);

        //_gameManager.Settings.gameObject.SetActive(true);
        //if (PlayerPrefs.HasKey("Music Volume") || PlayerPrefs.HasKey("SFX Volume"))
        //{
        //    MusicSlider.value = PlayerPrefs.GetFloat("Music Volume");
        //    SFXSlider.value = PlayerPrefs.GetFloat("SFX Volume");
        //}
        //SetMusicVolume();
        //SetSFXVolume();
        //_gameManager.Settings.gameObject.SetActive(false);
    }

    public void SetEnemyCount(int EnemyCount)
    {
        UserData.gameSetting.enemyCount = EnemyCount+1;
        Debug.Log("enemy count" +UserData.gameSetting.enemyCount);
    }


    public void SetSolution (int ResolutionIndex)
    {
        Resolution Resolution = Resolutions[ResolutionIndex];
        Screen.SetResolution(Resolution.width, Resolution.height, false);
    }

    public void EditUsername(string NewUsername)
    {
        string ValidFormat = @"^[0-9a-zA-Z._()]{4,20}$"; // chagne ValidFormat to put acceptable characters
        bool IsUsed = NewUsername.Equals("used");
        bool IsValid = Regex.IsMatch(NewUsername, ValidFormat);
        if (IsUsed) // chagne condition
        {
            UsernameUsedMsg.gameObject.SetActive(true);
            UsernameInvalidFormatMsg.gameObject.SetActive(false);
        }
        else if (!IsValid)
        {
            UsernameUsedMsg.gameObject.SetActive(false);
            UsernameInvalidFormatMsg.gameObject.SetActive(true);
        }
        else
        {
            UsernameUsedMsg.gameObject.SetActive(false);
            UsernameInvalidFormatMsg.gameObject.SetActive(false);
        }
    }

    public void SetMusicVolume()
    {
        float volume = MusicSlider.value;
        Mixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("Music Volume", volume);
    }

    public void SetSFXVolume()
    {
        float volume = SFXSlider.value;
        Mixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFX Volume", volume);
    }

    public void InitializeVolume()
    {
        if (PlayerPrefs.HasKey("Music Volume") || PlayerPrefs.HasKey("SFX Volume"))
        {
            MusicSlider.value = PlayerPrefs.GetFloat("Music Volume");
            SFXSlider.value = PlayerPrefs.GetFloat("SFX Volume");
        }
        SetMusicVolume();
        SetSFXVolume();
    }
}
