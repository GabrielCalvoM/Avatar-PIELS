using UnityEngine;
using UnityEngine.InputSystem;

public class ThumbMov : ArticulationMov
{
    override protected void OnMove()
    {
        if (!RotationManager.Instance.InX && !RotationManager.Instance.InZ) return;

        Vector2 rawPos = Mouse.current.position.value - center;
        Vector2 pos = MousePos(rawPos);
        Vector2 correctedPrev = MousePos(prevPos);
        float angle = GetAngle(correctedPrev, pos);

        if (angle == 0) return;

        if (RotationManager.Instance.InX)
        {
            adjustedConstrains = Constraints.x;

            float prevRot = GetSignedLocalX();
            float nextRot = prevRot + angle;
            float clampedRot = Mathf.Clamp(nextRot, adjustedConstrains.MinValue, adjustedConstrains.MaxValue);

            SetLocalX(clampedRot);
            rotX = clampedRot;
        }
        else if (RotationManager.Instance.InZ)
        {
            adjustedConstrains = Constraints.z;

            float prevRot = GetSignedLocalZ();
            float nextRot = prevRot + angle;
            float clampedRot = Mathf.Clamp(nextRot, adjustedConstrains.MinValue, adjustedConstrains.MaxValue);

            SetLocalZ(clampedRot);
            rotZ = clampedRot;
        }

        prevPos = rawPos;
    }

    override protected void AdjustConstraints()
    {
        if (RotationManager.Instance.InX)
        {
            adjustedConstrains = Constraints.x;

            float currentRot = GetSignedLocalX();
            float clampedRot = Mathf.Clamp(currentRot, adjustedConstrains.MinValue, adjustedConstrains.MaxValue);

            if (!Mathf.Approximately(currentRot, clampedRot))
                SetLocalX(clampedRot);

            rotX = clampedRot;
        }
        else if (RotationManager.Instance.InZ)
        {
            adjustedConstrains = Constraints.z;

            float currentRot = GetSignedLocalZ();
            float clampedRot = Mathf.Clamp(currentRot, adjustedConstrains.MinValue, adjustedConstrains.MaxValue);

            if (!Mathf.Approximately(currentRot, clampedRot))
                SetLocalZ(clampedRot);

            rotZ = clampedRot;
        }
    }

    float GetSignedLocalX() => Mathf.DeltaAngle(0f, transform.localEulerAngles.x);
    float GetSignedLocalZ() => Mathf.DeltaAngle(0f, transform.localEulerAngles.z);

    void SetLocalX(float value)
    {
        Vector3 euler = transform.localEulerAngles;
        euler.x = ToEulerAngle(value);
        transform.localEulerAngles = euler;
    }

    void SetLocalZ(float value)
    {
        Vector3 euler = transform.localEulerAngles;
        euler.z = ToEulerAngle(value);
        transform.localEulerAngles = euler;
    }

    float ToEulerAngle(float signedAngle)
    {
        return signedAngle < 0f ? signedAngle + 360f : signedAngle;
    }
}
