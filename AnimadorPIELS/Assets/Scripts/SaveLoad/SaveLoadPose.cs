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
public class FacialExpressionData
{
    public float leftEyelid;
    public float rightEyelid;
    public float raiseEyebrow;
    public float angleEyebrow;
    public float mouthH;
    public float mouthV;
}

[System.Serializable]
public class PoseData
{
    public List<BoneData> bones = new List<BoneData>();
    public FacialExpressionData facialExpression = new FacialExpressionData();
}

public class SaveLoadPose : MonoBehaviour
{
    SaveLoadUI loadUI;

    [SerializeField] Transform avatarSpine;
    [SerializeField] MongoDBConfig mongoConfig;
    [SerializeField] FaceFocus faceFocus;

    private MongoDBService mongoService;
    private PoseHistory poseHistory;

    async void Start()
    {
        loadUI = GetComponent<SaveLoadUI>();
        poseHistory = GetComponent<PoseHistory>();

        // Initialize backend API service
        if (mongoConfig == null)
        {
            Debug.LogError("MongoDBConfig not assigned. Please assign the API config in the Inspector.");
            return;
        }

        mongoService = new MongoDBService();
        bool connected = await mongoService.Initialize(mongoConfig);

        if (!connected)
        {
            Debug.LogError("Pose backend connection failed. Please check API URL, backend status, and internet connection.");
        }
        else
        {
            Debug.Log("Pose backend connected successfully! Poses will be saved to the cloud.");
        }
    }

    //////////////////////////////////////////////////////////// PUBLIC API - POSE CAPTURE/APPLY

    public PoseData CapturePose(Transform root)
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

        if (faceFocus != null)
        {
            pose.facialExpression = faceFocus.GetFacialExpression();
        }
        else
        {
            pose.facialExpression = new FacialExpressionData(); // Default
        }

        return pose;
    }

    public void ApplyPose(Transform root, PoseData pose, bool applyFacialExpression = true)
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

        if (applyFacialExpression && faceFocus != null && pose.facialExpression != null)
        {
            faceFocus.SetFacialExpression(pose.facialExpression);
        }
    }

    //////////////////////////////////////////////////////////// PUBLIC API - UNDO/REDO DELEGATION

    public void BeginPoseEdit()
    {
        if (poseHistory != null)
        {
            poseHistory.BeginPoseEdit();
        }
    }

    public void EndPoseEdit()
    {
        if (poseHistory != null)
        {
            poseHistory.EndPoseEdit();
        }
    }

    //////////////////////////////////////////////////////////// FILE I/O HELPERS

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
        if (loadUI == null)
        {
            Debug.LogError("SaveLoadPose: SaveLoadUI component not found.");
            return;
        }

        PoseData pose = CapturePose(avatarSpine);
        string poseName = SanitizeFilename(loadUI.saveFileInput.text);
        if (poseName == "")
        {
            Debug.LogWarning("Please enter a name for the pose.");
            return;
        }

        if (mongoService == null || !mongoService.IsConnected)
        {
            Debug.LogError("Pose backend not connected. Can't save pose.");
            return;
        }

        // Save via backend API
        bool success = await mongoService.SavePose(poseName, pose, false);

        if (success)
        {
            Debug.Log($"Pose '{poseName}' saved to backend");
        }
        else
        {
            Debug.LogError($"Failed to save pose '{poseName}' to backend.");
        }

        loadUI.CancelSaveButton();
    }

    public async void ApplyPoseButton()
    {
        if (loadUI == null)
        {
            Debug.LogError("SaveLoadPose: SaveLoadUI component not found.");
            return;
        }

        if (loadUI.selected_pose == "")
        {
            Debug.LogWarning("No pose selected to load.");
            return;
        }

        if (mongoService == null || !mongoService.IsConnected)
        {
            Debug.LogError("Pose backend not connected. Can't load pose.");
            return;
        }

        // Load via backend API
        PoseData pose = await mongoService.LoadPose(loadUI.selected_pose, false);

        if (pose != null)
        {
            if (poseHistory != null)
            {
                poseHistory.RecordStateBeforePoseApply();
            }
            ApplyPose(avatarSpine, pose);
            Debug.Log($"Pose '{loadUI.selected_pose}' loaded from backend");
        }
        else
        {
            Debug.LogError($"Failed to load pose: {loadUI.selected_pose}");
        }

        loadUI.CancelLoadButton();
    }

    public async void ApplyTPose()
    {
        if (loadUI == null)
        {
            Debug.LogError("SaveLoadPose: SaveLoadUI component not found.");
            return;
        }

        if (mongoService == null || !mongoService.IsConnected)
        {
            Debug.LogError("Pose backend not connected. Can't load T-pose.");
            return;
        }

        // Load T-pose via backend API
        PoseData pose = await mongoService.LoadPose("tpose", true); // true = system pose

        if (pose != null)
        {
            if (poseHistory != null)
            {
                poseHistory.RecordStateBeforePoseApply();
            }
            ApplyPose(avatarSpine, pose);
            Debug.Log("T-pose loaded from backend");
            loadUI.CancelLoadButton();
        }
        else
        {
            Debug.LogError("T-pose not found in backend. Please save it first using 'Save Current Pose as T-Pose to MongoDB' from the GameManager object -> SaveLoadPose component menu.");
        }
    }

    /// <summary>
    /// Get all pose names
    /// Returns empty list if backend is not connected
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
            Debug.LogError("Pose backend not connected. Can't save T-pose.");
            return;
        }

        PoseData tpose = CapturePose(avatarSpine);
        bool success = await mongoService.SavePose("tpose", tpose, true); // true = system pose

        if (success)
        {
            Debug.Log("T-pose saved to backend successfully");
        }
        else
        {
            Debug.LogError("Failed to save T-pose to backend.");
        }
    }
}
