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
        }
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
