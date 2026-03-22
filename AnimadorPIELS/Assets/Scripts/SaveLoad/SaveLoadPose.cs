using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System.Text;

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

    void Start()
    {
        loadUI = GetComponent<SaveLoadUI>();
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

    public void SavePoseButton()
    {
        PoseData pose = CapturePose(avatarSpine);
        string poseName = SanitizeFilename(loadUI.saveFileInput.text);
        if (poseName == "")
        {
            Debug.LogWarning("Please enter a name for the pose.");
            return;
        }
        string path = Application.persistentDataPath + "/UserPoses/" + poseName + ".json";
        SavePoseToFile(pose, path);
        loadUI.CancelSaveButton();
    }

    public void ApplyPoseButton()
    {
        if (loadUI.selected_pose == "")
        {
            Debug.LogWarning("No pose selected to load.");
            return;
        }
        PoseData pose = LoadPoseFromFile(loadUI.selected_pose);
        ApplyPose(avatarSpine, pose);
        loadUI.CancelLoadButton();
    }

    public void ApplyTPose()
    {
        // Ensure TPose.json exists in this path:
        string path = Application.persistentDataPath + "/SystemPoses/" + "tpose" + ".json";
        PoseData pose = LoadPoseFromFile(path);
        if (pose == null)
        {
            Debug.LogWarning("TPose file not found at: " + path);
            return;
        }
        ApplyPose(avatarSpine, pose);
    }
}
