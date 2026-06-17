using NaughtyAttributes;
using System;
using UnityEngine;
using VisualKeyboard;

public class LetterKeyForKeyboard : VisualKeyForKeyboard
{
    [SerializeField, Dropdown("GetLettersPoseName")] private string poseName;
    public static new event Action<LetterKeyForKeyboard> OnKeyboardButtonClick;

    public DropdownList<string> GetLettersPoseName()
    {
        return LetterPoses.GetLettersPoseName();
    }

    public void LoadPose()
    {
        SaveLoadHotkeys.Instance.HandleLoadLetterHandPoseHotkeys(poseName);
    }

    public override void UI_Click()
    {
        Debug.Log($"Keyboard button is clicked: {gameObject.name}", gameObject);
        OnKeyboardButtonClick?.Invoke(this);
    }
}
