using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class LoweArmMov : MonoBehaviour
{
    [SerializeField] RotationConstraints constraints;
    [SerializeField] bool mirror;

    Camera cameraRef;
    RotationConstraints.AxisConstraints adjustedConstrains;

    Vector3 right
    {
        get
        {
            if (RotationManager.Instance.Axis == RotationManager.x) return transform.forward * -1;
            return transform.right;
        }
    }
    Vector3 up
    {
        get
        {
            if (RotationManager.Instance.Axis == RotationManager.y) return transform.forward * -1;
            return transform.up;
        }
    }
    Vector3 forward
    {
        get
        {
            if (RotationManager.Instance.Axis == RotationManager.x) return transform.right;
            if (RotationManager.Instance.Axis == RotationManager.y) return transform.up;
            return transform.forward;
        }
    }

    bool presionado = false;
    float rotation;
    Vector2 prevPos;
    Vector2 center;
    SaveLoadPose saveLoadPose;

    void Start()
    {
        cameraRef = Camera.main;
        saveLoadPose = FindFirstObjectByType<SaveLoadPose>();
    }

    private void Update()
    {
        if (presionado) OnMove();
    }

    public void OnMove()
    {
        //float pitchRad = cameraRef.transform.eulerAngles.x * Mathf.Deg2Rad;
        //float ellipseFactor = Mathf.Abs(Mathf.Cos(pitchRad));
        //ellipseFactor = Mathf.Max(ellipseFactor, 0.1f);

        Vector2 rawPos = Mouse.current.position.value - center;
        //Vector2 pos = new Vector2(rawPos.x, rawPos.y / ellipseFactor);
        //Vector2 pos = MousePos(rawPos);
        Vector2 pos = Mouse.current.position.value - center;
        //Vector2 rawPrev = prevPos;
        //Vector2 correctedPrev = new Vector2(rawPrev.x, rawPrev.y / ellipseFactor);
        //Vector2 correctedPrev = MousePos(prevPos);
        Vector2 correctedPrev = prevPos;

        float angleCos = Vector2.Dot(correctedPrev, pos) / (correctedPrev.magnitude * pos.magnitude);
        angleCos = Mathf.Clamp(angleCos, -1f, 1f);
        float angle = Mathf.Acos(angleCos) * Mathf.Rad2Deg;
        float cruz = correctedPrev.x * pos.y - correctedPrev.y * pos.x;

        Vector3 actualAxis = RotationManager.Instance.Axis;

        float dot = Vector3.Dot(transform.up, cameraRef.transform.forward);
        float signo = dot < 0f ? 1f : -1f;

        if (cruz > 0) rotation += angle * signo;
        else if (cruz < 0) rotation -= angle * signo;
        else return;

        //Vector3 vec = actualAxis == RotationManager.x ?
        //    new(rotation, transform.localEulerAngles.y, transform.localEulerAngles.z) :
        //    actualAxis == RotationManager.y ?
        //    new(transform.localEulerAngles.x, rotation, transform.localEulerAngles.z) :
        //    new(transform.localEulerAngles.x, transform.localEulerAngles.y, rotation);
        Vector3 vec = new(transform.localEulerAngles.x, rotation, transform.localEulerAngles.z);

        Debug.Log($"Ángulo: {angle},     Cruz: {cruz}" +
            $"Prev: {correctedPrev},   Actual: {pos}");

        if (vec == Vector3.zero) return;
        if (float.IsNaN(vec.x) || float.IsNaN(vec.y) || float.IsNaN(vec.z)) return;
        if (vec.sqrMagnitude <= 0.001f) return;

        transform.localEulerAngles = vec;

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
        rotation = actualAxis == RotationManager.x ?
            transform.localEulerAngles.x : actualAxis == RotationManager.y ?
            transform.localEulerAngles.y : transform.localEulerAngles.z;
    }

    public void OnPointerUp(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;

        if (pointerData.button != PointerEventData.InputButton.Left) return;

        presionado = false;
        prevPos = new(0, 0);
        center = new(0, 0);
        rotation = 0;
        saveLoadPose?.EndPoseEdit();
    }

    Vector2 MousePos(Vector2 rawPos)
    {
        Vector2 uS = new(
            Vector3.Dot(right, cameraRef.transform.right),
           -Vector3.Dot(right, cameraRef.transform.up)
        );
        Vector2 vS = new Vector2(
            Vector3.Dot(up, cameraRef.transform.right),
           -Vector3.Dot(up, cameraRef.transform.up)
        );

        float det = uS.x * vS.y - vS.x * uS.y;
        if (Mathf.Abs(det) < 0.01f) return new(0, 0);

        return new(
            (vS.y * rawPos.x - vS.x * rawPos.y) / det,
            (-uS.y * rawPos.x + uS.x * rawPos.y) / det
        );
    }
}
