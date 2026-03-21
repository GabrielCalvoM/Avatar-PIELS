using UnityEngine;
using UnityEngine.InputSystem;

public class LoweArmMov : MonoBehaviour
{
    [SerializeField] GameObject rotator;
    [SerializeField] Camera cameraRef;
    [SerializeField] UIObjectList rotatorList;

    bool presionado = false;
    float rotation;
    Vector2 prevPos;
    Vector2 center;

    void Start()
    {
        if (cameraRef == null) cameraRef = Camera.main;
    }

    private void Update()
    {
        if (presionado) OnMove();
    }

    public void OnButtonPressed()
    {
        foreach (var rotator in rotatorList.ObjectList)
        {
            rotator.SetActive(false);
        }

        rotator.SetActive(true);
    }

    public void OnMove()
    {
        if (presionado)
        {
            Vector2 pos = Mouse.current.position.value - center;
            float angleCos = Vector2.Dot(prevPos, pos) / (prevPos.magnitude * pos.magnitude);
            float angle = Mathf.Acos(angleCos) * 180 / Mathf.PI;
            float cruz = prevPos.x * pos.y - prevPos.y * pos.x;

            if (cruz > 0) rotation += angle;
            else if (cruz < 0) rotation -= angle;
            else return;

            transform.eulerAngles = new(transform.eulerAngles.x, transform.eulerAngles.y, rotation);

            prevPos = pos;
        }
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
