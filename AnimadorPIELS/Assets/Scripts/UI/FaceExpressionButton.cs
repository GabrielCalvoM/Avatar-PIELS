using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class FaceExpressionButton : MonoBehaviour
{
    [SerializeField] private string expressionName;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    private async void OnButtonClicked()
    {
        if (string.IsNullOrWhiteSpace(expressionName))
        {
            Debug.LogWarning("FaceExpressionButton: expressionName is empty.");
            return;
        }

        SaveLoadPose saveLoadPose = FindFirstObjectByType<SaveLoadPose>();
        if (saveLoadPose == null)
        {
            Debug.LogError("FaceExpressionButton: SaveLoadPose not found in scene.");
            return;
        }

        PoseData pose = await saveLoadPose.LoadFacePoseFromMongoDB(expressionName);
        if (pose == null)
        {
            Debug.Log($"Face expression '{expressionName}' not found in MongoDB (or backend not connected).");
            return;
        }

        Transform faceRoot = null;
        const string faceBaseBoneName = "Bone_Base_Face";

        // Prefer resolving the face root under the active avatar hierarchy.
        FaceFocus faceFocus = FaceFocus.Instance != null ? FaceFocus.Instance : FindFirstObjectByType<FaceFocus>();
        if (faceFocus != null)
        {
            foreach (Transform t in faceFocus.transform.root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == faceBaseBoneName)
                {
                    faceRoot = t;
                    break;
                }
            }
        }

        // Fallback: global lookup.
        if (faceRoot == null)
        {
            GameObject found = GameObject.Find(faceBaseBoneName);
            if (found != null)
            {
                faceRoot = found.transform;
            }
        }

        if (faceRoot == null)
        {
            Debug.LogError("FaceExpressionButton: Could not find Bone_Base_Face in scene.");
            return;
        }

        saveLoadPose.BeginPoseEdit();
        try
        {
            saveLoadPose.ApplyFacePose(faceRoot, pose);
        }
        finally
        {
            saveLoadPose.EndPoseEdit();
        }
    }
}
