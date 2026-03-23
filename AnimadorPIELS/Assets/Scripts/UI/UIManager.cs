using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject faceCamera;

    [Header("UI Panels")]
    [SerializeField] private GameObject genUI;
    [SerializeField] private GameObject envUI;
    [SerializeField] private GameObject faceUI;

    void Update()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (mainCamera.activeSelf)
        {
            envUI.SetActive(true);
            if (faceUI != null) faceUI.SetActive(false);
        }
        else if (faceCamera.activeSelf)
        {
            envUI.SetActive(false);
            if (faceUI != null) faceUI.SetActive(true);
        }
    }
}