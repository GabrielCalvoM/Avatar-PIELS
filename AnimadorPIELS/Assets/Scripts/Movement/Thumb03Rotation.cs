using UnityEngine;
using UnityEngine.InputSystem;

public class Thumb03Rotation : ArticulationMov
{
    const float MinBend = 0f;
    const float MaxBend = 90f;

    Quaternion initialRotation;

    void Awake()
    {
        initialRotation = transform.localRotation;
    }

    protected override void OnMove()
    {
        if (!RotationManager.Instance.InX) return;

        Vector2 rawPos = Mouse.current.position.value - center;
        Vector2 pos = MousePos(rawPos);
        Vector2 correctedPrev = MousePos(prevPos);
        float angle = GetAngle(correctedPrev, pos);

        if (Mathf.Approximately(angle, 0f))
        {
            prevPos = rawPos;
            return;
        }

        // Keep drag direction consistent across hands.
        rotX = Mathf.Clamp(rotX - angle, MinBend, MaxBend);
        ApplyRotation();

        prevPos = rawPos;
    }

    protected override void AdjustConstraints()
    {
        // Even if a RotationConstraints asset is present, this bone is hard-limited to 0..90 on X.
        rotX = Mathf.Clamp(rotX, MinBend, MaxBend);
        ApplyRotation();
    }

    void ApplyRotation()
    {
        // Apply as a bend (negative around X) to match the curling direction.
        transform.localRotation = initialRotation * Quaternion.AngleAxis(-rotX, RotationManager.X);
    }
}
