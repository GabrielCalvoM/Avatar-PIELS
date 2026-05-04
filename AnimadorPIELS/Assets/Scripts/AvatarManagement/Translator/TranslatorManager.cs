using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class TranslatorManager : MonoBehaviour
{
    static TranslatorManager _instance;

    [SerializeField]
    List<AvatarTranslator> _avatarTranslators;
    public List<AvatarTranslator> Translators => _avatarTranslators;

    private void Awake()
    {
        _instance = this;
    }

    
}
