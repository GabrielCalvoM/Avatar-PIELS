using UnityEngine;
using UnityEngine.InputSystem;

public abstract class ArticulationMov : MonoBehaviour
{
    [SerializeField] RotationConstraints _constraints;
    public RotationConstraints Constraints { get { return _constraints; } } // Restricciones de rotaci�n del hueso
    protected RotationConstraints.AxisConstraints adjustedConstrains;       // Restricciones seg�n el eje

    protected Camera cameraRef; // C�mara actual del usuario

    // Ejes relativos para la rotaci�n en X, Y y Z
    protected Vector3 Right { get { return RotationManager.Instance.Right(transform); } }
    protected Vector3 Up { get { return RotationManager.Instance.Up(transform); } }
    protected Vector3 Forward { get { return RotationManager.Instance.Forward(transform); } }


    protected bool presionado = false;
    protected float rotX = 0, rotY = 0, rotZ = 0;
    protected float ActualRotation
    {
        get
        {
            if (RotationManager.Instance.InX) return rotX;
            else if (RotationManager.Instance.InY) return rotY;
            else return rotZ;
        }
    }

    Vector2 uS, vS;                             // Orientaci�n del hueso seg�n la perspectiva de la c�mara
    protected Vector2 initPos, prevPos, center; // Posiciones clave para el movimiento del hueso con el mouse
    protected SaveLoadPose saveLoadPose;

    protected Camera ResolveActiveCamera()
    {
        if (cameraRef != null && cameraRef.isActiveAndEnabled) return cameraRef;

        Camera cam = Camera.main;
        if (cam != null && cam.isActiveAndEnabled) return cam;

        int count = Camera.allCamerasCount;
        if (count <= 0) return cameraRef;

        Camera[] cams = new Camera[count];
        int written = Camera.GetAllCameras(cams);
        for (int i = 0; i < written; i++)
        {
            if (cams[i] != null && cams[i].isActiveAndEnabled) return cams[i];
        }

        return cameraRef;
    }

    protected void Start()
    {
        cameraRef = ResolveActiveCamera();
        saveLoadPose = FindFirstObjectByType<SaveLoadPose>();
    }

    protected void Update()
    {
        if (presionado) OnMove();
    }

    /// <summary>
    /// Calcula la rotaci�n del hueso seg�n el eje deseado
    /// </summary>
    protected virtual void OnMove()
    {
        Vector2 rawPos = Mouse.current.position.value - center;
        Vector2 pos = MousePos(rawPos);
        Vector2 correctedPrev = MousePos(prevPos);
        Vector3 actualAxis = RotationManager.Instance.Axis;
        float angle = GetAngle(correctedPrev, pos);

        if (angle == 0) return;

        transform.Rotate(actualAxis, angle);
        prevPos = rawPos;
    }

    /// <summary>
    /// Se activa cuando se agarra un objeto rotador del hueso
    /// </summary>
    public void OnPointerDown()
    {
        presionado = true;
        saveLoadPose?.BeginPoseEdit();

        cameraRef = ResolveActiveCamera();
        if (cameraRef == null)
        {
            presionado = false;
            saveLoadPose?.EndPoseEdit();
            return;
        }

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

    /// <summary>
    /// Se activa cuando se suelta un objeto rotador del hueso
    /// </summary>
    public void OnPointerUp()
    {
        presionado = false;
        prevPos = new(0, 0);
        center = new(0, 0);
        saveLoadPose?.EndPoseEdit();
    }

    /// <summary>
    /// Calcula la posici�n del mouse respecto al hueso seg�n la perspectiva de la c�mara
    /// </summary>
    /// <param name="rawPos">Posici�n real del mouse en la pantalla</param>
    protected Vector2 MousePos(Vector2 rawPos)
    {
        float det = uS.x * vS.y - vS.x * uS.y;
        if (Mathf.Abs(det) < 0.01f) return new(0, 0);

        return new(
            (vS.y * rawPos.x - vS.x * rawPos.y) / det,
            (-uS.y * rawPos.x + uS.x * rawPos.y) / det
        );
    }

    /// <summary>
    /// Calcula el �ngulo de rotaci�n respecto al movimiento del mouse
    /// </summary>
    /// <param name="origin">Posici�n anterior registrada del mouse con centro en el hueso</param>
    /// <param name="destiny">Posici�n actual del mouse con centro en el hueso</param>
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

    /// <summary>
    /// Ajusta las restricciones respectivas del movimiento de los ejes
    /// </summary>
    protected virtual void AdjustConstraints()
    {
        if (RotationManager.Instance.InX)
        {
            adjustedConstrains = Constraints.x;
        }
        else if (RotationManager.Instance.InY)
        {
            adjustedConstrains = Constraints.y;
        }
        else
        {
            adjustedConstrains = Constraints.z;
        }
    }
}
