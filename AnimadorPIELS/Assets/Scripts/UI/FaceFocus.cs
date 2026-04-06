using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    [Header("History")]
    [SerializeField] private SaveLoadPose saveLoadPose;

    private bool isFacialEditInProgress;
    private bool suppressHistoryForProgrammaticSliderUpdate;

    //////////////////////////////////////////////////////////// SIGNALS - CAMERA

    private void SetFaceFocusState(bool focused)
    {
        isFocused = focused;

        if (mainCamera != null)
        {
            mainCamera.SetActive(!isFocused);
        }

        if (faceCamera != null)
        {
            faceCamera.SetActive(isFocused);
        }

        if (focusButton != null)
        {
            focusButton.SetActive(!isFocused);
        }

        uiManager?.RefreshUI();
    }

    public void OnButtonPressed()
    {
        SetFaceFocusState(!isFocused);
    }

    public void OnReturnPressed()
    {
        Debug.Log("ME CAGO EN FIGUERES");
        if (isFocused)
        {
            SetFaceFocusState(false);
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
    
    private void OnLeftEyelidChanged(float value)
    {
        NotifyFacialEditChanged();
        avatarFace.SetBlendShapeWeight(leftEyelidIndex, value);
    }

    private void OnRightEyelidChanged(float value)
    {
        NotifyFacialEditChanged();
        avatarFace.SetBlendShapeWeight(rightEyelidIndex, value);
    }

    private void OnRaiseEyebrowChanged(float value)
    {
        NotifyFacialEditChanged();
        avatarFace.SetBlendShapeWeight(raiseEyebrowIndex, value);
    }

    private void OnAngleEyebrowChanged(float value)
    {
        NotifyFacialEditChanged();
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
        NotifyFacialEditChanged();
        avatarFace.SetBlendShapeWeight(mouthHIndex, value);
    }

    private void OnMouthVChanged(float value)
    {
        NotifyFacialEditChanged();
        avatarFace.SetBlendShapeWeight(mouthVIndex, value);
    }

    //////////////////////////////////////////////////////////// GAME LOOP
    
    void Start()
    {
        if (saveLoadPose == null)
        {
            saveLoadPose = FindFirstObjectByType<SaveLoadPose>();
        }

        // Hide in Game
        ColorBlock colors = focusButton.GetComponent<Button>().colors;
        colors.normalColor = new Color(0,0,0,0);
        focusButton.GetComponent<Button>().colors = colors;

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