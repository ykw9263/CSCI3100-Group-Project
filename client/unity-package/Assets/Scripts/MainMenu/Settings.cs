using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class Settings : MonoBehaviour
{
    public TMP_Dropdown ResolutionDropdown;
    public TextMeshProUGUI UsernameUsedMsg, UsernameInvalidFormatMsg;
    public TMP_InputField NewUsername;
    Resolution[] Resolutions;

    // Start is called before the first frame update
    void Start()
    {
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
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
