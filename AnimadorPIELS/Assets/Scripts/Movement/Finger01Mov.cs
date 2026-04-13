using UnityEngine;
using UnityEngine.InputSystem;

public class Finger01Mov : ArticulationMov
{
    override protected void OnMove()
    {
        if (!RotationManager.Instance.InX) return;

        Vector2 rawPos = Mouse.current.position.value - center;
        Vector2 pos = MousePos(rawPos);
        Vector2 correctedPrev = MousePos(prevPos);
        float angle = GetAngle(correctedPrev, pos);

        if (angle == 0) return;

        float prevRot = GetSignedLocalX();
        float nextRot = prevRot + angle;

        float clampedRot = Mathf.Clamp(nextRot, adjustedConstrains.MinValue, adjustedConstrains.MaxValue);

        SetLocalX(clampedRot);

        rotX = clampedRot;

        prevPos = rawPos;
    }

    override protected void AdjustConstraints()
    {
        adjustedConstrains = Constraints.x;

        float currentRot = GetSignedLocalX();
        float clampedRot = Mathf.Clamp(currentRot, adjustedConstrains.MinValue, adjustedConstrains.MaxValue);

        if (!Mathf.Approximately(currentRot, clampedRot))
            SetLocalX(clampedRot);

        rotX = clampedRot;
    }

    float GetSignedLocalX() => Mathf.DeltaAngle(0f, transform.localEulerAngles.x);

    void SetLocalX(float value)
    {
        Vector3 euler = transform.localEulerAngles;
        euler.x = value < 0f ? value + 360f : value;
        transform.localEulerAngles = euler;
    }
}
