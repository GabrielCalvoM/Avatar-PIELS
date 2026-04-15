using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Rotator : MonoBehaviour
{
    [OnValueChanged("ToAxisX"), BoxGroup("Axis")]
    public bool x = true;
    [OnValueChanged("ToAxisY"), BoxGroup("Axis")]
    public bool y = false;
    [OnValueChanged("ToAxisZ"), BoxGroup("Axis")]
    public bool z = false;

    [SerializeField] UnityEvent pointerDown;
    [SerializeField] UnityEvent pointerUp;

    Action changeAxis;
    bool _pressed = false, _highlighted = false;

    /// <summary>
    /// Devuelve <c>true</c> si se está presionando el objeto respectivo
    /// </summary>
    public bool Pressed { get { return _pressed; } }

    /// <summary>
    /// Devuelve <c>true</c> si el mouse está sobre el objeto respectivo
    /// </summary>
    public bool Highlighted { get { return _highlighted; } }

    private void Start()
    {
        if (x) changeAxis = RotationManager.Instance.ChangeRotationAxisToX;
        if (y) changeAxis = RotationManager.Instance.ChangeRotationAxisToY;
        if (z) changeAxis = RotationManager.Instance.ChangeRotationAxisToZ;
    }

    void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        _highlighted = Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Highlighted)
            {
                OnPointerDown();
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            OnPointerUp();
        }
    }

    /// <summary>
    /// Se activa cuando se agarra el objeto respectivo
    /// </summary>
    private void OnPointerDown()
    {
        _pressed = true;
        changeAxis();
        pointerDown.Invoke();
    }

    /// <summary>
    /// Se activa cuando se suelta el objeto respectivo
    /// </summary>
    private void OnPointerUp()
    {
        _pressed = false;
        pointerUp.Invoke();
    }

    private void ToAxisX()
    {
        y = z = false;
    }

    private void ToAxisY()
    {
        x = z = false;
    }

    private void ToAxisZ()
    {
        x = y = false;
    }
}
