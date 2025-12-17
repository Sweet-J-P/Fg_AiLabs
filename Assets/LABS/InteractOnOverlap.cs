using System;
using UnityEngine;
using UnityEngine.Events;

public class InteractOnOverlap : MonoBehaviour
{
    [SerializeField] private UnityEvent onInteract = new UnityEvent();
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IInteractable>() != null)
        {
            other.GetComponent<IInteractable>().Interact();
            onInteract.Invoke();
        }
    }
}
