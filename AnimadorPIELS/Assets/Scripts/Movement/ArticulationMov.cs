using UnityEngine;
using UnityEngine.InputSystem;

public abstract class ArticulationMov : MonoBehaviour
{
    [SerializeField] RotationConstraints _constraints;
    public RotationConstraints Constraints { get { return _constraints; } } // Restricciones de rotación del hueso
    protected RotationConstraints.AxisConstraints adjustedConstrains;       // Restricciones según el eje

    protected Camera cameraRef; // Cámara actual del usuario

    protected Vector3 Right { get { return RotationManager.Instance.Right(transform); } }
    protected Vector3 Up { get { return RotationManager.Instance.Up(transform); } }
    protected Vector3 Forward { get { return RotationManager.Instance.Forward(transform); } }

    protected bool presionado = false;
    protected float rotX = 0, rotY = 0, rotZ = 0;   // Rotación actual del gizmo local respecto a la inicial

    Vector2 uS, vS;                             // Orientación del hueso según la perspectiva de la cámara
    protected Vector2 initPos, prevPos, center; // Posiciones clave para el movimiento del hueso con el mouse
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

    public void OnPointerDown()
    {
        presionado = true;
        saveLoadPose?.BeginPoseEdit();
        center = cameraRef.WorldToScreenPoint(transform.position);
        prevPos = Mouse.current.position.value - center;
        initPos = prevPos;

        uS = new(
            Vector3.Dot(Right, cameraRef.transform.right),
           -Vector3.Dot(Right, cameraRef.transform.up)
        );                                                  // Calcula el eje horizontal del hueso
        vS = new(
            Vector3.Dot(Up, cameraRef.transform.right),
           -Vector3.Dot(Up, cameraRef.transform.up)
        );                                                  // Calcula el eje vertical del hueso

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
        /// Posición del mouse respecto al hueso según la perspectiva de la cámara

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
