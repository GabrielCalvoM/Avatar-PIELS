using System;
using UnityEngine;
using VisualKeyboard;

public class LetterKeyboard : VisualKeyboard.VisualKeyboard
{
    public new event Action<LetterKeyForKeyboard> OnKeyClick;
    [SerializeField] private float animationTime = 1f;

    protected override void OnKeyboardButtonClick(VisualKeyForKeyboard key) {;}

    protected void OnLetterKeyboardButtonClick(LetterKeyForKeyboard key)
    {
        Debug.Log($"[Visual Keyboard] Key is clicked: {key.gameObject.name}", gameObject);
        if (keyPressAnimation)
            key.HighlightAnimation(keyPressAnimationColor, animationTime);

        key.LoadPose();

        OnKeyClick?.Invoke(key);
    }

    protected void OnEnable()
    {
        LetterKeyForKeyboard.OnKeyboardButtonClick += OnLetterKeyboardButtonClick;
    }

    protected void OnDisable()
    {
        LetterKeyForKeyboard.OnKeyboardButtonClick -= OnLetterKeyboardButtonClick;
    }
}
