using UnityEngine;
using UnityEngine.InputSystem;

public class LoweArmMov : MonoBehaviour
{
    [SerializeField] RotationConstraints constraints;
    [SerializeField] bool mirror;

    Camera cameraRef;
    RotationConstraints.AxisConstraints adjustedConstrains;

    bool presionado = false;
    float rotation;
    Vector2 prevPos;
    Vector2 center;

    void Start()
    {
        cameraRef = Camera.main;
    }

    private void Update()
    {
        if (presionado) OnMove();
    }

    public void OnMove()
    {
        float pitchRad = cameraRef.transform.eulerAngles.x * Mathf.Deg2Rad;
        float ellipseFactor = Mathf.Abs(Mathf.Cos(pitchRad));
        ellipseFactor = Mathf.Max(ellipseFactor, 0.1f);

        Vector2 rawPos = Mouse.current.position.value - center;
        Vector2 pos = new Vector2(rawPos.x, rawPos.y / ellipseFactor);

        Vector2 rawPrev = prevPos;
        Vector2 correctedPrev = new Vector2(rawPrev.x, rawPrev.y / ellipseFactor);

        float angleCos = Vector2.Dot(correctedPrev, pos) / (correctedPrev.magnitude * pos.magnitude);
        angleCos = Mathf.Clamp(angleCos, -1f, 1f);
        float angle = Mathf.Acos(angleCos) * Mathf.Rad2Deg;
        float cruz = correctedPrev.x * pos.y - correctedPrev.y * pos.x;

        float dot = Vector3.Dot(transform.forward, cameraRef.transform.forward);
        float signo = dot > 0f ? 1f : -1f;

        if (cruz > 0) rotation += angle * signo;
        else if (cruz < 0) rotation -= angle * signo;
        else return;

        Vector3 vec = new(transform.eulerAngles.x, transform.eulerAngles.y, rotation);

        if (vec == Vector3.zero) return;
        if (float.IsNaN(vec.x) || float.IsNaN(vec.y) || float.IsNaN(vec.z)) return;
        if (vec.sqrMagnitude <= 0.001f) return;

        transform.eulerAngles = vec;

        prevPos = rawPos;
    }

    public void OnPointerDown()
    {
        presionado = true;
        center = cameraRef.WorldToScreenPoint(transform.position);
        prevPos = Mouse.current.position.value - center;
        rotation = transform.eulerAngles.z;
    }

    public void OnPointerUp()
    {
        presionado = false;
        prevPos = new(0, 0);
        center = new(0, 0);
        rotation = 0;
    }

    
}
