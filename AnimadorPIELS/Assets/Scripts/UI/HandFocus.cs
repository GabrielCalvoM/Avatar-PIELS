using System;
using Unity.VisualScripting;
using UnityEngine;

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
