using UnityEngine;

public class WorldCamera : MonoBehaviour
{
    //////////////////////////////////////////////////////////// ATTRIBUTES
    
    [Header("Avatar Target")]
    [SerializeField] private Transform avatar;
    [SerializeField] private float orbit_speed = 5.0f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoom_speed = 2.0f;
    [SerializeField] private float start_dist = 10.0f;
    [SerializeField] private float min_zoom = 2.0f;
    [SerializeField] private float max_zoom = 50.0f;
    
    private float curr_zoom;
    private float h_angle;
    private float v_angle;

    //////////////////////////////////////////////////////////// METHODS
    
    private void HandleZoom() {
        float dir = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Approximately(dir, 0f)) return; 

        curr_zoom -= dir * zoom_speed * curr_zoom;
        curr_zoom = Mathf.Clamp(curr_zoom, min_zoom, max_zoom);
    }

    private void HandleOrbit() {
        if (!Input.GetMouseButton(2)) return;

        float mouse_x = Input.GetAxis("Mouse X");
        float mouse_y = Input.GetAxis("Mouse Y"); 

        h_angle += mouse_x * orbit_speed;
        v_angle -= mouse_y * orbit_speed;

        v_angle = Mathf.Clamp(v_angle, -80f, 80f);
    }

    private void ApplyTransform() {
        Quaternion r = Quaternion.Euler(v_angle, h_angle, 0.0f);
        Vector3 z_dist = new Vector3(0.0f, 0.0f, -curr_zoom);

        transform.position = avatar.position + r * z_dist;
        transform.LookAt(avatar.position);
    }


    //////////////////////////////////////////////////////////// GAME LOOP
    
    void Start() {
        curr_zoom = start_dist;
        h_angle = transform.eulerAngles.y;
        v_angle = transform.eulerAngles.x;

        if (avatar == null)
            Debug.LogError("NO AVATAR");
    }

    void Update() {
        if (avatar == null)
            Debug.LogError("NO AVATAR");

        HandleZoom();
        HandleOrbit();
        ApplyTransform();
    }
}
