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


    public void RefreshUI()
    {
        envUI.SetActive(mainCamera.activeSelf);
        faceUI.SetActive(faceCamera.activeSelf);
        genUI.SetActive(mainCamera.activeSelf || faceCamera.activeSelf);
    }
}