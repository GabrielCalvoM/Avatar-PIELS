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

    public PoseData CapturePose(Transform root, bool includeFacialExpression = true)
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

        if (includeFacialExpression && faceFocus != null)
        {
            pose.facialExpression = faceFocus.GetFacialExpression();
        }
        else
        {
            pose.facialExpression = new FacialExpressionData(); // Default
        }

        return pose;
    }

    public PoseData CaptureHandPose(Transform handRoot)
    {
        return CapturePose(handRoot, false);
    }

    public void ApplyHandPose(Transform handRoot, PoseData pose)
    {
        char activeHandSide = GetHandSide(handRoot);
        char poseSide = DetectPoseSide(pose);

        if (activeHandSide != poseSide && poseSide != '\0')
        {
            pose = MirrorPose(pose);
        }

        ApplyPose(handRoot, pose, false);
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

    //////////////////////////////////////////////////////////// HAND DETECTION

    private Transform GetActiveHandRoot()
    {
        if (HandFocus.Instance != null)
        {
            Transform current = HandFocus.Instance.transform;
            while (current != null)
            {
                if (current.name.StartsWith("Bone_Hand_"))
                {
                    return current;
                }
                current = current.parent;
            }
        }
        return null;
    }

    private bool IsInHandMode()
    {
        return HandFocus.Instance != null && GetActiveHandRoot() != null;
    }

    //////////////////////////////////////////////////////////// HAND MIRRORING

    /// <summary>
    /// Detects if the hand root is left (L) or right (R) based on its name.
    /// </summary>
    private char GetHandSide(Transform handRoot)
    {
        if (handRoot.name.EndsWith("_L"))
            return 'L';
        if (handRoot.name.EndsWith("_R"))
            return 'R';
        return '\0';
    }

    /// <summary>
    /// Detects if a pose is from the left (L) or right (R) hand by checking bone name suffixes.
    /// </summary>
    private char DetectPoseSide(PoseData pose)
    {
        if (pose == null || pose.bones == null || pose.bones.Count == 0)
            return '\0';

        foreach (BoneData bone in pose.bones)
        {
            if (bone.name.EndsWith("_L"))
                return 'L';
            if (bone.name.EndsWith("_R"))
                return 'R';
        }
        return '\0';
    }

    /// <summary>
    /// Creates a mirrored copy of a pose
    /// </summary>
    private PoseData MirrorPose(PoseData pose)
    {
        PoseData mirrored = new PoseData();
        mirrored.facialExpression = pose.facialExpression;
        mirrored.bones = new List<BoneData>();

        foreach (BoneData bone in pose.bones)
        {
            BoneData mirroredBone = new BoneData();

            if (bone.name.EndsWith("_L"))
                mirroredBone.name = bone.name.Substring(0, bone.name.Length - 1) + "R";
            else if (bone.name.EndsWith("_R"))
                mirroredBone.name = bone.name.Substring(0, bone.name.Length - 1) + "L";
            else
                mirroredBone.name = bone.name;

            mirroredBone.localPosition = new Vector3(
                -bone.localPosition.x,
                bone.localPosition.y,
                bone.localPosition.z
            );

            mirroredBone.localRotation = new Quaternion(
                bone.localRotation.x,
                -bone.localRotation.y,
                -bone.localRotation.z,
                bone.localRotation.w
            );

            mirroredBone.localScale = bone.localScale;

            mirrored.bones.Add(mirroredBone);
        }

        return mirrored;
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

        bool success = false;

        if (IsInHandMode())
        {
            Transform activeHand = GetActiveHandRoot();
            success = await SaveHandPose(poseName, activeHand, false);
        }
        else
        {
            PoseData pose = CapturePose(avatarSpine);
            success = await mongoService.SavePose(poseName, pose, false);

            if (success)
            {
                Debug.Log($"Pose '{poseName}' saved to backend");
            }
            else
            {
                Debug.LogError($"Failed to save pose '{poseName}' to backend.");
            }
        }

        loadUI.CancelSaveButton();
    }

    public async Task<bool> SaveHandPose(string poseName, Transform handRoot, bool isSystemPose = false)
    {
        if (mongoService == null || !mongoService.IsConnected)
        {
            Debug.LogError("Pose backend not connected. Can't save hand pose.");
            return false;
        }

        PoseData handPose = CaptureHandPose(handRoot);
        bool success = await mongoService.SaveHandPose(poseName, handPose, isSystemPose);

        if (success)
        {
            Debug.Log($"Hand pose '{poseName}' saved to backend");
        }
        else
        {
            Debug.LogError($"Failed to save hand pose '{poseName}' to backend.");
        }

        return success;
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

        if (IsInHandMode())
        {
            Transform activeHand = GetActiveHandRoot();
            PoseData handPose = await LoadHandPoseFromMongoDB(loadUI.selected_pose, false);

            if (handPose != null)
            {
                if (poseHistory != null)
                {
                    poseHistory.RecordStateBeforePoseApply();
                }
                ApplyHandPose(activeHand, handPose);
                Debug.Log($"Hand pose '{loadUI.selected_pose}' loaded from backend");
            }
            else
            {
                Debug.LogError($"Failed to load hand pose: {loadUI.selected_pose}");
            }
        }
        else
        {
            // Load full avatar pose
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

    public async Task<List<string>> GetAllHandPoseNamesFromMongoDB(bool isSystemPose = false)
    {
        if (mongoService != null && mongoService.IsConnected)
        {
            return await mongoService.GetAllHandPoseNames(isSystemPose);
        }
        return new List<string>();
    }

    public async Task<PoseData> LoadHandPoseFromMongoDB(string poseName, bool isSystemPose = false)
    {
        if (mongoService != null && mongoService.IsConnected)
        {
            return await mongoService.LoadHandPose(poseName, isSystemPose);
        }
        return null;
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
