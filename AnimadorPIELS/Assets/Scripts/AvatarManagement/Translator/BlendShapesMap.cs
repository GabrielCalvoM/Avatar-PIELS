using UnityEngine;

[CreateAssetMenu(fileName = "BlendShapesMap", menuName = "Scriptable Objects/BlendShapesMap")]
public class BlendShapesMap : ScriptableObject
{
    [SerializeField] string _blinkL;
    [SerializeField] string _blinkR;

    [SerializeField] string _raiseEyebrow;
}
