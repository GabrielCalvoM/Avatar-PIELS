using UnityEngine;
using UnityEngine.InputSystem;

public class Finger01Mov : ArticulationMov
{
    Quaternion initialRotation;
    bool isRight;

    void Awake()
    {
        initialRotation = transform.localRotation;
        isRight = name.Contains("_R") || name.EndsWith("R");
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
            float min = adjustedConstrains.MinValue, max = adjustedConstrains.MaxValue;
            if (isRight) { float t = min; min = -max; max = -t; }
            float clampedRot = Mathf.Clamp(nextRot, min, max);
            rotX = clampedRot;
        }
        else // InZ
        {
            adjustedConstrains = Constraints.z;

            float nextRot = rotZ + angle;
            float min = adjustedConstrains.MinValue, max = adjustedConstrains.MaxValue;
            if (isRight) { float t = min; min = -max; max = -t; }
            float clampedRot = Mathf.Clamp(nextRot, min, max);
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
            float min = adjustedConstrains.MinValue, max = adjustedConstrains.MaxValue;
            if (isRight) { float t = min; min = -max; max = -t; }
            rotX = Mathf.Clamp(rotX, min, max);
        }
        else if (RotationManager.Instance.InZ)
        {
            adjustedConstrains = Constraints.z;
            float min = adjustedConstrains.MinValue, max = adjustedConstrains.MaxValue;
            if (isRight) { float t = min; min = -max; max = -t; }
            rotZ = Mathf.Clamp(rotZ, min, max);
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
