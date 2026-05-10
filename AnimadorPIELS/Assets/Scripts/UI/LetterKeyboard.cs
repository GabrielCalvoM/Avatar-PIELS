using System;
using UnityEngine;
using VisualKeyboard;

public class LetterKeyboard : VisualKeyboard.VisualKeyboard
{
    public new event Action<VisualKeyForKeyboard> OnKeyClick;
    [SerializeField] private float animationTime = 1f;

    protected override void OnKeyboardButtonClick(VisualKeyForKeyboard key)
    {
        Debug.Log($"[Visual Keyboard] Key is clicked: {key.gameObject.name}", gameObject);
        if (keyPressAnimation)
            key.HighlightAnimation(keyPressAnimationColor, animationTime);
        OnKeyClick?.Invoke(key);
    }
}
