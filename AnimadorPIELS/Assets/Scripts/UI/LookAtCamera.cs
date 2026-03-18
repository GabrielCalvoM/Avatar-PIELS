using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] Camera cameraRef;

    void Start()
    {
        if (cameraRef == null) cameraRef = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(cameraRef.transform);
    }
}
