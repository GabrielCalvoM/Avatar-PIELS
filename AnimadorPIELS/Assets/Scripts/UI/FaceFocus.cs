using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.ProBuilder;

public class FaceFocus : MonoBehaviour
{
    //////////////////////////////////////////////////////////// ATTRIBUTES

    [Header("UI Components")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject focusButton;
    private bool isFocused = false;

    [Header("Cameras")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject focusCamera;

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

    [Header("History")]
    [SerializeField] private SaveLoadPose saveLoadPose;

    [Header("UI")]
    [SerializeField] private GameObject faceUI;

    private bool isFacialEditInProgress;
    private bool suppressHistoryForProgrammaticSliderUpdate;

    private static FaceFocus _instance;
    public static FaceFocus Instance { get { return _instance; } }

    //////////////////////////////////////////////////////////// SIGNALS - CAMERA

    private void SetFaceFocusState(bool focused)
    {
        isFocused = focused;
        mainCamera.SetActive(!isFocused);
        focusCamera.SetActive(isFocused);
        focusButton.SetActive(!isFocused);
    }

    public void OnFaceButtonPressed()
    {
        //Focus face
        _instance = this;
        SetFaceFocusState(true);
        uiManager.hideCameraControls();
        faceUI.SetActive(true);
        Button button = faceUI.GetComponentInChildren<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnReturnPressed);
        uiManager.SetFocusedOnHands(true);
    }

    public void OnReturnPressed()
    {
        SetFaceFocusState(false);
        faceUI.SetActive(false);
        uiManager.SetFocusedOnHands(false);
        _instance = null;
        uiManager.showCameraControls();
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

        CompleteFacialEdit();

        suppressHistoryForProgrammaticSliderUpdate = true;
        try
        {
            // Set slider values
            leftEyelidSlider.value = data.leftEyelid;
            rightEyelidSlider.value = data.rightEyelid;
            raiseEyebrowSlider.value = data.raiseEyebrow;
            angleEyebrowSlider.value = data.angleEyebrow;
            mouthHSlider.value = data.mouthH;
            mouthVSlider.value = data.mouthV;
        }
        finally
        {
            suppressHistoryForProgrammaticSliderUpdate = false;
        }
    }

    //////////////////////////////////////////////////////////// SIGNALS - SLIDERS

    private void NotifyFacialEditChanged()
    {
        if (suppressHistoryForProgrammaticSliderUpdate)
        {
            return;
        }

        if (saveLoadPose != null && !isFacialEditInProgress)
        {
            saveLoadPose.BeginPoseEdit();
            isFacialEditInProgress = true;
        }
    }

    private void CompleteFacialEdit()
    {
        if (!isFacialEditInProgress)
        {
            return;
        }

        isFacialEditInProgress = false;
        saveLoadPose?.EndPoseEdit();
    }

    private void RegisterSliderHistoryHandlers(Slider slider)
    {
        if (slider == null)
        {
            return;
        }

        EventTrigger trigger = slider.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = slider.gameObject.AddComponent<EventTrigger>();
        }

        AddEventTrigger(trigger, EventTriggerType.PointerDown, _ =>
        {
            if (suppressHistoryForProgrammaticSliderUpdate)
            {
                return;
            }

            NotifyFacialEditChanged();
        });

        AddEventTrigger(trigger, EventTriggerType.PointerUp, _ =>
        {
            if (suppressHistoryForProgrammaticSliderUpdate)
            {
                return;
            }

            CompleteFacialEdit();
        });

        AddEventTrigger(trigger, EventTriggerType.EndDrag, _ =>
        {
            if (suppressHistoryForProgrammaticSliderUpdate)
            {
                return;
            }

            CompleteFacialEdit();
        });
    }

    private void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = eventType
        };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    private void SetBlendShapeWeightSafe(int blendShapeIndex, float value)
    {
        if (avatarFace == null || blendShapeIndex < 0)
        {
            return;
        }

        avatarFace.SetBlendShapeWeight(blendShapeIndex, value);
    }

    private int GetBlendShapeIndexOrFallback(string primaryName, string fallbackName)
    {
        if (avatarFace == null || avatarFace.sharedMesh == null)
        {
            return -1;
        }

        int index = avatarFace.sharedMesh.GetBlendShapeIndex(primaryName);
        if (index >= 0)
        {
            return index;
        }

        return avatarFace.sharedMesh.GetBlendShapeIndex(fallbackName);
    }

    private void ApplyBlinkWeight()
    {
        if (leftEyelidIndex == rightEyelidIndex)
        {
            float mergedBlink = Mathf.Max(leftEyelidSlider.value, rightEyelidSlider.value);
            SetBlendShapeWeightSafe(leftEyelidIndex, mergedBlink);
            return;
        }

        SetBlendShapeWeightSafe(leftEyelidIndex, leftEyelidSlider.value);
        SetBlendShapeWeightSafe(rightEyelidIndex, rightEyelidSlider.value);
    }

    private void OnLeftEyelidChanged(float value)
    {
        NotifyFacialEditChanged();
        ApplyBlinkWeight();
    }

    private void OnRightEyelidChanged(float value)
    {
        NotifyFacialEditChanged();
        ApplyBlinkWeight();
    }

    private void OnRaiseEyebrowChanged(float value)
    {
        NotifyFacialEditChanged();
        SetBlendShapeWeightSafe(raiseEyebrowIndex, value);
    }

    private void OnAngleEyebrowChanged(float value)
    {
        NotifyFacialEditChanged();
        if (Mathf.Approximately(value, 50.0f))
        {
            SetBlendShapeWeightSafe(low_angleEyebrowIndex, 0.0f);
            SetBlendShapeWeightSafe(high_angleEyebrowIndex, 0.0f);
        }

        else if (value < 50.0f)
        {
            float w = Mathf.InverseLerp(50.0f, 0.0f, value) * 100.0f;

            SetBlendShapeWeightSafe(low_angleEyebrowIndex, w);
            SetBlendShapeWeightSafe(high_angleEyebrowIndex, 0.0f);
        }

        else if (value > 50.0f)
        {
            float w = Mathf.InverseLerp(50.0f, 100.0f, value) * 100.0f;

            SetBlendShapeWeightSafe(low_angleEyebrowIndex, 0.0f);
            SetBlendShapeWeightSafe(high_angleEyebrowIndex, w);
        }
    }

    private void OnMouthHChanged(float value)
    {
        NotifyFacialEditChanged();
        SetBlendShapeWeightSafe(mouthHIndex, value);
    }

    private void OnMouthVChanged(float value)
    {
        NotifyFacialEditChanged();
        SetBlendShapeWeightSafe(mouthVIndex, value);
    }

    //////////////////////////////////////////////////////////// GAME LOOP

    void Start()
    {
        if (saveLoadPose == null)
        {
            saveLoadPose = FindFirstObjectByType<SaveLoadPose>();
        }

        if (avatarFace == null || avatarFace.sharedMesh == null)
        {
            Debug.LogError("FaceFocus requires a valid avatarFace with a sharedMesh.");
            return;
        }

        // Hide in Game
        ColorBlock colors = focusButton.GetComponent<Button>().colors;
        colors.normalColor = new Color(0, 0, 0, 0);
        focusButton.GetComponent<Button>().colors = colors;

        // Get Idx
        leftEyelidIndex = GetBlendShapeIndexOrFallback("Blink", "blink_left.001");
        rightEyelidIndex = GetBlendShapeIndexOrFallback("Blink", "blink_right.001");

        raiseEyebrowIndex = GetBlendShapeIndexOrFallback("Emout_Eyebrow_rise", "raise_eyebrows3");
        low_angleEyebrowIndex = GetBlendShapeIndexOrFallback("Emout_furrow", "angry");
        high_angleEyebrowIndex = GetBlendShapeIndexOrFallback("Emout_Sad", "sad");

        mouthHIndex = GetBlendShapeIndexOrFallback("A", "open_mouth");
        mouthVIndex = GetBlendShapeIndexOrFallback("O", "o_mouth");

        if (leftEyelidIndex < 0) Debug.LogWarning("Could not find left eyelid blendshape");
        if (rightEyelidIndex < 0) Debug.LogWarning("Could not find right eyelid blendshape");
        if (raiseEyebrowIndex < 0) Debug.LogWarning("Could not find raise eyebrow blendshape");
        if (low_angleEyebrowIndex < 0) Debug.LogWarning("Could not find low-angle eyebrow blendshape");
        if (high_angleEyebrowIndex < 0) Debug.LogWarning("Could not find high-angle eyebrow blendshape");
        if (mouthHIndex < 0) Debug.LogWarning("Could not find mouthH blendshape");
        if (mouthVIndex < 0) Debug.LogWarning("Could not find mouthV blendshape");

        // Set Range
        leftEyelidSlider.minValue = 0.0f;
        leftEyelidSlider.maxValue = 100.0f;
        rightEyelidSlider.minValue = 0.0f;
        rightEyelidSlider.maxValue = 100.0f;

        raiseEyebrowSlider.minValue = 0.0f;
        raiseEyebrowSlider.maxValue = 100.0f;
        angleEyebrowSlider.minValue = 0.0f;
        angleEyebrowSlider.maxValue = 100.0f;
        angleEyebrowSlider.value = 50.0f;

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

        RegisterSliderHistoryHandlers(leftEyelidSlider);
        RegisterSliderHistoryHandlers(rightEyelidSlider);
        RegisterSliderHistoryHandlers(raiseEyebrowSlider);
        RegisterSliderHistoryHandlers(angleEyebrowSlider);
        RegisterSliderHistoryHandlers(mouthHSlider);
        RegisterSliderHistoryHandlers(mouthVSlider);
    }

    void OnDisable()
    {
        CompleteFacialEdit();
    }

}