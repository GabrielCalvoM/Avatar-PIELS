using UnityEngine;
using UnityEditor;

////////////////////////////////////////////////////////////////////////////////// STRUCT GLORIFICADO
[CreateAssetMenu(fileName = "BoneDef", menuName = "Scriptable Objects/BoneDef")]
public class BoneDef : ScriptableObject
{
    [Header("Bone Nomenclature List")]
    public string boneName;
    public bool boneSide; // L/R

    [Header("Mov Script")]
    public MonoScript movScript;

    [Header("Articulation UI")]
    public GameObject articulationUI;
    public bool enableX = true;
    public bool enableY = true;
    public bool enableZ = true;
    public float uiSize = 1f;

    [Header("Constraints")]
    public RotationConstraints constraints;

    [Header("Focusable UI")] // Optional
    public MonoScript focusScript;
    public GameObject focusCamera;
}
