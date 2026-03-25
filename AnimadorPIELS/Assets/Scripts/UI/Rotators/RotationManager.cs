using UnityEngine;

public class RotationManager : MonoBehaviour
{
    [SerializeField] GameObject axisUI;

    [SerializeField] GameObject buttonY;
    [SerializeField] GameObject buttonX;
    [SerializeField] GameObject buttonZ;

    static RotationManager _instance;
    public static RotationManager Instance
    {
        get
        {
            if (!_instance)
                _instance = new RotationManager();
            return _instance;
        }
    }

    public void setConstraints(RotationConstraints constraints)
    {
        buttonX.SetActive(constraints.x.active);
        buttonY.SetActive(constraints.y.active);
        buttonZ.SetActive(constraints.z.active);
    }

}
