using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Rotator : MonoBehaviour
{
    [SerializeField] UnityEvent onPointerDown;
    [SerializeField] UnityEvent onPointerUp;

    EventTrigger trigger;
    
    void Awake()
    {
        trigger = GetComponent<EventTrigger>();
    }

    
    void Update()
    {
        
    }
}
