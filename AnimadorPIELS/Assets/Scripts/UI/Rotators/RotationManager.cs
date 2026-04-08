using System;
using UnityEngine;
using UnityEngine.Events;

public class RotationManager : MonoBehaviour
{
    UnityEvent onAxisChanged;

    static RotationManager _instance;
    Vector3 _rotationVec;

    public static Vector3 x { get { return new(1, 0, 0); } }
    public static Vector3 y { get { return new(0, 1, 0); } }
    public static Vector3 z { get { return new(0, 0, 1); } }

    public static RotationManager Instance
    {
        get
        {
            if (!_instance)
                _instance = new RotationManager();
            return _instance;
        }
    }

    public Vector3 Rotation { get { return _rotationVec; } }
    public Vector3 Axis
    {
        get
        {
            if (_rotationVec.x == 0) return x;
            if (_rotationVec.y == 0) return y;
            return z;
        }
    }

    public bool InX { get { return Axis == x; } }
    public bool InY { get { return Axis == y; } }
    public bool InZ { get { return Axis == z; } }

    private void Awake()
    {
        onAxisChanged = new UnityEvent();
        _instance = this;
    }

    //public void SetConstraints(RotationConstraints constraints)
    //{
    //    if (constraints.x.active) ChangeRotationAxisToX();
    //    else if (constraints.y.active) ChangeRotationAxisToY();
    //    else ChangeRotationAxisToZ();
    //}

    public void SyncRotatorAxis(Action function)
    {
        onAxisChanged.RemoveAllListeners();
        onAxisChanged.AddListener(new UnityAction(function));
    }

    public void ChangeRotationAxisToX() => ChangeRotationAxis(new(0, 1, 1));
    public void ChangeRotationAxisToY() => ChangeRotationAxis(new(1, 0, 1));
    public void ChangeRotationAxisToZ() => ChangeRotationAxis(new(1, 1, 0));

    public void ChangeRotationAxis(Vector3 vec)
    {
        _rotationVec = vec;
        onAxisChanged.Invoke();
    }

    public Vector3 Right(Transform tr)
    {
            if (InX) return tr.forward * -1;
            return tr.right;
    }
    public Vector3 Up(Transform tr)
    {
            if (InY) return tr.forward * -1;
            return tr.up;
    }
    public Vector3 Forward(Transform tr)
    {
            if (InX) return tr.right;
            if (InY) return tr.up;
            return tr.forward;
    }
}
