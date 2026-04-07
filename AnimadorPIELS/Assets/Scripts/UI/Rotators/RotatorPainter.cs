using UnityEngine;

public class RotatorPainter : MonoBehaviour
{
    [SerializeField] RotatorColors colors;
    [SerializeField] Rotator rotator;
    Material _material;

    void Start()
    {
        _material = GetComponent<Renderer>().material;

        _material.SetColor("_IdleColor", colors.idle);
        _material.SetColor("_HighlightedColor", colors.highlighted);
        _material.SetColor("_PressedColor", colors.pressed);
    }

    private void Update()
    {
        _material.SetFloat("_Pressed", rotator.Pressed ? 1f : 0f);
        _material.SetFloat("_Highlighted", rotator.Highlighted ? 1f : 0f);
    }

    private void OnDestroy()
    {
        Destroy(_material);
    }
}
