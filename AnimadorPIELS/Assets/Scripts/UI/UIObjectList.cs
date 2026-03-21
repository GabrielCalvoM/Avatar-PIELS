using UnityEngine;

public class UIObjectList : MonoBehaviour
{
    [SerializeField] GameObject[] _objectList;

    public GameObject[] ObjectList { get { return _objectList; } }
}
