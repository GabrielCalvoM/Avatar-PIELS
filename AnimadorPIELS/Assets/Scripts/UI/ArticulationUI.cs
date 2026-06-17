using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ArticulationUI : MonoBehaviour
{
    [SerializeField] ArticulationMov articulation;
    [SerializeField] GameObject button;

    [SerializeField] UIManager uiManager;
    [SerializeField] Camera cameraRef;

    [SerializeField] GameObject rotatorX;
    [SerializeField] GameObject rotatorY;
    [SerializeField] GameObject rotatorZ;

    [SerializeField] bool X, Y, Z;
    [SerializeField] float scale = 1f;

    Vector3 originalRotation;
    bool moving;
    readonly List<GameObject> spawnedRotators = new();

    Camera ResolveActiveCamera()
    {
        if (cameraRef != null && cameraRef.isActiveAndEnabled) return cameraRef;

        Camera cam = Camera.main;
        if (cam != null && cam.isActiveAndEnabled) return cam;

        int count = Camera.allCamerasCount;
        if (count <= 0) return cameraRef;

        Camera[] cams = new Camera[count];
        int written = Camera.GetAllCameras(cams);
        for (int i = 0; i < written; i++)
        {
            if (cams[i] != null && cams[i].isActiveAndEnabled) return cams[i];
        }

        return cameraRef;
    }

    private void Awake()
    {
        originalRotation = transform.localEulerAngles;
    }

    void Start()
    {
        cameraRef = ResolveActiveCamera();
    }

    void Update()
    {
        cameraRef = ResolveActiveCamera();

        if (!moving)
        {
            if (cameraRef == null) return;

            transform.LookAt(cameraRef.transform);
            transform.Rotate(new(0, 180, 0));
        }
    }

    public void ToggleOff()
    {
        button.GetComponent<Toggle>().isOn = false;
    }

    /// <summary>
    /// Se activa cuando se selecciona/deselecciona un hueso
    /// </summary>
    public void OnButtonToggled()
    {
        bool isOn = button.GetComponent<Toggle>().isOn;
        moving = isOn;

        // foreach (Transform child in transform)
        // {
        //     if (child.gameObject == button) continue;
        //     child.gameObject.SetActive(isOn);
        // }

        if (!isOn)
        {
            ClearRotators();
        }

        Color newColor = button.GetComponent<Image>().color;
        newColor.a = isOn ? 0 : 1;
        button.GetComponent<Image>().color = newColor;

        if (isOn)
        {
            transform.localEulerAngles = originalRotation;
            SpawnRotators();
            uiManager.DisableButtons();
            Toggle toggle = button.GetComponent<Toggle>();

            if (HandFocus.Instance && toggle.group == HandFocus.Instance.FingerGroup)
            {
                HandFocus.Instance.activeUI = this;
            }
        }

        //Debug.Log(isOn);
    }

    void SpawnRotators()
    {
        ClearRotators();

        if (articulation == null) return;

        if (X)
        {
            CreateRotator(rotatorX, true, false, false, Quaternion.Euler(0f, 0f, 90f));
        }

        if (Y)
        {
            CreateRotator(rotatorY, false, true, false, Quaternion.Euler(0f, 0f, 0f));
        }

        if (Z)
        {
            CreateRotator(rotatorZ, false, false, true, Quaternion.Euler(90f, 0f, 0f));
        }
    }

    void CreateRotator(GameObject prefab, bool x, bool y, bool z, Quaternion localRotation)
    {
        if (prefab == null) return;

        GameObject rotatorInstance = Instantiate(prefab, transform);
        rotatorInstance.transform.localPosition = Vector3.zero;
        rotatorInstance.transform.localRotation = localRotation;
        rotatorInstance.transform.localScale = Vector3.one * scale;
        SetLayerRecursively(rotatorInstance, gameObject.layer);

        Rotator rotator = rotatorInstance.GetComponent<Rotator>();
        if (rotator == null) return;

        rotator.ConfigureAxis(x, y, z);
        rotator.pointerDown.AddListener(articulation.OnPointerDown);
        rotator.pointerUp.AddListener(articulation.OnPointerUp);
        rotator.mainCamera = ResolveActiveCamera();
        spawnedRotators.Add(rotatorInstance);
    }

    void SetLayerRecursively(GameObject target, int layer)
    {
        target.layer = layer;

        foreach (Transform child in target.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    void ClearRotators()
    {
        foreach (GameObject rotator in spawnedRotators)
        {
            if (rotator != null)
            {
                Destroy(rotator);
            }
        }

        spawnedRotators.Clear();
    }
}
