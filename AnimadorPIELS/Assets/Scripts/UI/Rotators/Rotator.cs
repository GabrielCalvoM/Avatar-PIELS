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
    bool pressed = false, highlighted = false;

    private void Start()
    {
        if (x) changeAxis = RotationManager.Instance.ChangeRotationAxisToX;
        if (y) changeAxis = RotationManager.Instance.ChangeRotationAxisToY;
        if (z) changeAxis = RotationManager.Instance.ChangeRotationAxisToZ;
    }

    void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        highlighted = Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (highlighted)
            {
                OnPointerDown();
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            OnPointerUp();
        }
    }

    private void OnPointerDown()
    {
        pressed = true;
        changeAxis();
        pointerDown.Invoke();
    }

    private void OnPointerUp()
    {
        pressed = false;
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

    //private void OnEnable()
    //{
    //    RotationManager.Instance.SetConstraints(constraints);
    //}
}
