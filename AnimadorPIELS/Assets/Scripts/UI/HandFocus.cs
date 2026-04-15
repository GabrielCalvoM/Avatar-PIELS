using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HandFocus : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject handCamera;
    [SerializeField] private GameObject cameraControls;
    [SerializeField] private GameObject handsUI;
    [SerializeField] UIManager uiManager;

    public void OnHandFocusButtonPressed()
    {
        mainCamera.SetActive(false);
        handCamera.SetActive(true);
        cameraControls.SetActive(false);
        handsUI.SetActive(true);
        Button button = handsUI.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnHandFocusReturnButtonPressed);
        uiManager.SetFocusedOnHands(true);
    }

    public void OnHandFocusReturnButtonPressed()
    {
        mainCamera.SetActive(true);
        handCamera.SetActive(false);
        cameraControls.SetActive(true);
        handsUI.SetActive(false);
        uiManager.SetFocusedOnHands(false);
    }
}
