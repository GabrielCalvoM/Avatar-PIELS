using System.Collections.Generic;
using UnityEngine;

public class KeyFrame
{
    private float _minute = 0.0f;
    public float Minute => _minute;

    private Dictionary<string, BoneData> _poseData;
    private static Dictionary<string, ArticulationMov> _poseBones;
    public IEnumerable<string> Bones => _poseBones.Keys;

    private Dictionary<string, BlendshapeWeightData> _faceExpression;
    private static SkinnedMeshRenderer _avatarMesh;
    public static SkinnedMeshRenderer AvatarMesh => _avatarMesh;
    public IEnumerable<string> Blendshapes => _faceExpression.Keys;


    public void SaveMinute(float minute)
    {
        _minute = minute;
    }

    public void SaveBoneRotation(string bone, Quaternion localRotation)
    {
        _poseData[bone].localRotation = localRotation;
    }

    public void SaveFacialExpression()
    {
        if (_avatarMesh == null || _avatarMesh.sharedMesh == null)
            return;

        int blendShapeCount = _avatarMesh.sharedMesh.blendShapeCount;

        for (int i = 0; i < blendShapeCount; i++)
        {
            string blendshape = _avatarMesh.sharedMesh.GetBlendShapeName(i);
            float weight = _avatarMesh.GetBlendShapeWeight(i);

            _faceExpression[blendshape].weight = weight;
        }
    }

    public void SaveBlendshapeWeight(string blendshape, float weight)
    {
        _faceExpression[blendshape].weight = weight;
    }


    public void ApplyPose()
    {
        if (_poseBones != null && _poseData != null)
            foreach (var kvp in _poseData)
                if (_poseBones.TryGetValue(kvp.Key, out var bone))
                    bone.transform.localRotation = kvp.Value.localRotation;

        if (_avatarMesh != null && _avatarMesh.sharedMesh != null && _faceExpression != null)
            foreach (var kvp in _faceExpression)
            {
                int idx = _avatarMesh.sharedMesh.GetBlendShapeIndex(kvp.Key);
                if (idx >= 0) _avatarMesh.SetBlendShapeWeight(idx, kvp.Value.weight);
            }
    }

    public Quaternion GetRotation(string bone) => _poseData[bone].localRotation;
    public ArticulationMov GetBone(string bone) => _poseBones[bone];

    public float GetBlendshapeWeight(string blendshape) => _faceExpression[blendshape].weight;
    public int GetBlendshapeIndex(string blendshape) => _avatarMesh.sharedMesh.GetBlendShapeIndex(blendshape);
}
