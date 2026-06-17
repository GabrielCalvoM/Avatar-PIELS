using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Timeline
{
    private Dictionary<int, KeyFrame> _keyFrames = new Dictionary<int, KeyFrame>();
    public List<KeyFrame> KeyFramesSortedByMinute => _keyFrames.Values.OrderBy(k => k.Minute).ToList();

    public void SaveMinute(int keyframe, float minute)
    {
        if (!_keyFrames.ContainsKey(keyframe))
            _keyFrames[keyframe] = new KeyFrame();

        _keyFrames[keyframe].SaveMinute(minute);
    }

    public void SaveBoneRotation(int keyframe, string bone, Quaternion localRotation)
    {
        _keyFrames[keyframe].SaveBoneRotation(bone, localRotation);
    }

    public bool TryApplyKeyframe(float minute)
    {
        foreach (var kf in _keyFrames.Values)
            if (Mathf.Approximately(kf.Minute, minute))
            {
                kf.ApplyPose();
                return true;
            }
        return false;
    }

    public void SetMinute(float minute)
    {
        // Need at least 2 keyframes to interpolate
        var sorted = KeyFramesSortedByMinute;
        if (sorted.Count < 2) return;

        // Find the surrounding keyframes
        KeyFrame prev = sorted[0], next = sorted[sorted.Count - 1];

        for (int i = 0; i < sorted.Count - 1; i++)
        {
            if (sorted[i].Minute <= minute && sorted[i + 1].Minute >= minute)
            {
                prev = sorted[i];
                next = sorted[i + 1];
                break;
            }
        }

        // Avoid division by zero if two keyframes share the same minute
        float span = next.Minute - prev.Minute;
        if (span == 0f) return;

        float interpolation = (minute - prev.Minute) / span;

        foreach (var bone in prev.Bones.Intersect(next.Bones))
        {
            Quaternion prevRotation = prev.GetRotation(bone);
            Quaternion nextRotation = next.GetRotation(bone);

            if (prevRotation == nextRotation) continue;

            prev.GetBone(bone).transform.localRotation =
                Quaternion.Slerp(prevRotation, nextRotation, interpolation);
        }

        SkinnedMeshRenderer mesh = KeyFrame.AvatarMesh;
        if (mesh == null || mesh.sharedMesh == null) return;

        foreach (var blendshape in prev.Blendshapes.Intersect(next.Blendshapes))
        {
            float prevWeight = prev.GetBlendshapeWeight(blendshape);
            float nextWeight = next.GetBlendshapeWeight(blendshape);

            if (prevWeight == nextWeight) continue;

            mesh.SetBlendShapeWeight(
                prev.GetBlendshapeIndex(blendshape),
                Mathf.Lerp(prevWeight, nextWeight, interpolation)
            );
        }
    }
}