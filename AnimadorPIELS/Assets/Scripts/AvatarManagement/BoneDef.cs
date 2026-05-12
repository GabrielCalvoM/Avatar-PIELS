using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

////////////////////////////////////////////////////////////////////////////////// STRUCT GLORIFICADO
[CreateAssetMenu(fileName = "BoneDef", menuName = "Scriptable Objects/BoneDef")]
public class BoneDef : ScriptableObject
{
    [Header("Bone Nomenclature List")]
    public string boneName;
    public bool boneSide; // L/R

    [Header("UI Group")]
    public bool isHand;

    [Header("Mov Script")]
#if UNITY_EDITOR
    public MonoScript movScript;
#endif
    [SerializeField]
    private string movScriptTypeName;
    public string MovScript => movScriptTypeName;

    [Header("Articulation UI")]
    public GameObject articulationUI;
    public bool enableX = true;
    public bool enableY = true;
    public bool enableZ = true;
    public float uiSize = 1f;
    public ToggleGroupOption toggleGroup = ToggleGroupOption.ArticulationToggleGroup;

    [Header("Constraints")]
    public RotationConstraints constraints;

    [Header("Focusable UI")] // Optional
#if UNITY_EDITOR
    public MonoScript focusScript;
#endif
    [SerializeField]
    private string focusScriptTypeName;
    public string FocusScript => focusScriptTypeName;
    public GameObject focusableUI;
    public GameObject focusCamera;

    public enum ToggleGroupOption
    {
        ArticulationToggleGroup,
        FingersToggleGroup,
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (movScript != null)
            movScriptTypeName = movScript.GetClass()?.AssemblyQualifiedName;
        else
            movScriptTypeName = "";
        if (focusScript != null)
            focusScriptTypeName = focusScript.GetClass()?.AssemblyQualifiedName;
        else
            focusScriptTypeName = "";

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
