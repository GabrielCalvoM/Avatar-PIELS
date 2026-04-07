using Unity.VisualScripting;
using UnityEngine;

using NaughtyAttributes;

[CreateAssetMenu(fileName = "RotationConstraints", menuName = "Scriptable Objects/RotationConstraints")]
public class RotationConstraints : ScriptableObject
{
    [System.Serializable]
    public struct AxisConstraints
    {
        public bool active;
        [MinMaxSlider(-180f, 180f), ShowIf("active")]
        public Vector2 constraints;
        public float MinValue { get { return constraints[0]; } }
        public float MaxValue { get { return constraints[1]; } }
    }

    [BoxGroup("")]
    public AxisConstraints x;

    [BoxGroup("")]
    public AxisConstraints y;

    [BoxGroup("")]
    public AxisConstraints z;
}
