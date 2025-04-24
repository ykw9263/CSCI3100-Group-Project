using UnityEngine;
using TMPro;

public class InputToText : MonoBehaviour
{
    public TMP_InputField UserInput;
    public TextMeshProUGUI Username;

    void Start()
    {
        UserInput.onEndEdit.AddListener(DisplayUsername);
    }

    void DisplayUsername(string Input)
    {
        Username.text = Input;
    }
}