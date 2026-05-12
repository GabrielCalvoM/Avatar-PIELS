using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class SaveLoadHotkeys : MonoBehaviour
{
    private static SaveLoadHotkeys _instance;
    public static SaveLoadHotkeys Instance => _instance;

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

        _instance = this;
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

    private string GetLetterPosePressed(Keyboard keyboard)
    {
        string pose = "";

        //if (keyboard.anyKey.wasPressedThisFrame)
        //{
            if (keyboard.aKey.wasPressedThisFrame) pose = LetterPoses.A;
            if (keyboard.bKey.wasPressedThisFrame) pose = LetterPoses.B;
            if (keyboard.cKey.wasPressedThisFrame) pose = LetterPoses.C;
            if (keyboard.dKey.wasPressedThisFrame) pose = LetterPoses.D;
            if (keyboard.eKey.wasPressedThisFrame) pose = LetterPoses.E;
            if (keyboard.fKey.wasPressedThisFrame) pose = LetterPoses.F;
            if (keyboard.gKey.wasPressedThisFrame) pose = LetterPoses.G;
            if (keyboard.hKey.wasPressedThisFrame) pose = LetterPoses.H;
            if (keyboard.iKey.wasPressedThisFrame) pose = LetterPoses.I;
            if (keyboard.jKey.wasPressedThisFrame) pose = LetterPoses.J;
            if (keyboard.kKey.wasPressedThisFrame) pose = LetterPoses.K;
            if (keyboard.lKey.wasPressedThisFrame) pose = LetterPoses.L;
            if (keyboard.mKey.wasPressedThisFrame) pose = LetterPoses.M;
            if (keyboard.nKey.wasPressedThisFrame) pose = LetterPoses.N;
            if (keyboard.oKey.wasPressedThisFrame) pose = LetterPoses.O;
            if (keyboard.pKey.wasPressedThisFrame) pose = LetterPoses.P;
            if (keyboard.qKey.wasPressedThisFrame) pose = LetterPoses.Q;
            if (keyboard.rKey.wasPressedThisFrame) pose = LetterPoses.R;
            if (keyboard.sKey.wasPressedThisFrame) pose = LetterPoses.S;
            if (keyboard.tKey.wasPressedThisFrame) pose = LetterPoses.T;
            if (keyboard.uKey.wasPressedThisFrame) pose = LetterPoses.U;
            if (keyboard.vKey.wasPressedThisFrame) pose = LetterPoses.V;
            if (keyboard.wKey.wasPressedThisFrame) pose = LetterPoses.W;
            if (keyboard.xKey.wasPressedThisFrame) pose = LetterPoses.X;
            if (keyboard.yKey.wasPressedThisFrame) pose = LetterPoses.Y;
            if (keyboard.zKey.wasPressedThisFrame) pose = LetterPoses.Z;
        //}

        if ((pose.Equals(LetterPoses.H) && keyboard.cKey.isPressed) ||
            (pose.Equals(LetterPoses.C) && keyboard.hKey.isPressed))
        {
            pose = LetterPoses.CH;
        }

        return pose;
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

    public async void HandleLoadLetterHandPoseHotkeys(string pose)
    {
        Transform hand = SaveLoadPose.Instance.GetActiveHandRoot();

        if (!hand) return;

        PoseData handPose = await SaveLoadPose.Instance.LoadHandPoseFromMongoDB(pose, false);

        if (handPose != null)
        {
            if (poseHistory != null)
            {
                poseHistory.RecordStateBeforePoseApply();
            }
            SaveLoadPose.Instance.ApplyHandPose(hand, handPose);
            Debug.Log($"Hand pose '{pose}' loaded from backend");
        }
        else
        {
            Debug.LogError($"Failed to load hand pose: {pose}");
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

    private void LateUpdate()
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

        string pose = GetLetterPosePressed(keyboard);
        if (!string.IsNullOrEmpty(pose) && IsShiftPressed(keyboard)) HandleLoadLetterHandPoseHotkeys(pose);
    }
}
