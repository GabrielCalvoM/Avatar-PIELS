using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] Camera cameraRef;

    void Start()
    {
        if (cameraRef == null) cameraRef = Camera.main;
    }

    void Update()
    {
        transform.LookAt(cameraRef.transform);
        transform.Rotate(new(0, 180, 0));
    }
}
