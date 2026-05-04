using NaughtyAttributes;
using System;
using UnityEngine;

[Serializable]
public struct AvatarTranslator
{
    [SerializeField, Expandable]
    private AvatarMap _avatarMap;
    [SerializeField]
    private GameObject _root;
    [SerializeField]
    private SkinnedMeshRenderer _avatarFace;
}
