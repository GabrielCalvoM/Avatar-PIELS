using System;
using UnityEngine;
using UnityEngine.Events;

public class RotationManager : MonoBehaviour
{
    UnityEvent onAxisChanged;

    static RotationManager _instance;
    Vector3 _rotationVec;

    /// <summary>
    /// Vector de rotación del eje X
    /// </summary>
    public static Vector3 X { get { return new(1, 0, 0); } }
    /// <summary>
    /// Vector de rotación del eje Y
    /// </summary>
    public static Vector3 Y { get { return new(0, 1, 0); } }
    /// <summary>
    /// Vector de rotación del eje Z
    /// </summary>
    public static Vector3 Z { get { return new(0, 0, 1); } }

    /// <summary>
    /// Instancia global de la clase
    /// </summary>
    public static RotationManager Instance
    {
        get
        {
            if (!_instance)
                _instance = new RotationManager();
            return _instance;
        }
    }

    /// <summary>
    /// Indica el eje actual con un 0 en el eje
    /// </summary>
    public Vector3 Rotation { get { return _rotationVec; } }
    /// <summary>
    /// Eje de rotación actual
    /// </summary>
    public Vector3 Axis
    {
        get
        {
            if (_rotationVec.x == 0) return X;
            if (_rotationVec.y == 0) return Y;
            return Z;
        }
    }

    /// <summary>
    /// Devuelve <c>true</c> si el eje actual es X
    /// </summary>
    public bool InX { get { return Axis == X; } }
    /// <summary>
    /// Devuelve <c>true</c> si el eje actual es Y
    /// </summary>
    public bool InY { get { return Axis == Y; } }
    /// <summary>
    /// Devuelve <c>true</c> si el eje actual es Z
    /// </summary>
    public bool InZ { get { return Axis == Z; } }

    private void Awake()
    {
        onAxisChanged = new UnityEvent();
        _instance = this;
    }

    /// <summary>
    /// Establece la función que se ejecuta al momento de cambiar de eje
    /// </summary>
    public void SyncRotatorAxis(Action function)
    {
        onAxisChanged.RemoveAllListeners();
        onAxisChanged.AddListener(new UnityAction(function));
    }

    /// <summary>
    /// Cambia el eje de rotación actual al eje X
    /// </summary>
    public void ChangeRotationAxisToX() => ChangeRotationAxis(new(0, 1, 1));
    /// <summary>
    /// Cambia el eje de rotación actual al eje Y
    /// </summary>
    public void ChangeRotationAxisToY() => ChangeRotationAxis(new(1, 0, 1));
    /// <summary>
    /// Cambia el eje de rotación actual al eje Z
    /// </summary>
    public void ChangeRotationAxisToZ() => ChangeRotationAxis(new(1, 1, 0));

    private void ChangeRotationAxis(Vector3 vec)
    {
        _rotationVec = vec;
        onAxisChanged.Invoke();
    }

    /// <summary>
    /// Devuelve el vector que apunta hacia la derecha del <c>transform</c> considerando el eje actual como el frente
    /// </summary>
    public Vector3 Right(Transform tr)
    {
            if (InX) return tr.forward * -1;
            return tr.right;
    }
    /// <summary>
    /// Devuelve el vector que apunta hacia arriba del <c>transform</c> considerando el eje actual como el frente
    /// </summary>
    public Vector3 Up(Transform tr)
    {
            if (InY) return tr.forward * -1;
            return tr.up;
    }
    /// <summary>
    /// Devuelve el vector que apunta hacia adelante del <c>transform</c> considerando el eje actual como el frente
    /// </summary>
    public Vector3 Forward(Transform tr)
    {
            if (InX) return tr.right;
            if (InY) return tr.up;
            return tr.forward;
    }
}
