using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RotationManager : MonoBehaviour
{
    [SerializeField] GameObject axisUI;

    [SerializeField] GameObject buttonX;
    [SerializeField] GameObject buttonY;
    [SerializeField] GameObject buttonZ;

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

    private void Awake()
    {
        onAxisChanged = new UnityEvent();
    }

    void Start()
    {
        if (!_instance) _instance = this;
    }

    public void SetConstraints(RotationConstraints constraints)
    {
        buttonX.GetComponent<Button>().enabled = constraints.x.active;
        buttonY.GetComponent<Button>().enabled = constraints.y.active;
        buttonZ.GetComponent<Button>().enabled = constraints.z.active;

        if (constraints.x.active) ChangeRotationAxisToX();
        else if (constraints.y.active) ChangeRotationAxisToY();
        else ChangeRotationAxisToZ();
    }

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
}
