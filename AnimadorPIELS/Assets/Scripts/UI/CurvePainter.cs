using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CurvePainter : MonoBehaviour
{
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

    void CalculatePointsCant()
    {
        line.positionCount = Mathf.CeilToInt(angle * resolution / 90);
    }

    void CalculatePointsPos()
    {
        Vector3 objPos = transform.position;
        Vector3[] pointsPos = new Vector3[line.positionCount];
        float angleBase = angle * (Mathf.PI / 180) / line.positionCount;

        for (int i = 0; i < line.positionCount; i++)
        {
            float x = Mathf.Cos(angleBase * i) * radius * 0.15f;
            float y = Mathf.Sin(angleBase * i) * radius * 0.15f;
            pointsPos[i] = objPos + new Vector3(x, y, 0);
        }

        line.startColor = Color.red;
        line.endColor = Color.red;

        line.SetPositions(pointsPos);
    }

    void OnEnable()
    {
        CalculatePointsCant();
        CalculatePointsPos();
    }
}
