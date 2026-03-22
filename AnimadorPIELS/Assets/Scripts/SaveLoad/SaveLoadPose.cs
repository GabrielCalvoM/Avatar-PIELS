using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System.Text;
using System.Threading.Tasks;

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

    private MongoDBService mongoService;

    async void Start()
    {
        loadUI = GetComponent<SaveLoadUI>();

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

    // Update is called once per frame
    void Update()
    {

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
            ApplyPose(avatarSpine, pose);
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
            ApplyPose(avatarSpine, pose);
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
