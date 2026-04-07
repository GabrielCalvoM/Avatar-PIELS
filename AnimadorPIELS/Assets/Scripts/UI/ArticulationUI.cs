using UnityEngine;
using UnityEngine.UI;

public class ArticulationUI : MonoBehaviour
{
    [SerializeField] ArticulationMov articulation;
    [SerializeField] GameObject button;

    [SerializeField] Camera cameraRef;

    Vector3 originalRotation;
    bool moving;

    private void Awake()
    {
        originalRotation = transform.localEulerAngles;
    }

    void Start()
    {
        if (cameraRef == null) cameraRef = Camera.main;
    }

    void Update()
    {
        if (!moving)
        {
            transform.LookAt(cameraRef.transform);
            transform.Rotate(new(0, 180, 0));
        }
    }

    public void OnButtonToggled()
    {
        bool isOn = button.GetComponent<Toggle>().isOn;
        moving = isOn;

        if (isOn)
        {
            RotationManager.Instance.SetConstraints(articulation.Constraints);
            transform.localEulerAngles = originalRotation;
        }

        foreach (Transform child in transform)
        {
            if (child.gameObject == button) continue;
            child.gameObject.SetActive(isOn);
        }

        Color newColor = button.GetComponent<Image>().color;
        newColor.a = isOn ? 0 : 1;
        button.GetComponent<Image>().color = newColor;
    }
}
