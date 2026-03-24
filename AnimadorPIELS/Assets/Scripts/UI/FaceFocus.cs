using UnityEngine;
using UnityEngine.UI;

public class FaceFocus : MonoBehaviour
{
    //////////////////////////////////////////////////////////// ATTRIBUTES
    
    [Header ("UI Manager")]
    [SerializeField] private UIManager uiManager;

    [Header("Cameras")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject faceCamera;
    private bool isFocused = false;

    [Header("Avatar Face")]
    [SerializeField] private SkinnedMeshRenderer avatarFace;
    [SerializeField] private Slider leftEyelidSlider;
    [SerializeField] private Slider rightEyelidSlider;

    private int leftEyelidIndex;
    private int rightEyelidIndex;

    //////////////////////////////////////////////////////////// SIGNALS - CAMERA

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

    //////////////////////////////////////////////////////////// SIGNALS - SLIDERS
    
    private void OnLeftEyelidChanged(float value)
    {
        avatarFace.SetBlendShapeWeight(leftEyelidIndex, value);
    }

    private void OnRightEyelidChanged(float value)
    {
        avatarFace.SetBlendShapeWeight(rightEyelidIndex, value);
    }

    //////////////////////////////////////////////////////////// GAME LOOP
    
    void Start()
    {
        // Get Idx
        leftEyelidIndex = avatarFace.sharedMesh.GetBlendShapeIndex("blink_left");
        rightEyelidIndex = avatarFace.sharedMesh.GetBlendShapeIndex("blink_right");

        // Set Range
        leftEyelidSlider.minValue = 0.0f;
        leftEyelidSlider.maxValue = 100.0f;
        rightEyelidSlider.minValue = 0.0f;
        rightEyelidSlider.maxValue = 100.0f;

        // Connect Signals
        leftEyelidSlider.onValueChanged.AddListener(OnLeftEyelidChanged);
        rightEyelidSlider.onValueChanged.AddListener(OnRightEyelidChanged);
    }

}