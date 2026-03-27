using UnityEngine;
using UnityEngine.UI;

public class FaceFocus : MonoBehaviour
{
    //////////////////////////////////////////////////////////// ATTRIBUTES
    
    [Header ("UI Components")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject focusButton; 
    private bool isFocused = false;

    [Header("Cameras")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject faceCamera;

    [Header("Avatar Face")]
    [SerializeField] private SkinnedMeshRenderer avatarFace;

    [SerializeField] private Slider leftEyelidSlider;
    [SerializeField] private Slider rightEyelidSlider;
    private int leftEyelidIndex;
    private int rightEyelidIndex;

    [SerializeField] private Slider raiseEyebrowSlider;
    [SerializeField] private Slider angleEyebrowSlider;
    private int raiseEyebrowIndex;
    private int low_angleEyebrowIndex;
    private int high_angleEyebrowIndex;

    [SerializeField] private Slider mouthHSlider;
    [SerializeField] private Slider mouthVSlider;
    private int mouthHIndex;
    private int mouthVIndex;

    //////////////////////////////////////////////////////////// SIGNALS - CAMERA

    public void OnButtonPressed()
    {
        isFocused = !isFocused;

        mainCamera.SetActive(!isFocused);
        faceCamera.SetActive(isFocused);

        focusButton.SetActive(!isFocused);
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

    //////////////////////////////////////////////////////////// PUBLIC API - GET/SET FACIAL EXPRESSIONS

    public FacialExpressionData GetFacialExpression()
    {
        return new FacialExpressionData
        {
            leftEyelid = leftEyelidSlider.value,
            rightEyelid = rightEyelidSlider.value,
            raiseEyebrow = raiseEyebrowSlider.value,
            angleEyebrow = angleEyebrowSlider.value,
            mouthH = mouthHSlider.value,
            mouthV = mouthVSlider.value
        };
    }

    public void SetFacialExpression(FacialExpressionData data)
    {
        if (data == null)
        {
            Debug.LogWarning("FacialExpressionData is null. Using default values.");
            return;
        }

        // Set slider values
        leftEyelidSlider.value = data.leftEyelid;
        rightEyelidSlider.value = data.rightEyelid;
        raiseEyebrowSlider.value = data.raiseEyebrow;
        angleEyebrowSlider.value = data.angleEyebrow;
        mouthHSlider.value = data.mouthH;
        mouthVSlider.value = data.mouthV;
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

    private void OnRaiseEyebrowChanged(float value)
    {
        avatarFace.SetBlendShapeWeight(raiseEyebrowIndex, value);
    }

    private void OnAngleEyebrowChanged(float value)
    {
        if (Mathf.Approximately(value, 50.0f)) {
            avatarFace.SetBlendShapeWeight(low_angleEyebrowIndex , 0.0f);
            avatarFace.SetBlendShapeWeight(high_angleEyebrowIndex, 0.0f);
        }

        else if (value < 50.0f) {
            float w = Mathf.InverseLerp(50.0f, 0.0f, value) * 100.0f;

            avatarFace.SetBlendShapeWeight(low_angleEyebrowIndex , w);
            avatarFace.SetBlendShapeWeight(high_angleEyebrowIndex, 0.0f);
        }

        else if (value > 50.0f) {
            float w = Mathf.InverseLerp(50.0f, 100.0f, value) * 100.0f;

            avatarFace.SetBlendShapeWeight(low_angleEyebrowIndex , 0.0f);
            avatarFace.SetBlendShapeWeight(high_angleEyebrowIndex, w);
        }
    }

    private void OnMouthHChanged(float value)
    {
        avatarFace.SetBlendShapeWeight(mouthHIndex, value);
    }

    private void OnMouthVChanged(float value)
    {
        avatarFace.SetBlendShapeWeight(mouthVIndex, value);
    }

    //////////////////////////////////////////////////////////// GAME LOOP
    
    void Start()
    {
        // Get Idx
        leftEyelidIndex  = avatarFace.sharedMesh.GetBlendShapeIndex("blink_left.001");
        rightEyelidIndex = avatarFace.sharedMesh.GetBlendShapeIndex("blink_right.001");
        
        raiseEyebrowIndex = avatarFace.sharedMesh.GetBlendShapeIndex("raise_eyebrows3");
        low_angleEyebrowIndex = avatarFace.sharedMesh.GetBlendShapeIndex("angry");
        high_angleEyebrowIndex = avatarFace.sharedMesh.GetBlendShapeIndex("sad");

        mouthHIndex = avatarFace.sharedMesh.GetBlendShapeIndex("open_mouth");
        mouthVIndex = avatarFace.sharedMesh.GetBlendShapeIndex("o_mouth");

        // Set Range
        leftEyelidSlider.minValue  = 0.0f;
        leftEyelidSlider.maxValue  = 100.0f;
        rightEyelidSlider.minValue = 0.0f;
        rightEyelidSlider.maxValue = 100.0f;

        raiseEyebrowSlider.minValue = 0.0f;
        raiseEyebrowSlider.maxValue = 100.0f;
        angleEyebrowSlider.minValue = 0.0f;
        angleEyebrowSlider.maxValue = 100.0f;
        angleEyebrowSlider.value    = 50.0f;

        mouthHSlider.minValue = 0.0f;
        mouthHSlider.maxValue = 100.0f;
        mouthVSlider.minValue = 0.0f;
        mouthVSlider.maxValue = 100.0f;

        // Connect Signals
        leftEyelidSlider.onValueChanged.AddListener(OnLeftEyelidChanged);
        rightEyelidSlider.onValueChanged.AddListener(OnRightEyelidChanged);

        raiseEyebrowSlider.onValueChanged.AddListener(OnRaiseEyebrowChanged);
        angleEyebrowSlider.onValueChanged.AddListener(OnAngleEyebrowChanged);

        mouthHSlider.onValueChanged.AddListener(OnMouthHChanged);
        mouthVSlider.onValueChanged.AddListener(OnMouthVChanged);
    }

}