using NaughtyAttributes;
using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "DefSet", menuName = "Scriptable Objects/DefSet")]
public class DefSet : ScriptableObject
{
    [SerializeField, Expandable, BoxGroup("Focusable")]
    private FocusableDef _faceFocusDef;
    [SerializeField, Expandable, BoxGroup("Focusable")]
    private FocusableDef _handFocusDef;

    [SerializeField, Expandable, BoxGroup("Body")]
    private BoneDef _neckDef;
    [SerializeField, Expandable, BoxGroup("Body")]
    private BoneDef _armDef;
    [SerializeField, Expandable, BoxGroup("Body")]
    private BoneDef _elbowDef;
    [SerializeField, Expandable, BoxGroup("Body")]
    private BoneDef _handDef;

    [SerializeField, Expandable, BoxGroup("Thumb")]
    private BoneDef _thumb1Def;
    [SerializeField, Expandable, BoxGroup("Thumb")]
    private BoneDef _thumb2Def;
    [SerializeField, Expandable, BoxGroup("Thumb")]
    private BoneDef _thumb3Def;

    [SerializeField, Expandable, BoxGroup("Index")]
    private BoneDef _index1Def;
    [SerializeField, Expandable, BoxGroup("Index")]
    private BoneDef _index2Def;
    [SerializeField, Expandable, BoxGroup("Index")]
    private BoneDef _index3Def;

    [SerializeField, Expandable, BoxGroup("Middle")]
    private BoneDef _middle1Def;
    [SerializeField, Expandable, BoxGroup("Middle")]
    private BoneDef _middle2Def;
    [SerializeField, Expandable, BoxGroup("Middle")]
    private BoneDef _middle3Def;

    [SerializeField, Expandable, BoxGroup("Ring")]
    private BoneDef _ring1Def;
    [SerializeField, Expandable, BoxGroup("Ring")]
    private BoneDef _ring2Def;
    [SerializeField, Expandable, BoxGroup("Ring")]
    private BoneDef _ring3Def;

    [SerializeField, Expandable, BoxGroup("Pinky")]
    private BoneDef _pinky1Def;
    [SerializeField, Expandable, BoxGroup("Pinky")]
    private BoneDef _pinky2Def;
    [SerializeField, Expandable, BoxGroup("Pinky")]
    private BoneDef _pinky3Def;

    public FocusableDef FaceFocusDef => _faceFocusDef;
    public FocusableDef HandFocusDef => _handFocusDef;

    public BoneDef NeckDef => _neckDef;
    public BoneDef ArmDef => _armDef;
    public BoneDef ElbowDef => _elbowDef;
    public BoneDef HandDef => _handDef;

    public BoneDef Thumb1Def => _thumb1Def;
    public BoneDef Thumb2Def => _thumb2Def;
    public BoneDef Thumb3Def => _thumb3Def;

    public BoneDef Index1Def => _index1Def;
    public BoneDef Index2Def => _index2Def;
    public BoneDef Index3Def => _index3Def;

    public BoneDef Middle1Def => _middle1Def;
    public BoneDef Middle2Def => _middle2Def;
    public BoneDef Middle3Def => _middle3Def;

    public BoneDef Ring1Def => _ring1Def;
    public BoneDef Ring2Def => _ring2Def;
    public BoneDef Ring3Def => _ring3Def;

    public BoneDef Pinky1Def => _pinky1Def;
    public BoneDef Pinky2Def => _pinky2Def;
    public BoneDef Pinky3Def => _pinky3Def;

}
