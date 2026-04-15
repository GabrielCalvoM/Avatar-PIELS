using UnityEngine;
using UnityEngine.UI;

public class ToggleUIOnOff : MonoBehaviour
{
    public Image buttonImage;
    public Sprite eyeOpen;
    public Sprite eyeClosed;
    public UIManager uiManager;

    private bool isVisible = true;

    public void ToggleUI()
    {
        isVisible = !isVisible;
        uiManager.DisableButtons();
        buttonImage.sprite = isVisible ? eyeOpen : eyeClosed;
    }
}
