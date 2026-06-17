using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnimationSaveLoad : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TimelineUI _timelineUI;
    [SerializeField] private MongoDBConfig _mongoConfig;
    [SerializeField] private UIManager _uiManager;

    [Header("Save UI")]
    [SerializeField] private GameObject _saveUI;
    [SerializeField] private TMP_InputField _saveNameInput;
    [SerializeField] private Button _confirmSaveButton;
    [SerializeField] private Button _cancelSaveButton;

    [Header("Load UI")]
    [SerializeField] private GameObject _loadUI;
    [SerializeField] private Transform _listContent;
    [SerializeField] private GameObject _listButtonPrefab;
    [SerializeField] private Button _confirmLoadButton;
    [SerializeField] private Button _cancelLoadButton;

    private MongoDBService _mongoService;
    private string _selectedAnimation = "";

    private async void Start()
    {
        _mongoService = new MongoDBService();
        bool connected = await _mongoService.Initialize(_mongoConfig);

        if (!connected)
            Debug.LogError("[AnimationSaveLoad] Could not connect to backend.");

        _confirmSaveButton.onClick.AddListener(OnConfirmSave);
        _cancelSaveButton.onClick.AddListener(OnCancelSave);
        _confirmLoadButton.onClick.AddListener(OnConfirmLoad);
        _cancelLoadButton.onClick.AddListener(OnCancelLoad);

        _confirmLoadButton.interactable = false;
    }

    public void BeginSave()
    {
        _saveNameInput.text = "";
        _saveUI.SetActive(true);
        _uiManager?.HideAnimatorUI();
    }

    private async void OnConfirmSave()
    {
        string animName = _saveNameInput.text.Trim();
        if (string.IsNullOrEmpty(animName))
        {
            Debug.LogWarning("[AnimationSaveLoad] Animation name is empty.");
            return;
        }

        List<KeyframeData> keyframes = _timelineUI.GetKeyframes();
        if (keyframes.Count == 0)
        {
            Debug.LogWarning("[AnimationSaveLoad] No keyframes to save.");
            return;
        }

        bool ok = await _mongoService.SaveAnimation(animName, keyframes);
        if (ok)
            Debug.Log($"[AnimationSaveLoad] Animation '{animName}' saved.");
        else
            Debug.LogError($"[AnimationSaveLoad] Failed to save animation '{animName}'.");

        OnCancelSave();
    }

    private void OnCancelSave()
    {
        _saveNameInput.text = "";
        _saveUI.SetActive(false);
        _uiManager?.ShowAnimatorUI();
    }

    public async void BeginLoad()
    {
        _selectedAnimation = "";
        _confirmLoadButton.interactable = false;
        ClearList();
        _loadUI.SetActive(true);
        _uiManager?.HideAnimatorUI();

        List<string> names = await _mongoService.GetAllAnimationNames();

        if (names.Count == 0)
        {
            Debug.LogWarning("[AnimationSaveLoad] No animations found.");
            return;
        }

        foreach (string name in names)
        {
            GameObject item = Instantiate(_listButtonPrefab, _listContent);
            item.GetComponentInChildren<TMP_Text>().text = name;

            string captured = name;
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                _selectedAnimation = captured;
                _confirmLoadButton.interactable = true;
            });
        }
    }

    private async void OnConfirmLoad()
    {
        if (string.IsNullOrEmpty(_selectedAnimation)) return;

        List<KeyframeData> keyframes = await _mongoService.LoadAnimation(_selectedAnimation);

        if (keyframes == null || keyframes.Count == 0)
        {
            Debug.LogError($"[AnimationSaveLoad] Failed to load animation '{_selectedAnimation}'.");
            return;
        }

        string loadedName = _selectedAnimation;
        OnCancelLoad();
        _timelineUI.LoadKeyframes(keyframes);
        Debug.Log($"[AnimationSaveLoad] Animation '{loadedName}' loaded ({keyframes.Count} keyframes).");
    }

    private void OnCancelLoad()
    {
        _selectedAnimation = "";
        _confirmLoadButton.interactable = false;
        ClearList();
        _loadUI.SetActive(false);
        _uiManager?.ShowAnimatorUI();
    }

    private void ClearList()
    {
        for (int i = _listContent.childCount - 1; i >= 0; i--)
            Destroy(_listContent.GetChild(i).gameObject);
    }
}
