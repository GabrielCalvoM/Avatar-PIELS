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
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
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
            foreach (GameObject bodyPart in bodyParts)
            {
                bodyPart.SetActive(false);
            }
        }
        else
        {
            DisableHandsButtons();
            EnableBodyButtons();
            foreach (GameObject bodyPart in bodyParts)
            {
                bodyPart.SetActive(true);
            }
        }
    }

    //////////////////////////////////////////////////////////// METHODS

    public void RefreshUI()
    {
        envUI.SetActive(mainCamera.activeSelf);
        faceUI.SetActive(faceCamera.activeSelf);
        genUI.SetActive(mainCamera.activeSelf || faceCamera.activeSelf);
    }

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
}