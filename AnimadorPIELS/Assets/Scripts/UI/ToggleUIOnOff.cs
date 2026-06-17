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
        bool focusedOnHands = uiManager.getFocusedOnHands();

        if (isVisible)
        {
            if (focusedOnHands)
            {
                uiManager.EnableHandsButtons();
            }
            else
            {
                uiManager.EnableBodyButtons();
            }
        }
        else
        {
            if (focusedOnHands)
            {
                uiManager.DisableHandsButtons();
            }
            else
            {
                uiManager.DisableBodyButtons();
            }
        }

        buttonImage.sprite = isVisible ? eyeOpen : eyeClosed;
    }

    public void SetUIOn()
    {
        isVisible = true;
        buttonImage.sprite = eyeOpen;

        bool focusedOnHands = uiManager.getFocusedOnHands();
        if (focusedOnHands)
        {
            uiManager.EnableHandsButtons();
        }
        else
        {
            uiManager.EnableBodyButtons();
        }
    }
}
