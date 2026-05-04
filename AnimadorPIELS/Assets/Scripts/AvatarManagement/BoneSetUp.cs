using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class BoneSetUp : MonoBehaviour
{

    //////////////////////////////////////////////////////////// ATTRIBUTES
    // Bone Nomenclature List
    /*
        Bone_Neck + NeckMov Script + NeckConstraint + FaceFocus[Camera]
        Bone_UpperArm_<L/R> + UpperArmMov Script
        Bone_ForeArm_<L/R> + LowerArmMov Script + ElbowConstraint
        Bone_Hand_<L/R> + HandMove Script + HandConstraint + HandFocus[Camera]
        Bone_F_Index_01_<L/R> + Bone_F_Index_02_<L/R> + FingerMov Script + FingerConstraint01 + FingerConstraint02
        Bone_F_Middle_01_<L/R> + Bone_F_Middle_02_<L/R> + FingerMov Script + FingerConstraint01 + FingerConstraint02
        Bone_F_Ring_01_<L/R> + Bone_F_Ring_02_<L/R> + FingerMov Script + FingerConstraint01 + FingerConstraint02
        Bone_F_Pinky_01_<L/R> + Bone_F_Pinky_02_<L/R> + FingerMov Script + FingerConstraint01 + FingerConstraint02
        Bone_F_Thumb_01_<L/R> + Bone_F_Thumb_02_<L/R> + FingerMov Script + FingerConstraint01 + FingerConstraint02

    */

    [Header("Bone Skeleton Search")]
    [SerializeField] private GameObject bone_root;
    [SerializeField] private List<BoneDef> bone_definitions;
    [SerializeField] private List<FocusableDef> focusable_definitions; // hands and face

    [Header("3D model references")]
    [SerializeField] private SkinnedMeshRenderer avatarFace;

    [Header("UI References")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject uiManager;
    [SerializeField] private GameObject envUI;
    [SerializeField] private GameObject handsUI;
    [SerializeField] private GameObject faceUI;
    [SerializeField] private ToggleGroup articulationToggleGroup;
    [SerializeField] private ToggleGroup fingersToggleGroup;

    // Collected buttons to assign to UIManager at the end of initialization
    private List<GameObject> collectedBodyButtons = new();
    private List<GameObject> collectedHandsButtons = new();

    //////////////////////////////////////////////////////////// METHODS

    void recursive_collect(Transform root, List<Transform> res)
    {
        foreach (Transform child in root)
        {
            res.Add(child);
            recursive_collect(child, res);
        }
    }

    void SetField(Type t, object instance, string fieldName, object value, System.Reflection.BindingFlags flags)
    {
        var field = t.GetField(fieldName, flags);
        if (field != null)
        {
            field.SetValue(instance, value);
        }
        else
        {
            Debug.LogWarning($"Field {fieldName} not found in {t.Name}");
        }
    }

    void init_bone(Transform bone, BoneDef def)
    {
        Type tMov = def.movScript != null ? def.movScript.GetClass() : null;
        ArticulationMov aMov = null;

        // Attach Mov Script
        if (tMov != null)
        {
            if (!bone.TryGetComponent(tMov, out Component existing))
            {
                aMov = bone.gameObject.AddComponent(tMov) as ArticulationMov;
            }
            else
            {
                aMov = existing as ArticulationMov;
            }
        }

        // Assign Constraints
        if (aMov != null && def.constraints != null)
        {
            aMov.SetConstraints(def.constraints);
        }

        // Instantiate Prefab
        if (def.articulationUI != null)
        {
            GameObject instance = Instantiate(def.articulationUI, bone);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.position = bone.position;
            instance.transform.localScale = def.uiSize * Vector3.one;

            switch (def.toggleGroup)
            {
                case BoneDef.ToggleGroupOption.ArticulationToggleGroup:
                    break;
                case BoneDef.ToggleGroupOption.FingersToggleGroup:
                    break;
            }

            ToggleGroup targetToggleGroup = def.toggleGroup switch
            {
                BoneDef.ToggleGroupOption.ArticulationToggleGroup => articulationToggleGroup,
                BoneDef.ToggleGroupOption.FingersToggleGroup => fingersToggleGroup,
                _ => null
            };

            if (targetToggleGroup == null)
            {
                Debug.LogWarning($"ToggleGroup reference not set for {def.toggleGroup} (bone: {bone.name})");
            }
            else
            {
                Toggle toggle = instance.GetComponentInChildren<Toggle>(true);
                if (toggle != null)
                {
                    toggle.group = targetToggleGroup;
                }
            }

            ArticulationUI aUI = instance.GetComponent<ArticulationUI>();
            if (aUI != null)
            {
                var t = typeof(ArticulationUI);
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

                SetField(t, aUI, "articulation", aMov, flags);
                SetField(t, aUI, "uiManager", uiManager.GetComponent<UIManager>(), flags);
                SetField(t, aUI, "cameraRef", mainCamera.GetComponent<Camera>(), flags);
                SetField(t, aUI, "X", def.enableX, flags);
                SetField(t, aUI, "Y", def.enableY, flags);
                SetField(t, aUI, "Z", def.enableZ, flags);
                SetField(t, aUI, "scale", 1, flags);
            }

            // Collect any buttons/toggles from the instantiated articulation UI
            var found = CollectButtonsFromInstance(instance);
            foreach (var go in found)
            {
                if (def.isHand)
                {
                    collectedHandsButtons.Add(go);
                }
                else collectedBodyButtons.Add(go);
            }
        }

        Debug.Log($"Initialized bone {bone.name} with Mov: {(aMov != null ? aMov.GetType().Name : "None")} and UI: {(def.articulationUI != null ? def.articulationUI.name : "None")}");
    }

    void init_hand_focusable(Transform bone, FocusableDef def)
    {
        if (def.focusableUI == null) return;
        GameObject focusCamera = Instantiate(def.cameraPrefab, bone);

        float yRot = bone.name.Contains("_L") ? 90f : bone.name.Contains("_R") ? -90f : 0f;
        if (yRot != 0f) focusCamera.transform.RotateAround(bone.position, bone.up, yRot);

        Camera cam = focusCamera.GetComponentInChildren<Camera>(true);
        foreach (var c in bone.GetComponentsInChildren<Canvas>(true))
            c.worldCamera = cam;

        GameObject instance = Instantiate(def.focusableUI, bone);
        instance.transform.position = bone.position;
        instance.transform.localScale = def.uiSize * Vector3.one;

        // If this focusable uses HandFocus, wire required refs
        var handFocuses = instance.GetComponentsInChildren<HandFocus>(true);
        if (handFocuses != null && handFocuses.Length > 0)
        {
            var t = typeof(HandFocus);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            UIManager uiComp = uiManager != null ? uiManager.GetComponent<UIManager>() : null;

            foreach (var hf in handFocuses)
            {
                if (hf == null) continue;
                if (mainCamera != null) SetField(t, hf, "mainCamera", mainCamera, flags);
                if (focusCamera != null) SetField(t, hf, "focusCamera", focusCamera, flags);
                if (envUI != null) SetField(t, hf, "cameraControls", envUI, flags);
                if (handsUI != null) SetField(t, hf, "handsUI", handsUI, flags);
                if (uiComp != null) SetField(t, hf, "uiManager", uiComp, flags);
                if (fingersToggleGroup != null) SetField(t, hf, "_fingerGroup", fingersToggleGroup, flags);
            }
        }

        ToggleGroup targetToggleGroup = def.toggleGroup switch
        {
            FocusableDef.ToggleGroupOption.ArticulationToggleGroup => articulationToggleGroup,
            FocusableDef.ToggleGroupOption.FingersToggleGroup => fingersToggleGroup,
            _ => null
        };

        if (targetToggleGroup != null)
        {
            Toggle toggle = instance.GetComponentInChildren<Toggle>(true);
            if (toggle != null)
            {
                toggle.group = targetToggleGroup;
            }
        }

        // Collect any buttons/toggles from the instantiated focusable UI
        var found = CollectButtonsFromInstance(instance);
        foreach (var go in found)
        {
            if (def.isHand)
            {
                collectedHandsButtons.Add(go);
            }
            else collectedBodyButtons.Add(go);
        }
    }

    void init_face_focusable(Transform bone, FocusableDef def)
    {
        if (def.focusableUI == null) return;
        GameObject focusCamera = Instantiate(def.cameraPrefab, bone);

        GameObject instance = Instantiate(def.focusableUI, bone);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.position = bone.position;
        instance.transform.localScale = def.uiSize * Vector3.one;

        // If this focusable uses FaceFocus, wire required refs
        FaceFocus faceFocus = instance.gameObject.GetComponent<FaceFocus>();
        if (faceFocus == null) return;

        var t = typeof(FaceFocus);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        UIManager uiComp = uiManager != null ? uiManager.GetComponent<UIManager>() : null;

        SetField(t, faceFocus, "uiManager", uiComp, flags);
        SetField(t, faceFocus, "mainCamera", mainCamera, flags);
        SetField(t, faceFocus, "focusCamera", focusCamera, flags);
        SetField(t, faceFocus, "faceUI", faceUI, flags);
        SetField(t, faceFocus, "avatarFace", avatarFace, flags);

        Transform FindInChildren(Transform root, string childName)
        {
            foreach (Transform tr in root.GetComponentsInChildren<Transform>(true))
            {
                if (tr.name == childName) return tr;
            }
            return null;
        }

        // Face UI -> FacePanel -> (Eyelids/Eyebrows/Mouth) -> Sliders
        Transform facePanel = FindInChildren(faceUI.transform, "FacePanel");
        Transform eyelids = FindInChildren(facePanel, "Eyelids");
        Transform eyebrows = FindInChildren(facePanel, "Eyebrows");
        Transform mouth = FindInChildren(facePanel, "Mouth");

        Button returnButton = facePanel.GetComponentInChildren<Button>(true);
        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(faceFocus.OnReturnPressed);

        Slider[] eyelidSliders = eyelids.GetComponentsInChildren<Slider>(true);
        Slider[] eyebrowSliders = eyebrows.GetComponentsInChildren<Slider>(true);
        Slider[] mouthSliders = mouth.GetComponentsInChildren<Slider>(true);

        SetField(t, faceFocus, "leftEyelidSlider", eyelidSliders[0], flags);
        SetField(t, faceFocus, "rightEyelidSlider", eyelidSliders[1], flags);

        SetField(t, faceFocus, "raiseEyebrowSlider", eyebrowSliders[0], flags);
        SetField(t, faceFocus, "angleEyebrowSlider", eyebrowSliders[1], flags);

        SetField(t, faceFocus, "mouthHSlider", mouthSliders[0], flags);
        SetField(t, faceFocus, "mouthVSlider", mouthSliders[1], flags);


        ToggleGroup targetToggleGroup = def.toggleGroup switch
        {
            FocusableDef.ToggleGroupOption.ArticulationToggleGroup => articulationToggleGroup,
            FocusableDef.ToggleGroupOption.FingersToggleGroup => fingersToggleGroup,
            _ => null
        };

        if (targetToggleGroup != null)
        {
            Toggle toggle = instance.GetComponentInChildren<Toggle>(true);
            if (toggle != null)
            {
                toggle.group = targetToggleGroup;
            }
        }

        // Collect any buttons/toggles from the instantiated focusable UI
        var found = CollectButtonsFromInstance(instance);
        foreach (var go in found)
        {
            if (def.isHand)
            {
                collectedHandsButtons.Add(go);
            }
            else collectedBodyButtons.Add(go);
        }
    }

    void init_bones()
    {
        if (bone_root == null)
        {
            Debug.LogError("Bone Root not assigned in BoneSetUp script.");
            return;
        }

        List<Transform> bones = new();
        recursive_collect(bone_root.transform, bones);

        foreach (BoneDef def in bone_definitions)
        {
            if (!def) continue; // Skip null entries

            Regex regex = new Regex(def.boneName);
            foreach (Transform b in bones)
                if (regex.IsMatch(b.name)) { init_bone(b, def); }
        }

        foreach (FocusableDef def in focusable_definitions)
        {
            if (!def) continue; // Skip null entries

            Regex regex = new Regex(def.boneName);
            foreach (Transform b in bones)
                if (regex.IsMatch(b.name))
                {
                    if (b.name.StartsWith("Bone_Hand"))
                    {
                        init_hand_focusable(b, def);
                    }
                    else if (b.name.StartsWith("Bone_Base_Face"))
                    {
                        init_face_focusable(b, def);
                    }
                }
        }

        // After initializing bones, assign collected buttons to UIManager
        if (uiManager != null)
        {
            var uiComp = uiManager.GetComponent<UIManager>();
            if (uiComp != null)
            {
                var t = typeof(UIManager);
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

                SetField(t, uiComp, "bodyButtons", collectedBodyButtons.ToArray(), flags);
                SetField(t, uiComp, "handsButtons", collectedHandsButtons.ToArray(), flags);

                uiComp.EnableBodyButtons();
                uiComp.DisableHandsButtons();
            }
            else
            {
                Debug.LogWarning("uiManager GameObject does not have a UIManager component.");
            }
        }
        else
        {
            Debug.LogWarning("uiManager reference not set in BoneSetUp.");
        }
    }

    List<GameObject> CollectButtonsFromInstance(GameObject root)
    {
        List<GameObject> res = new List<GameObject>();
        var buttons = root.GetComponentsInChildren<UnityEngine.UI.Button>(true);
        foreach (var b in buttons) if (!res.Contains(b.gameObject)) res.Add(b.gameObject);
        var toggles = root.GetComponentsInChildren<UnityEngine.UI.Toggle>(true);
        foreach (var t in toggles) if (!res.Contains(t.gameObject)) res.Add(t.gameObject);
        return res;
    }

    //////////////////////////////////////////////////////////// GAMELOOP 
    void Start()
    {
        init_bones();
    }


}
