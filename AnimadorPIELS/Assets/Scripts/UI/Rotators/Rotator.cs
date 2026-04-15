using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Rotator : MonoBehaviour
{
    [OnValueChanged("ToAxisX"), BoxGroup("Axis"), HideInInspector]
    public bool x = true;
    [OnValueChanged("ToAxisY"), BoxGroup("Axis"), HideInInspector]
    public bool y = false;
    [OnValueChanged("ToAxisZ"), BoxGroup("Axis"), HideInInspector]
    public bool z = false;

    public UnityEvent pointerDown;
    public UnityEvent pointerUp;

    Action changeAxis;
    bool _pressed = false, _highlighted = false;

    /// <summary>
    /// Devuelve <c>true</c> si se est� presionando el objeto respectivo
    /// </summary>
    public bool Pressed { get { return _pressed; } }

    /// <summary>
    /// Devuelve <c>true</c> si el mouse est� sobre el objeto respectivo
    /// </summary>
    public bool Highlighted { get { return _highlighted; } }

    private void Start()
    {
        RefreshAxis();
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
        RefreshAxis();
    }

    private void ToAxisY()
    {
        x = z = false;
        RefreshAxis();
    }

    private void ToAxisZ()
    {
        x = y = false;
        RefreshAxis();
    }

    public void ConfigureAxis(bool axisX, bool axisY, bool axisZ)
    {
        x = axisX;
        y = axisY;
        z = axisZ;
        RefreshAxis();
    }

    void RefreshAxis()
    {
        if (x) changeAxis = RotationManager.Instance.ChangeRotationAxisToX;
        if (y) changeAxis = RotationManager.Instance.ChangeRotationAxisToY;
        if (z) changeAxis = RotationManager.Instance.ChangeRotationAxisToZ;
    }
}
