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
    public Button save_pose_button;
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

    async Task PopulateHandFiles(string folderPath)
    {
        ClearFileList();

        if (saveLoadPose == null)
        {
            Debug.LogError("SaveLoadPose component not found!");
            return;
        }

        // Get hand poses from MongoDB
        List<string> poseNames = await saveLoadPose.GetAllHandPoseNamesFromMongoDB(false);

        if (poseNames.Count == 0)
        {
            Debug.LogWarning("No hand poses found in MongoDB.");
            return;
        }

        // Populate the UI list with hand pose names
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

            string displayName = poseName.Replace("\u200B", "").Replace("\u200C", "").Replace("\u200D", "").Replace("\uFEFF", "");

            text.text = displayName;
        }
    }

    async Task PopulateFaceFiles(string folderPath)
    {
        ClearFileList();

        if (saveLoadPose == null)
        {
            Debug.LogError("SaveLoadPose component not found!");
            return;
        }

        List<string> poseNames = await saveLoadPose.GetAllFacePoseNamesFromMongoDB();

        if (poseNames.Count == 0)
        {
            Debug.LogWarning("No face poses found in MongoDB.");
            return;
        }

        foreach (string poseName in poseNames)
        {
            GameObject item = Instantiate(listButtonPrefab, listContent);

            Button button = item.GetComponent<Button>();
            if (button != null)
            {
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

            string displayName = poseName.Replace("\u200B", "").Replace("\u200C", "").Replace("\u200D", "").Replace("\uFEFF", "");

            text.text = displayName;
        }
    }

    public async void LoadPoseButton()
    {
        worldCamera.StopCameraControls();
        loadUI.SetActive(true);
        string path = Application.persistentDataPath + "/UserPoses";

        if (saveLoadPose != null && FaceFocus.Instance != null)
        {
            await PopulateFaceFiles(path);
            UpdateLoadButtonText();
        }
        else if (saveLoadPose != null && HandFocus.Instance != null)
        {
            // Load hand poses instead of full poses
            await PopulateHandFiles(path);
            UpdateLoadButtonText();
        }
        else
        {
            await PopulateFiles(path);
            UpdateLoadButtonText();
        }
    }

    private void UpdateLoadButtonText()
    {
        if (saveLoadPose != null && FaceFocus.Instance != null)
        {
            if (apply_pose_button != null)
            {
                TextMeshProUGUI buttonText = apply_pose_button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Cargar cara";
                }
            }
        }
        else if (saveLoadPose != null && HandFocus.Instance != null)
        {
            // Find and update the apply button text
            if (apply_pose_button != null)
            {
                TextMeshProUGUI buttonText = apply_pose_button.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Cargar mano";
                }
            }
        }
    }

    public void CancelLoadButton()
    {
        selected_pose = "";
        apply_pose_button.interactable = false;
        loadUI.SetActive(false);
        worldCamera.ResumeCameraControls();
        ResetButtonTexts();
    }

    private void ResetButtonTexts()
    {
        // Reset button texts to defaults
        if (apply_pose_button != null)
        {
            TextMeshProUGUI buttonText = apply_pose_button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Aplicar";
            }
        }

        if (save_pose_button != null)
        {
            TextMeshProUGUI buttonText = save_pose_button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Guardar";
            }
        }
    }

    public void BeginSaveButton()
    {
        worldCamera.StopCameraControls();
        saveUI.SetActive(true);
        UpdateSaveButtonText();
    }

    private void UpdateSaveButtonText()
    {
        if (saveLoadPose != null && FaceFocus.Instance != null && save_pose_button != null)
        {
            TextMeshProUGUI buttonText = save_pose_button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Guardar cara";
            }
        }
        else if (saveLoadPose != null && HandFocus.Instance != null && save_pose_button != null)
        {
            TextMeshProUGUI buttonText = save_pose_button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Guardar mano";
            }
        }
    }

    public void CancelSaveButton()
    {
        saveFileInput.text = "";
        saveUI.SetActive(false);
        worldCamera.ResumeCameraControls();
        ResetButtonTexts();
    }
}
