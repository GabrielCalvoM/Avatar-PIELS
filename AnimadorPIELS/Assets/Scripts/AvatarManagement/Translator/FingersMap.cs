using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "FingersMap", menuName = "Scriptable Objects/FingersMap")]
public class FingersMap : ScriptableObject
{
    [SerializeField, BoxGroup("Thumb")]
    string _thumb1;
    [SerializeField, BoxGroup("Thumb")]
    string _thumb2;
    [SerializeField, BoxGroup("Thumb")]
    string _thumb3;

    [SerializeField, BoxGroup("Index")]
    string _index1;
    [SerializeField, BoxGroup("Index")]
    string _index2;
    [SerializeField, BoxGroup("Index")]
    string _index3;

    [SerializeField, BoxGroup("Middle")]
    string _middle1;
    [SerializeField, BoxGroup("Middle")]
    string _middle2;
    [SerializeField, BoxGroup("Middle")]
    string _middle3;

    [SerializeField, BoxGroup("Ring")]
    string _ring1;
    [SerializeField, BoxGroup("Ring")]
    string _ring2;
    [SerializeField, BoxGroup("Ring")]
    string _ring3;

    [SerializeField, BoxGroup("Pinky")]
    string _pinky1;
    [SerializeField, BoxGroup("Pinky")]
    string _pinky2;
    [SerializeField, BoxGroup("Pinky")]
    string _pinky3;

    public string Thumb1 => _thumb1;
    public string Thumb2 => _thumb2;
    public string Thumb3 => _thumb3;

    public string Index1 => _index1;
    public string Index2 => _index2;
    public string Index3 => _index3;
    
    public string Middle1 => _middle1;
    public string Middle2 => _middle2;
    public string Middle3 => _middle3;
    
    public string Ring1 => _ring1;
    public string Ring2 => _ring2;
    public string Ring3 => _ring3;
    
    public string Pinky1 => _pinky1;
    public string Pinky2 => _pinky2;
    public string Pinky3 => _pinky3;
}
