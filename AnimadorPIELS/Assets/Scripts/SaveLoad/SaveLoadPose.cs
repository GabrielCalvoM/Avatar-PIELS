using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

[System.Serializable]
public class BoneData
{
    public string name;
    public Vector3 localPosition;
    public Quaternion localRotation;
    public Vector3 localScale;
}

[System.Serializable]
public class PoseData
{
    public List<BoneData> bones = new List<BoneData>();
}

public class SaveLoadPose : MonoBehaviour
{
    SaveLoadUI loadUI;

    [SerializeField] Transform avatarSpine;
    [SerializeField] MongoDBConfig mongoConfig;
    [Header("Undo/Redo")]
    [SerializeField] private Button undoButton;
    [SerializeField] private Button redoButton;
    [SerializeField] private int maxHistoryStates = 50;

    private MongoDBService mongoService;
    private readonly List<PoseData> undoHistory = new List<PoseData>();
    private readonly List<PoseData> redoHistory = new List<PoseData>();
    private PoseData pendingEditStartPose;
    private bool isPoseEditInProgress;

    public bool CanUndo => undoHistory.Count > 0;
    public bool CanRedo => redoHistory.Count > 0;

    async void Start()
    {
        loadUI = GetComponent<SaveLoadUI>();
        ConfigureHistoryButtons();
        UpdateHistoryButtonsState();

        // Initialize MongoDB
        if (mongoConfig == null)
        {
            Debug.LogError("MongoDBConfig not assigned! Please assign it in the Inspector.");
            return;
        }

        mongoService = new MongoDBService();
        bool connected = await mongoService.Initialize(mongoConfig);

        if (!connected)
        {
            Debug.LogError("MongoDB connection failed. Please check your configuration and internet connection.");
        }
        else
        {
            Debug.Log("MongoDB connected successfully! Poses will be saved to the cloud.");
        }
    }

    private PoseData CapturePose(Transform root)
    {
        PoseData pose = new PoseData();

        foreach (Transform t in root.GetComponentsInChildren<Transform>())
        {
            BoneData bone = new BoneData
            {
                name = t.name,
                localPosition = t.localPosition,
                localRotation = t.localRotation,
                localScale = t.localScale
            };

            pose.bones.Add(bone);
        }

        return pose;
    }

    private void ApplyPose(Transform root, PoseData pose)
    {
        Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();

        foreach (Transform t in root.GetComponentsInChildren<Transform>())
        {
            boneMap[t.name] = t;
        }

        foreach (BoneData bone in pose.bones)
        {
            if (boneMap.TryGetValue(bone.name, out Transform t))
            {
                t.localPosition = bone.localPosition;
                t.localRotation = bone.localRotation;
                t.localScale = bone.localScale;
            }
        }
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

        if (avatarSpine == null)
        {
            Debug.LogWarning("Avatar spine is not assigned. Undo/Redo is disabled.");
            return false;
        }

        pose = CapturePose(avatarSpine);
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

    private void RecordStateBeforePoseApply()
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
        ApplyPose(avatarSpine, previousPose);
        UpdateHistoryButtonsState();
    }

    public void RedoPose()
    {
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
        ApplyPose(avatarSpine, nextPose);
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

    public void SavePoseToFile(PoseData pose, string path)
    {
        string json = JsonUtility.ToJson(pose, true);
        File.WriteAllText(path, json);
    }

    public PoseData LoadPoseFromFile(string path)
    {
        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<PoseData>(json);
    }

    private string SanitizeFilename(string filename)
    {
        // Remove zero-width characters and other invisible Unicode characters
        StringBuilder sb = new StringBuilder();
        foreach (char c in filename)
        {
            // Skip zero-width space (​), zero-width joiner, zero-width non-joiner, etc.
            if (c != '\u200B' && c != '\u200C' && c != '\u200D' && c != '\uFEFF' &&
                !char.IsControl(c))
            {
                sb.Append(c);
            }
        }
        return sb.ToString().Trim();
    }

    public async void SavePoseButton()
    {
        PoseData pose = CapturePose(avatarSpine);
        string poseName = SanitizeFilename(loadUI.saveFileInput.text);
        if (poseName == "")
        {
            Debug.LogWarning("Please enter a name for the pose.");
            return;
        }

        if (mongoService == null || !mongoService.IsConnected)
        {
            Debug.LogError("MongoDB not connected. Can't save pose.");
            return;
        }

        // Save to MongoDB only
        bool success = await mongoService.SavePose(poseName, pose, false);

        if (success)
        {
            Debug.Log($"Pose '{poseName}' saved to MongoDB");
        }
        else
        {
            Debug.LogError($"Failed to save pose '{poseName}' to MongoDB.");
        }

        loadUI.CancelSaveButton();
    }

    public async void ApplyPoseButton()
    {
        if (loadUI.selected_pose == "")
        {
            Debug.LogWarning("No pose selected to load.");
            return;
        }

        if (mongoService == null || !mongoService.IsConnected)
        {
            Debug.LogError("MongoDB not connected. Can't load pose.");
            return;
        }

        // Load from MongoDB only
        PoseData pose = await mongoService.LoadPose(loadUI.selected_pose, false);

        if (pose != null)
        {
            RecordStateBeforePoseApply();
            ApplyPose(avatarSpine, pose);
            UpdateHistoryButtonsState();
            Debug.Log($"Pose '{loadUI.selected_pose}' loaded from MongoDB");
        }
        else
        {
            Debug.LogError($"Failed to load pose: {loadUI.selected_pose}");
        }

        loadUI.CancelLoadButton();
    }

    public async void ApplyTPose()
    {
        if (mongoService == null || !mongoService.IsConnected)
        {
            Debug.LogError("MongoDB not connected. Can't load T-pose.");
            return;
        }

        // Load T-pose from MongoDB only
        PoseData pose = await mongoService.LoadPose("tpose", true); // true = system pose

        if (pose != null)
        {
            RecordStateBeforePoseApply();
            ApplyPose(avatarSpine, pose);
            UpdateHistoryButtonsState();
            Debug.Log("T-pose loaded from MongoDB");
        }
        else
        {
            Debug.LogError("T-pose not found in MongoDB. Please save it first using 'Save Current Pose as T-Pose to MongoDB' from the GameManager object -> SaveLoadPose component menu.");
        }
    }

    /// <summary>
    /// Get all pose names
    /// Returns empty list if MongoDB not connected
    /// </summary>
    public async Task<List<string>> GetAllPoseNamesFromMongoDB(bool isSystemPose = false)
    {
        if (mongoService != null && mongoService.IsConnected)
        {
            return await mongoService.GetAllPoseNames(isSystemPose);
        }
        return new List<string>();
    }

    /// <summary>
    /// Helper: Save the current pose as T-pose
    /// Use this once to set up the default T-pose in the database
    /// </summary>
    [ContextMenu("Save Current Pose as T-Pose to MongoDB")]
    public async void SaveCurrentPoseAsTPose()
    {
        if (mongoService == null || !mongoService.IsConnected)
        {
            Debug.LogError("MongoDB not connected. Can't save T-pose.");
            return;
        }

        PoseData tpose = CapturePose(avatarSpine);
        bool success = await mongoService.SavePose("tpose", tpose, true); // true = system pose

        if (success)
        {
            Debug.Log("T-pose saved to MongoDB successfully");
        }
        else
        {
            Debug.LogError("Failed to save T-pose to MongoDB.");
        }
    }
}
