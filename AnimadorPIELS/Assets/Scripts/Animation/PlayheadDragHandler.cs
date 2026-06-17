using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PlayheadDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    [Header("References")]
    [SerializeField] private ScrollRect _scrollRect;

    [Header("Layout")]
    [SerializeField] private float _unitsPerStep = 100f;
    [SerializeField] private float _minutesPerStep = 0.5f;
    [SerializeField] private float _timelineOffset = 50f;

    private RectTransform _handleRect;
    private RectTransform _timelineParent;
    private float _currentMinute;
    private float _grabOffset;

    private void Awake()
    {
        _handleRect = GetComponent<RectTransform>();
        _timelineParent = _handleRect.parent as RectTransform;
    }

    // Eat the pointer-down so the ScrollRect never gets focus
    public void OnPointerDown(PointerEventData eventData)
    {
        // Intentionally empty
        // consuming the event prevents the ScrollRect from claiming ownership of the drag sequence.
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_scrollRect != null)
            _scrollRect.enabled = false;

        // Capture the offset so the handle doesn't jump to the pointer tip
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _timelineParent, eventData.position, eventData.pressEventCamera,
            out Vector2 localPoint);

        _grabOffset = _handleRect.anchoredPosition.x - localPoint.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _timelineParent, eventData.position, eventData.pressEventCamera,
            out Vector2 localPoint);

        float rawUnits = localPoint.x + _grabOffset;
        float rawMinute = ((rawUnits - _timelineOffset) / _unitsPerStep) * _minutesPerStep;
        float snapped = Mathf.Round(rawMinute / _minutesPerStep) * _minutesPerStep;
        float clamped = Mathf.Max(0f, snapped);

        ApplyMinute(clamped);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_scrollRect != null)
            _scrollRect.enabled = true;
    }

    public void SetMinute(float minute)
    {
        ApplyMinute(Mathf.Max(0f, minute));
    }

    public float MinuteToX(float minute) => _timelineOffset + (minute / _minutesPerStep) * _unitsPerStep;

    private void ApplyMinute(float minute)
    {
        _currentMinute = minute;

        _handleRect.anchoredPosition = new Vector2(MinuteToX(minute), _handleRect.anchoredPosition.y);

        TimelineManager.Instance.SetMinute(minute);
    }
}