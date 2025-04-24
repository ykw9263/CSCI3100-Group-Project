using UnityEngine;
using TMPro;

public class InputClear : MonoBehaviour
{
    public TMP_InputField Input;
    public void Clear()
    {
        Input.text = "";
    }
}
