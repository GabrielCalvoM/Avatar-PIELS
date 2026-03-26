using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] RotationConstraints constraints;

    private void OnEnable()
    {
        RotationManager.Instance.SetConstraints(constraints);
    }
}
