using UnityEngine;
using UnityEngine.InputSystem;

public class LoweArmMov : ArticulationMov
{
    override protected void OnMove()
    {
        Vector2 rawPos = Mouse.current.position.value - center;
        Vector2 pos = MousePos(rawPos);
        Vector2 correctedPrev = MousePos(prevPos);
        Vector3 actualAxis = RotationManager.Instance.Axis;

        float angle = GetAngle(correctedPrev, pos);

        if (angle == 0) return;

        float prevRot = rotX;
        float nextRot = rotX + angle;

        float clampedRot = Mathf.Clamp(nextRot, adjustedConstrains.MinValue, adjustedConstrains.MaxValue);
        float realAngle = clampedRot - prevRot;

        if (realAngle != 0) transform.Rotate(actualAxis, realAngle);

        rotX = clampedRot;
        prevPos = rawPos;
    }

    override protected void AdjustConstraints()
    {
        adjustedConstrains = Constraints.x;
        rotX = Mathf.Clamp(rotX, adjustedConstrains.MinValue, adjustedConstrains.MaxValue);
    }
}
