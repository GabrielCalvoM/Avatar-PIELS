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

        float prevRot = 0, nextRot = 0;

        if (RotationManager.Instance.InX)
        {
            prevRot = rotX;
            nextRot = rotX + angle;
        }
        else if (RotationManager.Instance.InY)
        {
            prevRot = rotY;
            nextRot = rotY + angle;
        }

        float clampedRot = Mathf.Clamp(nextRot, adjustedConstrains.MinValue, adjustedConstrains.MaxValue);
        float realAngle = clampedRot - prevRot;

        if (realAngle != 0) transform.Rotate(actualAxis, realAngle);

        if (RotationManager.Instance.InX) rotX = clampedRot;
        else if (RotationManager.Instance.InY) rotY = clampedRot;

        prevPos = rawPos;
    }

    override protected void AdjustConstraints()
    {
        adjustedConstrains = RotationManager.Instance.InX ? Constraints.x : Constraints.y;

        if (RotationManager.Instance.InX)
        {
            if (rotX < adjustedConstrains.MinValue) rotX = adjustedConstrains.MinValue;
            if (rotX > adjustedConstrains.MaxValue) rotX = adjustedConstrains.MaxValue;
        }
        else if (RotationManager.Instance.InY)
        {
            if (rotY < adjustedConstrains.MinValue) rotY = adjustedConstrains.MinValue;
            if (rotY > adjustedConstrains.MaxValue) rotY = adjustedConstrains.MaxValue;
        }
    }
}
