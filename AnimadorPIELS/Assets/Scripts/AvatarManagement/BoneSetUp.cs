using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;



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

    [Header("UI References")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject uiManager;

    //////////////////////////////////////////////////////////// METHODS
    
    void recursive_collect(Transform root, List<Transform> res) {
        foreach (Transform child in root) {
            res.Add(child);
            recursive_collect(child, res);
        }
    }

    void SetField(Type t, object instance, string fieldName, object value, System.Reflection.BindingFlags flags) {
        var field = t.GetField(fieldName, flags);
        if (field != null) {
            field.SetValue(instance, value);
        } else {
            Debug.LogWarning($"Field {fieldName} not found in {t.Name}");
        }
    }

    void init_bone(Transform bone, BoneDef def) { 
        Type tMov = def.movScript != null ? def.movScript.GetClass() : null;
        ArticulationMov aMov = null;

        // Attach Mov Script
        if (tMov != null) {
            if (!bone.TryGetComponent(tMov, out Component existing)) {
                aMov = bone.gameObject.AddComponent(tMov) as ArticulationMov;
            } else {
                aMov = existing as ArticulationMov;
            }
        }

        // Assign Constraints
        if (aMov != null && def.constraints != null) {
            aMov.SetConstraints(def.constraints);
        }

        // Instantiate Prefab
        if (def.articulationUI != null) {
            GameObject instance = Instantiate(def.articulationUI, bone);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.position = bone.position;
            instance.transform.localScale = Vector3.one;
            
            ArticulationUI aUI = instance.GetComponent<ArticulationUI>();
            if (aUI != null) {
                var t = typeof(ArticulationUI);
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

                SetField(t, aUI, "articulation", aMov, flags);
                SetField(t, aUI, "cameraRef", mainCamera.GetComponent<Camera>(), flags);
                SetField(t, aUI, "X", def.enableX, flags);
                SetField(t, aUI, "Y", def.enableY, flags);
                SetField(t, aUI, "Z", def.enableZ, flags);
                SetField(t, aUI, "scale", def.uiSize, flags);
            }
        }

        // Attach Focusable
        Type tFocus = def.focusScript != null ? def.focusScript.GetClass() : null;
        if (tFocus != null && def.focusCamera != null) {
            if (!bone.TryGetComponent(tFocus, out Component existing)) 
            {    
                MonoBehaviour fInstance = bone.gameObject.AddComponent(tFocus) as MonoBehaviour;
                var t = fInstance.GetType();
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

                SetField(t, fInstance, "focusCamera", def.focusCamera.GetComponent<Camera>(), flags);
                SetField(t, fInstance, "uiManager", uiManager, flags);
            }
        }


        Debug.Log($"Initialized bone {bone.name} with Mov: {(aMov != null ? aMov.GetType().Name : "None")} and UI: {(def.articulationUI != null ? def.articulationUI.name : "None")}");
    }

    void init_bones() {
        if (bone_root == null) {
            Debug.LogError("Bone Root not assigned in BoneSetUp script.");
            return;
        }

        List<Transform> bones = new();
        recursive_collect(bone_root.transform, bones);

        foreach (BoneDef def in bone_definitions) {
            Regex regex = new Regex(def.boneName);
            foreach (Transform b in bones) 
                if (regex.IsMatch(b.name)) { init_bone(b, def); }
        }
    }
    

    //////////////////////////////////////////////////////////// GAMELOOP 
    void Start()
    {
        init_bones();
    }


}
