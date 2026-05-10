using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class TranslatorManager : MonoBehaviour
{
    private static TranslatorManager _instance;
    public static TranslatorManager Instance => _instance;

    private int _avatarIndx = 0;
    [SerializeField]
    private List<AvatarTranslator> _avatarTranslators;

    public List<AvatarTranslator> Translators => _avatarTranslators;
    public AvatarTranslator Avatar => _avatarTranslators[_avatarIndx];

    private void Awake()
    {
        _instance = this;
    }

    
}
