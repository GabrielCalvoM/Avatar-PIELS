using UnityEngine;
using UnityEngine.InputSystem;

public class WorldCamera : MonoBehaviour
{
    //////////////////////////////////////////////////////////// ATTRIBUTES

    [Header("Avatar Target")]
    [SerializeField] private Transform avatar;

    [SerializeField] private float v_offset = 1.5f;
    [SerializeField] private float orbit_speed = 30.0f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoom_speed = 10.0f;
    [SerializeField] private float start_dist = 5.0f;
    [SerializeField] private float min_zoom = 0.5f;
    [SerializeField] private float max_zoom = 5.0f;

    private float curr_zoom;
    private float h_angle;
    private float v_angle;

    private const float top_bottom_pitch = 89.0f;

    //////////////////////////////////////////////////////////// METHODS

    private void HandleZoom()
    {
        float dir = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Approximately(dir, 0f)) return;

        // Normalize the scroll value — new Input System returns larger raw values
        dir *= 0.01f;

        curr_zoom -= dir * zoom_speed * curr_zoom;
        curr_zoom = Mathf.Clamp(curr_zoom, min_zoom, max_zoom);
    }

    private void HandleOrbit()
    {
        if (!Mouse.current.middleButton.isPressed) return;

        Vector2 delta = Mouse.current.delta.ReadValue();

        h_angle += delta.x * orbit_speed * Time.deltaTime;
        v_angle -= delta.y * orbit_speed * Time.deltaTime;

        v_angle = Mathf.Clamp(v_angle, -80f, 80f);
    }

    private void ApplyTransform()
    {
        Vector3 pivot = avatar.position + Vector3.up * v_offset;

        Quaternion r = Quaternion.Euler(v_angle, h_angle, 0.0f);
        Vector3 z_dist = new Vector3(0.0f, 0.0f, -curr_zoom);

        transform.position = pivot + r * z_dist;
        transform.LookAt(pivot);
    }

    private void SnapToView(float yaw_offset, float pitch)
    {
        if (avatar == null) return;

        h_angle = avatar.eulerAngles.y + yaw_offset;
        v_angle = pitch;

        ApplyTransform();
    }

    public void CenterFrontView()
    {
        SnapToView(180.0f, 0.0f);
    }

    public void CenterTopView()
    {
        SnapToView(180.0f, top_bottom_pitch);
    }

    public void CenterBottomView()
    {
        SnapToView(0.0f, -top_bottom_pitch);
    }

    public void CenterLeftView()
    {
        SnapToView(-90.0f, 0.0f);
    }

    public void CenterRightView()
    {
        SnapToView(90.0f, 0.0f);
    }

    //////////////////////////////////////////////////////////// GAME LOOP

    void Start()
    {
        curr_zoom = start_dist;
        h_angle = transform.eulerAngles.y;
        v_angle = transform.eulerAngles.x;

        if (avatar == null)
            Debug.LogError("NO AVATAR");
    }

    void Update()
    {
        if (avatar == null) return;

        HandleZoom();
        HandleOrbit();
        ApplyTransform();
    }
}