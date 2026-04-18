using UnityEngine;
using UnityEngine.InputSystem;

public class Finger01Mov : ArticulationMov
{
    Quaternion initialRotation;

    void Awake()
    {
        initialRotation = transform.localRotation;
    }

    override protected void OnMove()
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

        float nextRot = rotX + angle;
        rotX = Mathf.Clamp(nextRot, adjustedConstrains.MinValue, adjustedConstrains.MaxValue);
        transform.localRotation = initialRotation * Quaternion.AngleAxis(rotX, RotationManager.X);

        prevPos = rawPos;
    }

    override protected void AdjustConstraints()
    {
        adjustedConstrains = Constraints.x;

        rotX = Mathf.Clamp(rotX, adjustedConstrains.MinValue, adjustedConstrains.MaxValue);

        transform.localRotation = initialRotation * Quaternion.AngleAxis(rotX, RotationManager.X);
    }
}
