using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

public abstract class ArticulationMov : MonoBehaviour
{
    [SerializeField] RotationConstraints _constraints;
    public RotationConstraints Constraints { get { return _constraints; } }

    protected Camera cameraRef;
    protected RotationConstraints.AxisConstraints adjustedConstrains;

    protected Vector3 Right { get { return RotationManager.Instance.Right(transform); } }
    protected Vector3 Up { get { return RotationManager.Instance.Up(transform); } }
    protected Vector3 Forward { get { return RotationManager.Instance.Forward(transform); } }

    protected bool presionado = false;
    protected float rotX = 0, rotY = 0, rotZ = 0;
    protected float angleX = 0, angleY = 0, angleZ = 0;
    protected Vector2 initPos, prevPos, center;
    Vector2 uS, vS;
    protected SaveLoadPose saveLoadPose;

    protected void Start()
    {
        cameraRef = Camera.main;
        saveLoadPose = FindFirstObjectByType<SaveLoadPose>();
    }

    protected void Update()
    {
        if (presionado) OnMove();
    }

    protected abstract void OnMove();

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

    public void OnPointerDown()
    {
        Vector3 actualAxis = RotationManager.Instance.Axis;
        presionado = true;
        saveLoadPose?.BeginPoseEdit();
        center = cameraRef.WorldToScreenPoint(transform.position);
        prevPos = Mouse.current.position.value - center;
        initPos = prevPos;

        uS = new(
            Vector3.Dot(Right, cameraRef.transform.right),
           -Vector3.Dot(Right, cameraRef.transform.up)
        );
        vS = new(
            Vector3.Dot(Up, cameraRef.transform.right),
           -Vector3.Dot(Up, cameraRef.transform.up)
        );

        AdjustConstraints();
    }

    public void OnPointerUp()
    {
        presionado = false;
        prevPos = new(0, 0);
        center = new(0, 0);
        saveLoadPose?.EndPoseEdit();
    }

    protected Vector2 MousePos(Vector2 rawPos)
    {
        float det = uS.x * vS.y - vS.x * uS.y;
        if (Mathf.Abs(det) < 0.01f) return new(0, 0);

        return new(
            (vS.y * rawPos.x - vS.x * rawPos.y) / det,
            (-uS.y * rawPos.x + uS.x * rawPos.y) / det
        );
    }

    protected float GetAngle(Vector2 origin, Vector2 destiny)
    {
        if (origin.magnitude < 0.01f || destiny.magnitude < 0.01f) return 0f;

        float angleCos = Vector2.Dot(origin, destiny) / (origin.magnitude * destiny.magnitude);
        angleCos = Mathf.Clamp(angleCos, -1f, 1f);
        float rawAngle = Mathf.Acos(angleCos) * Mathf.Rad2Deg;
        float cruz = origin.x * destiny.y - origin.y * destiny.x;

        if (cruz > 0) return -rawAngle;
        else if (cruz < 0) return rawAngle;
        else return 0;
    }

    protected abstract void AdjustConstraints();
}
