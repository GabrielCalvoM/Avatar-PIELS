using UnityEngine;

public class FaceFocus : MonoBehaviour
{
    [Header ("UI Manager")]
    [SerializeField] private UIManager uiManager;

    [Header("Cameras")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject faceCamera;

    private bool isFocused = false;

    public void OnButtonPressed()
    {
        isFocused = !isFocused;

        mainCamera.SetActive(!isFocused);
        faceCamera.SetActive(isFocused);

        uiManager.RefreshUI();
    }

    public void OnReturnPressed()
    {
        if (isFocused)
        {
            isFocused = false;

            mainCamera.SetActive(true);
            faceCamera.SetActive(false);

            uiManager.RefreshUI();
        }
    }
}