using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    void Start()
    {
        if (apply_pose_button != null)
        {
            apply_pose_button.interactable = false;
        }
    }

    void PopulateFiles(string folderPath)
    {
        ClearFileList();

        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("Folder not found: " + folderPath);
            return;
        }

        string[] files = Directory.GetFiles(folderPath);

        foreach (string file in files)
        {
            GameObject item = Instantiate(listButtonPrefab, listContent);

            string fullPath = file;

            Button button = item.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    selected_pose = fullPath;
                    if (apply_pose_button != null)
                    {
                        apply_pose_button.interactable = true;
                    }
                });
            }

            TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();

            string fileName = Path.GetFileName(file);

            text.text = fileName;
        }
    }

    void ClearFileList()
    {
        for (int i = listContent.childCount - 1; i >= 0; i--)
        {
            Destroy(listContent.GetChild(i).gameObject);
        }
    }

    public void LoadPoseButton()
    {
        worldCamera.StopCameraControls();
        loadUI.SetActive(true);
        string path = Application.persistentDataPath + "/Poses";
        PopulateFiles(path);
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
