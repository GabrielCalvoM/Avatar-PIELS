using UnityEngine;

public class FaceFocus : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject faceCamera;

    private bool isFocused = false;

    public void OnButtonPressed()
    {
        isFocused = !isFocused;

        mainCamera.SetActive(!isFocused);
        faceCamera.SetActive(isFocused);
    }
}