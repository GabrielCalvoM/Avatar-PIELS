using UnityEngine;
using UnityEngine.InputSystem;

public class ThumbMov : ArticulationMov
{
    Quaternion initialRotation;

    void Awake()
    {
        initialRotation = transform.localRotation;
    }

    override protected void OnMove()
    {
        if (!RotationManager.Instance.InX && !RotationManager.Instance.InZ) return;

        Vector2 rawPos = Mouse.current.position.value - center;
        Vector2 pos = MousePos(rawPos);
        Vector2 correctedPrev = MousePos(prevPos);
        float angle = GetAngle(correctedPrev, pos);

        if (Mathf.Approximately(angle, 0f))
        {
            prevPos = rawPos;
            return;
        }

        if (RotationManager.Instance.InX)
        {
            adjustedConstrains = Constraints.x;

            float nextRot = rotX + angle;
            float clampedRot = Mathf.Clamp(nextRot, adjustedConstrains.MinValue, adjustedConstrains.MaxValue);
            rotX = clampedRot;
        }
        else // InZ
        {
            adjustedConstrains = Constraints.z;

            float nextRot = rotZ + angle;
            float clampedRot = Mathf.Clamp(nextRot, adjustedConstrains.MinValue, adjustedConstrains.MaxValue);
            rotZ = clampedRot;
        }

        ApplyRotation();
        prevPos = rawPos;
    }

    override protected void AdjustConstraints()
    {
        if (RotationManager.Instance.InX)
        {
            adjustedConstrains = Constraints.x;
            rotX = Mathf.Clamp(rotX, adjustedConstrains.MinValue, adjustedConstrains.MaxValue);
        }
        else if (RotationManager.Instance.InZ)
        {
            adjustedConstrains = Constraints.z;
            rotZ = Mathf.Clamp(rotZ, adjustedConstrains.MinValue, adjustedConstrains.MaxValue);
        }

        ApplyRotation();
    }

    void ApplyRotation()
    {
        transform.localRotation = initialRotation
            * Quaternion.AngleAxis(rotX, RotationManager.X)
            * Quaternion.AngleAxis(rotZ, RotationManager.Z);
    }
}
