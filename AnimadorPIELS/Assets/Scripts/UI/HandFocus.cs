using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HandFocus : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject focusCamera;
    [SerializeField] private GameObject cameraControls;
    [SerializeField] private GameObject handsUI;
    [SerializeField] UIManager uiManager;
    [SerializeField] ToggleGroup _fingerGroup;

    public ArticulationUI activeFinger;
    public ToggleGroup FingerGroup { get { return _fingerGroup; } }

    private static HandFocus _instance;
    public static HandFocus Instance { get { return _instance; } }

    public void OnHandFocusButtonPressed()
    {
        _instance = this;
        mainCamera.SetActive(false);
        focusCamera.SetActive(true);
        if (uiManager != null) uiManager.UseFocusCamera(focusCamera);
        //cameraControls.SetActive(false);
        handsUI.SetActive(true);
        Button button = handsUI.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnHandFocusReturnButtonPressed);
        uiManager.SetFocusedOnHands(true);
    }

    public void OnHandFocusReturnButtonPressed()
    {
        if (activeFinger)
        {
            activeFinger.ToggleOff();
            activeFinger = null;
        }

        _instance = null;
        mainCamera.SetActive(true);
        focusCamera.SetActive(false);
        if (uiManager != null) uiManager.UseMainCamera();
        //cameraControls.SetActive(true);
        handsUI.SetActive(false);
        uiManager.SetFocusedOnHands(false);
    }
}
