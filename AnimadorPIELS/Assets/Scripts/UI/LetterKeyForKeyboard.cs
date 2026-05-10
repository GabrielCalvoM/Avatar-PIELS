using NaughtyAttributes;
using UnityEngine;
using VisualKeyboard;

public class LetterKeyForKeyboard : VisualKeyForKeyboard
{
    [SerializeField, Dropdown("GetLettersPoseName")] private string poseName;

    public DropdownList<string> GetLettersPoseName()
    {
        return LetterPoses.GetLettersPoseName();
    }
}
