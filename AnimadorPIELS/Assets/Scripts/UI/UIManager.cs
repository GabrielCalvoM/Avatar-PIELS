using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    //////////////////////////////////////////////////////////// ATTRIBUTES

    [Header("Cameras")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject faceCamera;
    [SerializeField] private GameObject activeCamera;

    [Header("UI Panels")]
    [SerializeField] private GameObject genUI;
    [SerializeField] private GameObject envUI;
    [SerializeField] private GameObject faceUI;

    [Header("Joint Buttons")]
    [SerializeField] GameObject[] bodyButtons;
    [SerializeField] ToggleGroup bodyToggleGroup;
    [SerializeField] GameObject[] handsButtons;
    [SerializeField] ToggleGroup handsToggleGroup;

    [Header("Body Mesh")]
    [SerializeField] GameObject[] bodyParts;

    private bool focusedOnHands = false;

    private ToggleUIOnOff toggleUIOnOff;

    void Start()
    {
        toggleUIOnOff = GetComponent<ToggleUIOnOff>();
        activeCamera = mainCamera;
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                toggleUIOnOff.SetUIOn();
            }
        }
    }

    public void SetFocusedOnHands(bool value)
    {
        focusedOnHands = value;

        if (focusedOnHands)
        {
            DisableBodyButtons();
            EnableHandsButtons();
        }
        else
        {
            DisableHandsButtons();
            EnableBodyButtons();
        }
    }

    //////////////////////////////////////////////////////////// METHODS

    public void UseFocusCamera(GameObject focusCamera)
    {
        activeCamera = focusCamera != null ? focusCamera : mainCamera;
    }

    public void UseMainCamera()
    {
        activeCamera = mainCamera;
    }

    public void CenterFrontView() => activeCamera?.BroadcastMessage(nameof(WorldCamera.CenterFrontView), SendMessageOptions.DontRequireReceiver);
    public void CenterTopView() => activeCamera?.BroadcastMessage(nameof(WorldCamera.CenterTopView), SendMessageOptions.DontRequireReceiver);
    public void CenterBottomView() => activeCamera?.BroadcastMessage(nameof(WorldCamera.CenterBottomView), SendMessageOptions.DontRequireReceiver);
    public void CenterLeftView() => activeCamera?.BroadcastMessage(nameof(WorldCamera.CenterLeftView), SendMessageOptions.DontRequireReceiver);
    public void CenterRightView() => activeCamera?.BroadcastMessage(nameof(WorldCamera.CenterRightView), SendMessageOptions.DontRequireReceiver);

    public void EnableButtons()
    {
        foreach (GameObject button in bodyButtons)
        {
            button.SetActive(true);
        }

        foreach (GameObject button in handsButtons)
        {
            button.SetActive(true);
        }

        bodyToggleGroup.SetAllTogglesOff();
        handsToggleGroup.SetAllTogglesOff();
    }

    public void DisableButtons()
    {
        foreach (GameObject button in bodyButtons)
        {
            button.SetActive(false);
        }

        foreach (GameObject button in handsButtons)
        {
            button.SetActive(false);
        }
    }

    public void EnableBodyButtons()
    {
        foreach (GameObject button in bodyButtons)
        {
            button.SetActive(true);
        }

        bodyToggleGroup.SetAllTogglesOff();
    }

    public void DisableBodyButtons()
    {
        foreach (GameObject button in bodyButtons)
        {
            button.SetActive(false);
        }
    }

    public void EnableHandsButtons()
    {
        foreach (GameObject button in handsButtons)
        {
            button.SetActive(true);
        }

        handsToggleGroup.SetAllTogglesOff();
    }

    public void DisableHandsButtons()
    {
        foreach (GameObject button in handsButtons)
        {
            button.SetActive(false);
        }
    }

    public bool getFocusedOnHands()
    {
        return focusedOnHands;
    }

    public void showCameraControls()
    {
        envUI.SetActive(true);
    }

    public void hideCameraControls()
    {
        envUI.SetActive(false);
    }
}