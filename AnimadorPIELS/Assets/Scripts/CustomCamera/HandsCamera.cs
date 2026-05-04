using UnityEngine;
using UnityEngine.InputSystem;

public class HandsCamera : MonoBehaviour
{
    //////////////////////////////////////////////////////////// ATTRIBUTES

    [Header("Hand Target")]
    [SerializeField] private Transform hand_bone;

    [Header("Framing")]
    [SerializeField] private float local_y_offset = 0.0f;

    [Header("Orbit Settings")]
    [SerializeField] private float orbit_speed = 30.0f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoom_speed = 10.0f;
    [SerializeField] private float min_zoom = 0.1f;
    [SerializeField] private float max_zoom = 2.5f;

    private float curr_zoom;
    private float h_angle;
    private float v_angle;
    private float base_h_angle;
    private float base_v_angle;
    private bool controls_enabled = true;

    private const float pitch_limit = 89.0f;

    //////////////////////////////////////////////////////////// METHODS

    private static float NormalizeSignedAngle(float angle)
    {
        if (angle > 180.0f)
            angle -= 360.0f;

        return angle;
    }

    private Vector3 GetFocusWorld()
    {
        return hand_bone.position + hand_bone.up * local_y_offset;
    }

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

        v_angle = Mathf.Clamp(v_angle, -pitch_limit, pitch_limit);
    }

    private void ApplyTransform()
    {
        Vector3 focus = GetFocusWorld();

        Quaternion local_orbit = Quaternion.Euler(v_angle, h_angle, 0.0f);
        Vector3 local_z_dist = new Vector3(0.0f, 0.0f, -curr_zoom);

        Vector3 orbit_world_offset = hand_bone.rotation * (local_orbit * local_z_dist);
        Vector3 camera_position = focus + orbit_world_offset;

        Vector3 look_dir = (focus - camera_position).normalized;
        Vector3 up_axis = hand_bone.up;

        transform.position = camera_position;
        transform.rotation = Quaternion.LookRotation(look_dir, up_axis);
    }

    private void SnapToView(float yaw_offset, float pitch_offset)
    {
        if (hand_bone == null) return;

        h_angle = base_h_angle + yaw_offset;
        v_angle = Mathf.Clamp(base_v_angle + pitch_offset, -pitch_limit, pitch_limit);

        ApplyTransform();
    }

    private void InitializeFromCurrentTransform()
    {
        Vector3 focus = GetFocusWorld();
        Vector3 world_from_focus = transform.position - focus;

        curr_zoom = Mathf.Clamp(world_from_focus.magnitude, min_zoom, max_zoom);
        if (curr_zoom <= Mathf.Epsilon)
            curr_zoom = min_zoom;

        Vector3 local_from_focus = Quaternion.Inverse(hand_bone.rotation) * world_from_focus;
        Quaternion local_look = Quaternion.LookRotation(-local_from_focus.normalized, Vector3.up);
        Vector3 local_euler = local_look.eulerAngles;

        h_angle = NormalizeSignedAngle(local_euler.y);
        v_angle = Mathf.Clamp(NormalizeSignedAngle(local_euler.x), -pitch_limit, pitch_limit);

        base_h_angle = h_angle;
        base_v_angle = v_angle;
    }

    public void CenterFrontView()
    {
        SnapToView(0.0f, 0.0f);
    }

    public void CenterTopView()
    {
        SnapToView(0.0f, pitch_limit);
    }

    public void CenterBottomView()
    {
        SnapToView(0.0f, -pitch_limit);
    }

    public void CenterLeftView()
    {
        SnapToView(90.0f, 0.0f);
    }

    public void CenterRightView()
    {
        SnapToView(-90.0f, 0.0f);
    }

    public void StopCameraControls()
    {
        controls_enabled = false;
    }

    public void ResumeCameraControls()
    {
        controls_enabled = true;
    }

    //////////////////////////////////////////////////////////// GAME LOOP

    private void Start()
    {
        if (hand_bone == null)
            hand_bone = transform.parent;

        if (hand_bone == null)
        {
            Debug.LogError("HandsCamera requires a hand bone reference or to be parented to the hand bone.");
            enabled = false;
            return;
        }

        InitializeFromCurrentTransform();
        ApplyTransform();
    }

    private void Update()
    {
        if (hand_bone == null) return;

        if (controls_enabled)
        {
            HandleZoom();
            HandleOrbit();
        }

        ApplyTransform();
    }

}
