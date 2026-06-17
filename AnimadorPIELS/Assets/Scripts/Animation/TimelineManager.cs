using System;
using UnityEngine;

public class TimelineManager : MonoBehaviour
{
    private static TimelineManager _instance;
    public static TimelineManager Instance => _instance;

    private float _minute = 0;
    public float CurrentMinute => _minute;

    private Timeline _timeline = null;
    private int _keyframeId;

    public event Action<float> OnMinuteChange;

    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        ArticulationMov.OnEndRotation -= SaveBoneRotation;
    }

    public void SetMinute(float newMinute)
    {
        _minute = newMinute;
        OnMinuteChange?.Invoke(_minute);
    }

    public void AdvanceMinute(float speed = 1)
    {
        _minute += Time.unscaledDeltaTime * speed;
        OnMinuteChange?.Invoke(_minute);
    }

    public void RewindMinute(float speed = 1)
    {
        _minute -= Time.unscaledDeltaTime * speed;
        OnMinuteChange?.Invoke(_minute);
    }

    public void SetTimeline(Timeline timeline)
    {
        _timeline = timeline;
    }

    public void SetCurrentKeyframe(int keyframe)
    {
        _keyframeId = keyframe;
    }

    public void SaveBoneRotation(string bone, Quaternion localRotation)
    {
        _timeline.SaveBoneRotation(_keyframeId, bone, localRotation);
    }
}