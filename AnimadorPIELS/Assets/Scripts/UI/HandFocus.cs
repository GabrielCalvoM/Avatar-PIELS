using System;
using Unity.VisualScripting;
using UnityEngine;

public class HandFocus : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject handCamera;
    [SerializeField] private GameObject cameraControls;
    [SerializeField] private GameObject handsUI;

    void Start()
    {

    }

    void Update()
    {

    }

    public void OnHandFocusButtonPressed()
    {
        mainCamera.SetActive(false);
        handCamera.SetActive(true);
        cameraControls.SetActive(false);
        handsUI.SetActive(true);
    }

    public void OnHandFocusReturnButtonPressed()
    {
        mainCamera.SetActive(true);
        handCamera.SetActive(false);
        cameraControls.SetActive(true);
        handsUI.SetActive(false);
    }
}
