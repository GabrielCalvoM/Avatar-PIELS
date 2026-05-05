using NaughtyAttributes;
using System;
using UnityEngine;

[Serializable]
public struct AvatarTranslator
{
    [SerializeField, AllowNesting, Label("Avatar Map"), Expandable]
    private AvatarMap _map;
    [SerializeField, AllowNesting, Label("Root Bone")]
    private GameObject _root;
    [SerializeField, AllowNesting, Label("Blendshapes Source")]
    private SkinnedMeshRenderer _face;

    public AvatarMap Map => _map;
    public GameObject Root => _root;
    public SkinnedMeshRenderer Face => _face;
}
