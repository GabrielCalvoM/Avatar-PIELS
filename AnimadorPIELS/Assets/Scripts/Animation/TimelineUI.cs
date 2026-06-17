using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class TimelineUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _addKeyframeButton;
    [SerializeField] private Button _playButton;
    [SerializeField] private PlayheadDragHandler _playhead;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private RectTransform _keyframesContainer;
    [SerializeField] private GameObject _keyframePrefab;
    [SerializeField] private RectTransform _timeTextsContainer;
    [SerializeField] private GameObject _timeLabelPrefab;
    [SerializeField] private RectTransform _scrollContent;

    [Header("Layout")]
    [SerializeField] private float _keyframeY = 0f;
    [SerializeField] private float _timeLabelY = 0f;

    private Timeline _timeline;
    private int _nextKeyframeId = 0;
    private bool _isPlaying = false;
    private Coroutine _playCoroutine;
    private float _maxLabelTime = 0f;
    private Dictionary<float, (int id, GameObject go, PoseData pose)> _keyframeObjects = new();

    private void Awake()
    {
        _addKeyframeButton.onClick.AddListener(OnAddKeyframeClicked);
        _playButton.onClick.AddListener(OnPlayClicked);
    }

    private void Start()
    {
        _timeline = new Timeline();
        TimelineManager.Instance.SetTimeline(_timeline);
        TimelineManager.Instance.OnMinuteChange += OnMinuteChanged;

        SpawnLabelsUpTo(5f);
    }

    private void OnDestroy()
    {
        if (TimelineManager.Instance != null)
            TimelineManager.Instance.OnMinuteChange -= OnMinuteChanged;
    }

    private void Update()
    {
        if (_uiManager != null && _uiManager.IsAnimatorUIOpen()
            && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            OnPlayClicked();
        }
    }

    private void OnMinuteChanged(float second)
    {
        if (_isPlaying) return;
        if (_keyframeObjects.TryGetValue(second, out var existing))
            SaveLoadPose.Instance.ApplyCurrentPose(existing.pose);
    }

    private void OnAddKeyframeClicked()
    {
        float second = TimelineManager.Instance.CurrentMinute;

        if (_keyframeObjects.TryGetValue(second, out var existing))
        {
            Destroy(existing.go);
            _keyframeObjects.Remove(second);
            return;
        }

        PoseData snapshot = SaveLoadPose.Instance.CaptureCurrentPose();
        int id = _nextKeyframeId++;
        TimelineManager.Instance.SetCurrentKeyframe(id);
        _timeline.SaveMinute(id, second);

        GameObject kfGo = Instantiate(_keyframePrefab, _keyframesContainer);
        RectTransform kfRect = kfGo.GetComponent<RectTransform>();
        kfRect.anchorMin = new Vector2(0f, 1f);
        kfRect.anchorMax = new Vector2(0f, 1f);
        kfRect.anchoredPosition = new Vector2(_playhead.MinuteToX(second), _keyframeY);

        _keyframeObjects[second] = (id, kfGo, snapshot);

        // Extend time labels if keyframe is near or beyond the end
        while (second >= _maxLabelTime - 1f)
            SpawnLabelsUpTo(_maxLabelTime + 5f);
    }

    private void OnPlayClicked()
    {
        if (_isPlaying)
        {
            StopCoroutine(_playCoroutine);
            _isPlaying = false;
            return;
        }

        if (_keyframeObjects.Count == 0) return;

        _playCoroutine = StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        _isPlaying = true;

        List<(float second, PoseData pose)> keyframes = _keyframeObjects
            .OrderBy(k => k.Key)
            .Select(k => (k.Key, k.Value.pose))
            .ToList();

        // Move playhead to 0
        _playhead.SetMinute(0f);

        // Determine starting pose at second 0
        PoseData poseAtZero = _keyframeObjects.TryGetValue(0f, out var kfAtZero)
            ? kfAtZero.pose
            : SaveLoadPose.Instance.CaptureCurrentPose();

        // Insert virtual keyframe at 0 if none exists there
        if (!_keyframeObjects.ContainsKey(0f))
            keyframes.Insert(0, (0f, poseAtZero));

        SaveLoadPose.Instance.ApplyCurrentPose(poseAtZero);

        float currentTime = 0f;
        float endTime = keyframes[keyframes.Count - 1].second;

        while (currentTime < endTime)
        {
            currentTime += Time.deltaTime;
            currentTime = Mathf.Min(currentTime, endTime);

            // Find surrounding keyframe pair
            (float second, PoseData pose) prev = keyframes[0];
            (float second, PoseData pose) next = keyframes[keyframes.Count - 1];

            for (int i = 0; i < keyframes.Count - 1; i++)
            {
                if (keyframes[i].second <= currentTime && keyframes[i + 1].second >= currentTime)
                {
                    prev = keyframes[i];
                    next = keyframes[i + 1];
                    break;
                }
            }

            float span = next.second - prev.second;
            float t = span > 0f ? (currentTime - prev.second) / span : 1f;

            PoseData lerped = SaveLoadPose.Instance.LerpPose(prev.pose, next.pose, t);
            SaveLoadPose.Instance.ApplyCurrentPose(lerped);
            _playhead.SetMinute(currentTime);

            yield return null;
        }

        // Hard-apply last keyframe
        SaveLoadPose.Instance.ApplyCurrentPose(keyframes[keyframes.Count - 1].pose);
        _isPlaying = false;
    }

    public List<KeyframeData> GetKeyframes()
    {
        return _keyframeObjects
            .OrderBy(k => k.Key)
            .Select(k => new KeyframeData { second = k.Key, pose = k.Value.pose })
            .ToList();
    }

    public void ClearKeyframes()
    {
        foreach (var entry in _keyframeObjects.Values)
            Destroy(entry.go);
        _keyframeObjects.Clear();
        _nextKeyframeId = 0;
        _timeline = new Timeline();
        TimelineManager.Instance.SetTimeline(_timeline);
        _playhead.SetMinute(0f);
    }

    public void LoadKeyframes(List<KeyframeData> keyframes)
    {
        // Clear existing keyframes
        foreach (var entry in _keyframeObjects.Values)
            Destroy(entry.go);
        _keyframeObjects.Clear();
        _nextKeyframeId = 0;
        _timeline = new Timeline();
        TimelineManager.Instance.SetTimeline(_timeline);

        foreach (var kf in keyframes)
        {
            int id = _nextKeyframeId++;
            _timeline.SaveMinute(id, kf.second);

            GameObject kfGo = Instantiate(_keyframePrefab, _keyframesContainer);
            RectTransform kfRect = kfGo.GetComponent<RectTransform>();
            kfRect.anchorMin = new Vector2(0f, 1f);
            kfRect.anchorMax = new Vector2(0f, 1f);
            kfRect.anchoredPosition = new Vector2(_playhead.MinuteToX(kf.second), _keyframeY);

            _keyframeObjects[kf.second] = (id, kfGo, kf.pose);

            while (kf.second >= _maxLabelTime - 1f)
                SpawnLabelsUpTo(_maxLabelTime + 5f);
        }
    }

    private void SpawnLabelsUpTo(float targetMax)
    {
        float step = 0.5f;
        float t = _maxLabelTime == 0f ? 0f : _maxLabelTime + step;

        while (t <= targetMax + 0.001f)
        {
            GameObject label = Instantiate(_timeLabelPrefab, _timeTextsContainer);
            label.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, _timeLabelY);
            label.GetComponentInChildren<TMP_Text>().text = t.ToString("0.0");
            t += step;
        }

        _maxLabelTime = targetMax;

        if (_scrollContent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_timeTextsContainer);
            _scrollContent.sizeDelta = new Vector2(
                _timeTextsContainer.rect.width,
                _scrollContent.sizeDelta.y);
        }
    }
}
