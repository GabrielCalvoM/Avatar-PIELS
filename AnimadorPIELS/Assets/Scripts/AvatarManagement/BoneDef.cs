using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

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
    public MonoScript movScript;

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
    public MonoScript focusScript;
    public GameObject focusableUI;
    public GameObject focusCamera;

    public enum ToggleGroupOption
    {
        ArticulationToggleGroup,
        FingersToggleGroup,
    }
}
