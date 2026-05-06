using System;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HandFocus : MonoBehaviour
{
    ////////////////////////////////////////////////////////////////////////////////// ATTRIBUTES

    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject focusCamera;
    [SerializeField] private GameObject cameraControls;
    [SerializeField] private GameObject handsUI;
    [SerializeField] UIManager uiManager;
    [SerializeField] Button returnButton;
    [SerializeField] ToggleGroup _fingerGroup;

    public ArticulationUI activeUI;
    public ToggleGroup FingerGroup { get { return _fingerGroup; } }

    private static HandFocus _instance;
    public static HandFocus Instance { get { return _instance; } }

    private void FetchButtons() 
    {
        Transform returnTransform = handsUI.transform.Find("ReturnButton");
        if (returnTransform != null)
            returnButton = returnTransform.GetComponent<Button>();
        else
            Debug.LogError("Return button not found in handsUI!");
        
        for (int i = 0; i < fingers.Count; i++)
        {
            Transform t = DeepFind(handsUI.transform, fingers[i].fingerTag);
            if (t != null)
                fingers[i].fingerButton = t.GetComponent<Button>();
            else
                Debug.LogWarning("Dead");
        }
    }

    private Transform DeepFind(Transform root, string name)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            if (child.name == name) return child;
        return null;
    }

    ////////////////////////////////////////////////////////////////////////////////// MAIN FLOW
   
    public void OnHandFocusButtonPressed()
    {
        _instance = this;
        mainCamera.SetActive(false);
        focusCamera.SetActive(true);
        if (uiManager != null) uiManager.UseFocusCamera(focusCamera);
        //cameraControls.SetActive(false);
        handsUI.SetActive(true);

        returnButton.onClick.RemoveAllListeners();
        returnButton.onClick.AddListener(OnHandFocusReturnButtonPressed);
        
        uiManager.SetFocusedOnHands(true);
    }

    public void OnHandFocusReturnButtonPressed()
    {
        if (activeUI)
        {
            activeUI.ToggleOff();
            activeUI = null;
        }

        _instance = null;
        mainCamera.SetActive(true);
        focusCamera.SetActive(false);
        if (uiManager != null) uiManager.UseMainCamera();
        //cameraControls.SetActive(true);
        handsUI.SetActive(false);
        uiManager.SetFocusedOnHands(false);
    }

    ////////////////////////////////////////////////////////////////////////////////// FINGER ENTRY
    
    [System.Serializable]
    public class FingerEntry 
    {
        public Button fingerButton;
        public string fingerTag;
        public List<GameObject> articulationButtons = new();
    }

    /// Attributes
    public string[] fingerTags = new string[] { "Thumb", "Index", "Middle", "Ring", "Pinky" };
    [SerializeField] private List<FingerEntry> fingers;
    private FingerEntry activeFinger = null;

    /// Methods
    private void CollectArticulationButtons()
    {
        ArticulationUI[] allUI = FindObjectsByType<ArticulationUI>(FindObjectsSortMode.None);

        foreach (FingerEntry f in fingers)
        {
            f.articulationButtons = new List<GameObject>();

            foreach (ArticulationUI a in allUI)
            {
                if (a.transform.parent == null) continue;

                if (a.transform.parent.name.Contains(f.fingerTag))
                    f.articulationButtons.Add(a.gameObject);
            }

            Debug.Log($"HandFocus: '{f.fingerTag}' collected {f.articulationButtons.Count} buttons.");
        }
    }

    private void SetFingerVisible(FingerEntry f, bool b) {
        foreach (GameObject a in f.articulationButtons)
            if (a != null) a.SetActive(b);
    }

    private void HideAllFingers()
    {
        foreach (FingerEntry f in fingers) {
            SetFingerVisible(f, false);
        }
    } 

    private void ConnectFingerClicks()
    {
        foreach(FingerEntry f in fingers) {
            FingerEntry l = f;
            f.fingerButton.onClick.AddListener(() => OnFingerClicked(l));
        }
    }

    ////////////////////////////////////////////////////////////////////////////////// FINGER SELECTOR
    
    private void OnFingerClicked(FingerEntry f) 
    {
        if (activeFinger == f) {
            SetFingerVisible(f, false);
            activeFinger = null;
            return;
        }

        if (activeFinger != null)
            SetFingerVisible(activeFinger, false);
        
        SetFingerVisible(f, true);
        activeFinger = f;
    }

    void Start() 
    {
        handsUI.SetActive(true);
        FetchButtons();

        CollectArticulationButtons();
        HideAllFingers();
        ConnectFingerClicks();
        handsUI.SetActive(false);
    }
}
