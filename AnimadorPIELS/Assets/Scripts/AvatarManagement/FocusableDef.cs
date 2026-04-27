using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

////////////////////////////////////////////////////////////////////////////////// STRUCT GLORIFICADO
[CreateAssetMenu(fileName = "FocusableDef", menuName = "Scriptable Objects/FocusableDef")]
public class FocusableDef : ScriptableObject
{
    [Header("Bone Nomenclature List")]
    public string boneName;
    public bool boneSide; // L/R

    [Header("UI Group")]
    public bool isHand;

    [Header("Focusable UI")]
    public GameObject focusableUI;
    public float uiSize = 1f;
    public ToggleGroupOption toggleGroup = ToggleGroupOption.ArticulationToggleGroup;

    public enum ToggleGroupOption
    {
        ArticulationToggleGroup,
        FingersToggleGroup,
    }
}
