using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CurvePainter : MonoBehaviour
{
    [SerializeField] Transform center;
    [SerializeField] float radius = 1;
    [SerializeField] float width = 1;
    [SerializeField] float resolution = 10;
    [SerializeField] float angle = 360;

    LineRenderer line;
    Vector3 iniRotation;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        iniRotation = transform.eulerAngles;
    }

    public void OnPointerDown(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;

        if (pointerData.button != PointerEventData.InputButton.Left) return;

        line.startColor = Color.yellow;
        line.endColor = Color.yellow;
    }

    public void OnPointerUp(BaseEventData data)
    {
        PointerEventData pointerData = (PointerEventData)data;

        if (pointerData.button != PointerEventData.InputButton.Left) return;

        line.startColor = Color.red;
        line.endColor = Color.red;
    }

    public void OnPointerDown()
    {
        line.startColor = Color.yellow;
        line.endColor = Color.yellow;
    }

    public void OnPointerUp()
    {
        line.startColor = Color.red;
        line.endColor = Color.red;
    }

    public void CalculateCurve()
    {
        CalculatePointsCant();
        CalculatePointsPos();
    }

    void CalculatePointsCant()
    {
        line.positionCount = Mathf.CeilToInt(angle * resolution / 90);
        line.widthMultiplier *= width;
    }

    void CalculatePointsPos()
    {
        Vector3 objPos = center.position;
        Vector3[] pointsPos = new Vector3[line.positionCount];
        float angleBase = angle * Mathf.Deg2Rad / line.positionCount;

        for (int i = 0; i < line.positionCount; i++)
        {
            float x2D = Mathf.Cos(angleBase * i) * radius * 0.15f;
            float y2D = Mathf.Sin(angleBase * i) * radius * 0.15f;

            Vector3 actualAxis = RotationManager.Instance.Axis;
            Vector3 vec = actualAxis == RotationManager.x ?
                new(0, x2D, y2D) : actualAxis == RotationManager.y ?
                new(x2D, 0, y2D) : new(x2D, y2D, 0);

            pointsPos[i] = objPos + (center.rotation * vec);
        }

        line.startColor = Color.red;
        line.endColor = Color.red;

        line.SetPositions(pointsPos);
    }

    void OnEnable()
    {
        RotationManager.Instance.SyncRotatorAxis(CalculateCurve);
        //CalculateCurve();
    }
}
