using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class SaveLoadHotkeys : MonoBehaviour
{
    [Header("Hotkeys")]
    [SerializeField] private bool enableHotkeys = true;

    private SaveLoadPose saveLoadPose;
    private SaveLoadUI saveLoadUI;
    private PoseHistory poseHistory;

    private void Awake()
    {
        saveLoadPose = GetComponent<SaveLoadPose>();
        saveLoadUI = GetComponent<SaveLoadUI>();
        poseHistory = GetComponent<PoseHistory>();
    }

    private static bool IsCtrlPressed(Keyboard keyboard)
    {
        return keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
    }

    private static bool IsAltPressed(Keyboard keyboard)
    {
        return keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed;
    }

    private static bool IsPrimaryModifierPressed(Keyboard keyboard)
    {
#if UNITY_EDITOR
        return IsAltPressed(keyboard);
#else
        return IsCtrlPressed(keyboard);
#endif
    }

    private static bool IsShiftPressed(Keyboard keyboard)
    {
        return keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
    }

    private static bool IsTextInputFocused()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        if (selectedObject == null)
        {
            return false;
        }

        return selectedObject.GetComponent<TMP_InputField>() != null
               || selectedObject.GetComponent<InputField>() != null;
    }

    private bool IsAnyModalOpen()
    {
        if (saveLoadUI == null)
        {
            return false;
        }

        return (saveLoadUI.loadUI != null && saveLoadUI.loadUI.activeSelf)
               || (saveLoadUI.saveUI != null && saveLoadUI.saveUI.activeSelf);
    }

    private void HandleUndoRedoHotkeys(Keyboard keyboard)
    {
        if (poseHistory == null)
        {
            return;
        }

        bool primaryModifierPressed = IsPrimaryModifierPressed(keyboard);
        bool shiftPressed = IsShiftPressed(keyboard);

        if (primaryModifierPressed && keyboard.zKey.wasPressedThisFrame)
        {
            if (shiftPressed)
            {
                if (poseHistory.CanRedo)
                {
                    poseHistory.RedoPose();
                }
            }
            else if (poseHistory.CanUndo)
            {
                poseHistory.UndoPose();
            }

            return;
        }

        if (primaryModifierPressed && keyboard.yKey.wasPressedThisFrame && poseHistory.CanRedo)
        {
            poseHistory.RedoPose();
        }
    }

    private void HandleSaveLoadHotkeys(Keyboard keyboard)
    {
        if (saveLoadUI == null || saveLoadPose == null || !IsPrimaryModifierPressed(keyboard))
        {
            return;
        }

        if (keyboard.sKey.wasPressedThisFrame)
        {
            if (saveLoadUI.saveUI == null || !saveLoadUI.saveUI.activeSelf)
            {
                saveLoadUI.BeginSaveButton();
            }

            return;
        }

        if (keyboard.lKey.wasPressedThisFrame)
        {
            if (saveLoadUI.loadUI == null || !saveLoadUI.loadUI.activeSelf)
            {
                saveLoadUI.LoadPoseButton();
            }

            return;
        }

        if (keyboard.rKey.wasPressedThisFrame && !IsAnyModalOpen())
        {
            saveLoadPose.ApplyTPose();
        }
    }

    private void Update()
    {
        if (!enableHotkeys)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || IsTextInputFocused())
        {
            return;
        }

        HandleUndoRedoHotkeys(keyboard);
        HandleSaveLoadHotkeys(keyboard);
    }
}
