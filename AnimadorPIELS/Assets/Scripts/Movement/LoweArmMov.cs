using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class LoweArmMov : MonoBehaviour
{
    [SerializeField] RotationConstraints constraints;
    [SerializeField] bool mirror;

    Camera cameraRef;
    RotationConstraints.AxisConstraints adjustedConstrains;

    Vector3 Right { get { return RotationManager.Instance.Right(transform); } }
    Vector3 Up { get { return RotationManager.Instance.Up(transform); } }
    Vector3 Forward { get { return RotationManager.Instance.Forward(transform); } }

    bool presionado = false;
    float rotX, rotY;
    Vector2 prevPos;
    Vector2 center;
    SaveLoadPose saveLoadPose;


    void Start()
    {
        cameraRef = Camera.main;
        saveLoadPose = FindFirstObjectByType<SaveLoadPose>();

        rotX = 0;
        rotY = 0;
    }

    private void Update()
    {
        if (presionado) OnMove();
    }

    public void OnMove()
    {
        Vector2 rawPos = Mouse.current.position.value - center;
        Vector2 pos = MousePos(rawPos);
        Vector2 correctedPrev = MousePos(prevPos);

        float angleCos = Vector2.Dot(correctedPrev, pos) / (correctedPrev.magnitude * pos.magnitude);
        angleCos = Mathf.Clamp(angleCos, -1f, 1f);
        float rawAngle = Mathf.Acos(angleCos) * Mathf.Rad2Deg;
        float cruz = correctedPrev.x * pos.y - correctedPrev.y * pos.x;

        Vector3 actualAxis = RotationManager.Instance.Axis;

        float dot = Vector3.Dot(Forward, cameraRef.transform.forward);
        float signo = dot < 0f ? 1f : -1f;
        float angle;

        if (cruz > 0) angle = rawAngle * signo;
        else if (cruz < 0) angle = -rawAngle * signo;
        else return;

        float prevRot = 0, actualRot = 0;

        if (RotationManager.Instance.InX)
        {
            prevRot = rotX;
            rotX += angle;
            actualRot = rotX;
        }
        else if (RotationManager.Instance.InY)
        {
            prevRot = rotY;
            rotY += angle;
            actualRot = rotY;
        }

        if (prevRot < adjustedConstrains.MinValue || prevRot > adjustedConstrains.MaxValue)
        {
            Debug.Log("A");
        }
        else if (actualRot < adjustedConstrains.MinValue || actualRot > adjustedConstrains.MaxValue)
        {
            Debug.Log("B");
            if (actualRot < adjustedConstrains.MinValue) transform.Rotate(actualAxis, adjustedConstrains.MinValue - prevRot);
            if (actualRot > adjustedConstrains.MaxValue) transform.Rotate(actualAxis, adjustedConstrains.MaxValue - prevRot);
        }
        else
        {
            transform.Rotate(actualAxis, angle);
        }

        prevPos = rawPos;
    }

    public void OnPointerDown(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;

        if (pointerData.button != PointerEventData.InputButton.Left) return;

        Vector3 actualAxis = RotationManager.Instance.Axis;
        presionado = true;
        saveLoadPose?.BeginPoseEdit();
        center = cameraRef.WorldToScreenPoint(transform.position);
        prevPos = Mouse.current.position.value - center;

        AdjustConstraints();
    }

    public void OnPointerUp(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;

        if (pointerData.button != PointerEventData.InputButton.Left) return;

        presionado = false;
        prevPos = new(0, 0);
        center = new(0, 0);
        saveLoadPose?.EndPoseEdit();
    }

    Vector2 MousePos(Vector2 rawPos)
    {
        Vector2 uS = new(
            Vector3.Dot(Right, cameraRef.transform.right),
           -Vector3.Dot(Right, cameraRef.transform.up)
        );
        Vector2 vS = new(
            Vector3.Dot(Up, cameraRef.transform.right),
           -Vector3.Dot(Up, cameraRef.transform.up)
        );

        float det = uS.x * vS.y - vS.x * uS.y;
        if (Mathf.Abs(det) < 0.01f) return new(0, 0);

        return new(
            (vS.y * rawPos.x - vS.x * rawPos.y) / det,
            (-uS.y * rawPos.x + uS.x * rawPos.y) / det
        );
    }

    void AdjustConstraints()
    {
        adjustedConstrains = constraints.AdjustConstraints(RotationManager.Instance.InX ? constraints.x : constraints.y, mirror);

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
