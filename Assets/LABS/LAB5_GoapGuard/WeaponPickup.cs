using System;
using UnityEngine;

public class WeaponPickup : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Destroy(gameObject);
    }
}
