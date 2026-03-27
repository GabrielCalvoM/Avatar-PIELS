using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages undo/redo history for pose editing
/// </summary>
public class PoseHistory : MonoBehaviour
{

    [Header("Configuration")]
    [SerializeField] private Transform avatarSpine;
    [SerializeField] private int maxHistoryStates = 50;

    [Header("UI Buttons")]
    [SerializeField] private Button undoButton;
    [SerializeField] private Button redoButton;

    private readonly List<PoseData> undoHistory = new List<PoseData>();
    private readonly List<PoseData> redoHistory = new List<PoseData>();
    private PoseData pendingEditStartPose;
    private bool isPoseEditInProgress;

    private SaveLoadPose saveLoadPose;

    public bool CanUndo => undoHistory.Count > 0;
    public bool CanRedo => redoHistory.Count > 0;


    void Start()
    {
        saveLoadPose = GetComponent<SaveLoadPose>();
        if (saveLoadPose == null)
        {
            Debug.LogError("PoseHistory: SaveLoadPose component not found!");
        }

        ConfigureHistoryButtons();
        UpdateHistoryButtonsState();
    }


    private PoseData ClonePose(PoseData source)
    {
        if (source == null)
        {
            return null;
        }

        PoseData clone = new PoseData();
        clone.bones.Capacity = source.bones.Count;

        foreach (BoneData bone in source.bones)
        {
            clone.bones.Add(new BoneData
            {
                name = bone.name,
                localPosition = bone.localPosition,
                localRotation = bone.localRotation,
                localScale = bone.localScale
            });
        }

        // Facial expressions are not part of undo/redo
        clone.facialExpression = new FacialExpressionData();

        return clone;
    }

    private bool ArePosesEquivalent(PoseData a, PoseData b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        if (a.bones.Count != b.bones.Count)
        {
            return false;
        }

        const float positionEpsilon = 0.0001f;
        const float rotationEpsilon = 0.01f;

        for (int i = 0; i < a.bones.Count; i++)
        {
            BoneData boneA = a.bones[i];
            BoneData boneB = b.bones[i];

            if (boneA.name != boneB.name)
            {
                return false;
            }

            if ((boneA.localPosition - boneB.localPosition).sqrMagnitude > positionEpsilon * positionEpsilon)
            {
                return false;
            }

            if ((boneA.localScale - boneB.localScale).sqrMagnitude > positionEpsilon * positionEpsilon)
            {
                return false;
            }

            if (Quaternion.Angle(boneA.localRotation, boneB.localRotation) > rotationEpsilon)
            {
                return false;
            }
        }

        return true;
    }


    private void PushHistoryState(List<PoseData> history, PoseData pose)
    {
        if (pose == null)
        {
            return;
        }

        history.Add(ClonePose(pose));
        while (history.Count > maxHistoryStates)
        {
            history.RemoveAt(0);
        }
    }

    private PoseData PopHistoryState(List<PoseData> history)
    {
        if (history.Count == 0)
        {
            return null;
        }

        int lastIndex = history.Count - 1;
        PoseData state = history[lastIndex];
        history.RemoveAt(lastIndex);
        return state;
    }

    private bool TryCaptureCurrentPose(out PoseData pose)
    {
        pose = null;

        if (saveLoadPose == null)
        {
            Debug.LogWarning("PoseHistory: SaveLoadPose reference is missing.");
            return false;
        }

        if (avatarSpine == null)
        {
            Debug.LogWarning("PoseHistory: Avatar spine is not assigned. Undo/Redo is disabled.");
            return false;
        }

        pose = saveLoadPose.CapturePose(avatarSpine);
        return true;
    }


    private void ConfigureHistoryButtons()
    {
        if (undoButton == null)
        {
            GameObject undoObject = GameObject.Find("Deshacer");
            if (undoObject != null)
            {
                undoButton = undoObject.GetComponent<Button>();
            }
        }

        if (redoButton == null)
        {
            GameObject redoObject = GameObject.Find("Rehacer");
            if (redoObject != null)
            {
                redoButton = redoObject.GetComponent<Button>();
            }
        }

        if (undoButton != null)
        {
            undoButton.onClick.RemoveListener(UndoPose);
            undoButton.onClick.AddListener(UndoPose);
        }

        if (redoButton != null)
        {
            redoButton.onClick.RemoveListener(RedoPose);
            redoButton.onClick.AddListener(RedoPose);
        }
    }

    private void UpdateHistoryButtonsState()
    {
        if (undoButton != null)
        {
            undoButton.interactable = CanUndo;
        }

        if (redoButton != null)
        {
            redoButton.interactable = CanRedo;
        }
    }


    public void BeginPoseEdit()
    {
        PoseData currentPose;
        if (!TryCaptureCurrentPose(out currentPose))
        {
            return;
        }

        pendingEditStartPose = currentPose;
        isPoseEditInProgress = true;
    }

    public void EndPoseEdit()
    {
        if (!isPoseEditInProgress || pendingEditStartPose == null)
        {
            return;
        }

        isPoseEditInProgress = false;

        PoseData currentPose;
        if (!TryCaptureCurrentPose(out currentPose))
        {
            pendingEditStartPose = null;
            return;
        }

        if (!ArePosesEquivalent(pendingEditStartPose, currentPose))
        {
            PushHistoryState(undoHistory, pendingEditStartPose);
            redoHistory.Clear();
        }

        pendingEditStartPose = null;
        UpdateHistoryButtonsState();
    }

    public void RecordStateBeforePoseApply()
    {
        PoseData currentPose;
        if (!TryCaptureCurrentPose(out currentPose))
        {
            return;
        }

        PushHistoryState(undoHistory, currentPose);
        redoHistory.Clear();
        UpdateHistoryButtonsState();
    }


    public void UndoPose()
    {
        if (saveLoadPose == null || avatarSpine == null)
        {
            return;
        }

        PoseData currentPose;
        if (!TryCaptureCurrentPose(out currentPose))
        {
            return;
        }

        PoseData previousPose = PopHistoryState(undoHistory);
        if (previousPose == null)
        {
            UpdateHistoryButtonsState();
            return;
        }

        PushHistoryState(redoHistory, currentPose);
        saveLoadPose.ApplyPose(avatarSpine, previousPose, false); // don't apply facial expressions
        UpdateHistoryButtonsState();
    }

    public void RedoPose()
    {
        if (saveLoadPose == null || avatarSpine == null)
        {
            return;
        }

        PoseData currentPose;
        if (!TryCaptureCurrentPose(out currentPose))
        {
            return;
        }

        PoseData nextPose = PopHistoryState(redoHistory);
        if (nextPose == null)
        {
            UpdateHistoryButtonsState();
            return;
        }

        PushHistoryState(undoHistory, currentPose);
        saveLoadPose.ApplyPose(avatarSpine, nextPose);
        UpdateHistoryButtonsState();
    }


    public void UndoPoseButton()
    {
        UndoPose();
    }

    public void RedoPoseButton()
    {
        RedoPose();
    }

    public void DeshacerButton()
    {
        UndoPose();
    }

    public void RehacerButton()
    {
        RedoPose();
    }
}
