using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(SaveLoadHotkeys))]
public class SaveLoadUI : MonoBehaviour
{
    public GameObject loadUI;
    public GameObject saveUI;
    public Transform listContent;
    public GameObject listButtonPrefab;
    public Button apply_pose_button;
    public WorldCamera worldCamera;
    public TextMeshProUGUI saveFileInput;

    [HideInInspector] public string selected_pose = "";

    private SaveLoadPose saveLoadPose;

    void Start()
    {
        saveLoadPose = GetComponent<SaveLoadPose>();

        if (apply_pose_button != null)
        {
            apply_pose_button.interactable = false;
        }
    }

    async Task PopulateFiles(string folderPath)
    {
        ClearFileList();

        if (saveLoadPose == null)
        {
            Debug.LogError("SaveLoadPose component not found!");
            return;
        }

        // Get poses from MongoDB only
        List<string> poseNames = await saveLoadPose.GetAllPoseNamesFromMongoDB(false);

        if (poseNames.Count == 0)
        {
            Debug.LogWarning("No poses found in MongoDB.");
            return;
        }

        // Populate the UI list with pose names
        foreach (string poseName in poseNames)
        {
            GameObject item = Instantiate(listButtonPrefab, listContent);

            Button button = item.GetComponent<Button>();
            if (button != null)
            {
                // Store just the pose name for MongoDB
                string capturedPoseName = poseName;
                button.onClick.AddListener(() =>
                {
                    selected_pose = capturedPoseName;
                    if (apply_pose_button != null)
                    {
                        apply_pose_button.interactable = true;
                    }
                });
            }

            TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();

            // Remove any invisible Unicode characters from display
            string displayName = poseName.Replace("\u200B", "").Replace("\u200C", "").Replace("\u200D", "").Replace("\uFEFF", "");

            text.text = displayName;
        }
    }

    void ClearFileList()
    {
        for (int i = listContent.childCount - 1; i >= 0; i--)
        {
            Destroy(listContent.GetChild(i).gameObject);
        }
    }

    public async void LoadPoseButton()
    {
        worldCamera.StopCameraControls();
        loadUI.SetActive(true);
        string path = Application.persistentDataPath + "/UserPoses";
        await PopulateFiles(path);
    }

    public void CancelLoadButton()
    {
        selected_pose = "";
        apply_pose_button.interactable = false;
        loadUI.SetActive(false);
        worldCamera.ResumeCameraControls();
    }

    public void BeginSaveButton()
    {
        worldCamera.StopCameraControls();
        saveUI.SetActive(true);
    }

    public void CancelSaveButton()
    {
        saveFileInput.text = "";
        saveUI.SetActive(false);
        worldCamera.ResumeCameraControls();
    }
}
