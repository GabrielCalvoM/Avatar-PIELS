using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarMap", menuName = "Scriptable Objects/AvatarMap")]
public class AvatarMap : ScriptableObject
{
    [SerializeField, Expandable, BoxGroup("Head")]
    BlendShapesMap _blendShapes;
    [SerializeField, BoxGroup("Head")]
    string _face;
    [SerializeField, BoxGroup("Head")]
    string _neck;

    [SerializeField, BoxGroup("Arms")]
    string _armL;
    [SerializeField, BoxGroup("Arms")]
    string _armR;

    [SerializeField, BoxGroup("Hands")]
    string _handL;
    [SerializeField, BoxGroup("Hands")]
    string _handR;

    [SerializeField, BoxGroup("Elbows")]
    string _elbowL;
    [SerializeField, BoxGroup("Elbows")]
    string _elbowR;

    [SerializeField, Expandable, BoxGroup("Fingers")]
    FingersMap _fingersL;
    [SerializeField, Expandable, BoxGroup("Fingers")]
    FingersMap _fingersR;

    public BlendShapesMap BlendShapes => _blendShapes;
    public string Face => _face;
    public string Neck => _neck;
    public string ArmL => _armL;
    public string ArmR => _armR;
    public string HandL => _handL;
    public string HandR => _handR;
    public string ElbowL => _elbowL;
    public string ElbowR => _elbowR;
    public FingersMap FingersL => _fingersL;
    public FingersMap FingersR => _fingersR;
}
