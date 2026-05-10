using UnityEngine;
using UnityEngine.UI;

public class FaceFocus : MonoBehaviour
{
    //////////////////////////////////////////////////////////// GAME COMPOSITION

    [Header("UI Components")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject focusButton;
    private bool isFocused = false;

    [Header("Cameras")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject focusCamera;

    [Header("Avatar Face")]
    [SerializeField] private SkinnedMeshRenderer avatarFace;

    [Header("History")]
    [SerializeField] private SaveLoadPose saveLoadPose;

    [Header("UI")]
    [SerializeField] private GameObject faceUI;

    private static FaceFocus _instance;
    public static FaceFocus Instance { get { return _instance; } }

    //////////////////////////////////////////////////////////// SIGNALS - CAMERA

    private void SetFaceFocusState(bool focused)
    {
        isFocused = focused;
        if (mainCamera != null) mainCamera.SetActive(!isFocused);
        if (focusCamera != null) focusCamera.SetActive(isFocused);
        if (focusButton != null) focusButton.SetActive(!isFocused);
    }

    public void OnFaceButtonPressed()
    {
        //Focus face
        _instance = this;
        SetFaceFocusState(true);
        if (uiManager != null)
        {
            uiManager.hideCameraControls();
            uiManager.SetFocusedOnHands(true);
        }

        if (faceUI != null)
        {
            faceUI.SetActive(true);
            Button button = faceUI.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnReturnPressed);
            }
        }
    }

    public void OnReturnPressed()
    {
        SetFaceFocusState(false);
        if (faceUI != null) faceUI.SetActive(false);
        if (uiManager != null)
        {
            uiManager.SetFocusedOnHands(false);
            uiManager.showCameraControls();
        }
        _instance = null;
    }

    //////////////////////////////////////////////////////////// PUBLIC API - GET/SET FACIAL EXPRESSIONS

    public FacialExpressionData GetFacialExpression()
    {
        FacialExpressionData data = new FacialExpressionData();

        if (avatarFace == null || avatarFace.sharedMesh == null)
        {
            return data;
        }

        int blendShapeCount = avatarFace.sharedMesh.blendShapeCount;
        for (int i = 0; i < blendShapeCount; i++)
        {
            string blendshapeName = avatarFace.sharedMesh.GetBlendShapeName(i);
            float weight = avatarFace.GetBlendShapeWeight(i);
            data.blendshapes.Add(new BlendshapeWeightData { name = blendshapeName, weight = weight });
        }

        return data;
    }

    public void SetFacialExpression(FacialExpressionData data)
    {
        if (data == null)
        {
            Debug.LogWarning("FacialExpressionData is null. Using default values.");
            return;
        }

        if (avatarFace == null || avatarFace.sharedMesh == null)
        {
            Debug.LogWarning("FaceFocus: avatarFace not set or missing sharedMesh; cannot apply facial expression.");
            return;
        }

        // Preferred: apply explicit blendshape weights by name.
        if (data.blendshapes != null && data.blendshapes.Count > 0)
        {
            foreach (BlendshapeWeightData blendshape in data.blendshapes)
            {
                if (blendshape == null || string.IsNullOrWhiteSpace(blendshape.name))
                {
                    continue;
                }

                int index = avatarFace.sharedMesh.GetBlendShapeIndex(blendshape.name);
                if (index >= 0)
                {
                    avatarFace.SetBlendShapeWeight(index, blendshape.weight);
                }
            }

            return;
        }

        // Backwards compatibility: older saved poses had only the legacy float fields.
        int leftEyelidIndex = GetBlendShapeIndexOrFallback("Blink", "Eye_Blink_L");
        int rightEyelidIndex = GetBlendShapeIndexOrFallback("Blink", "Eye_Blink_R");
        int raiseEyebrowIndex = GetBlendShapeIndexOrFallback("Emout_Eyebrow_rise", "Brow_Drop_L");
        int mouthHIndex = GetBlendShapeIndexOrFallback("A", "V_Lip_Open");
        int mouthVIndex = GetBlendShapeIndexOrFallback("O", "V_Wide");

        if (leftEyelidIndex == rightEyelidIndex)
        {
            SetBlendShapeWeightSafe(leftEyelidIndex, Mathf.Max(data.leftEyelid, data.rightEyelid));
        }
        else
        {
            SetBlendShapeWeightSafe(leftEyelidIndex, data.leftEyelid);
            SetBlendShapeWeightSafe(rightEyelidIndex, data.rightEyelid);
        }

        SetBlendShapeWeightSafe(raiseEyebrowIndex, data.raiseEyebrow);
        SetBlendShapeWeightSafe(mouthHIndex, data.mouthH);
        SetBlendShapeWeightSafe(mouthVIndex, data.mouthV);
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

    //////////////////////////////////////////////////////////// GAME LOOP

    void Start()
    {
        if (saveLoadPose == null)
        {
            saveLoadPose = FindFirstObjectByType<SaveLoadPose>();
        }

        // Optional: hide focus button visuals in-game.
        if (focusButton != null)
        {
            Button button = focusButton.GetComponent<Button>();
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = new Color(0, 0, 0, 0);
                button.colors = colors;
            }
        }
    }

}