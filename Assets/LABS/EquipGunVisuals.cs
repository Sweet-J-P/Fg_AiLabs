using System;
using UnityEngine;

public class EquipGunVisuals : MonoBehaviour
{
    [SerializeField] private GameObject unarmedArm;
    [SerializeField] private GameObject armedArm;
    [SerializeField] private GameObject weapon;
    [SerializeField] private bool bStartWithWeapon;

    private void Start()
    {
        if(bStartWithWeapon)
            ToggleEquipWeapon();
    }

    public void ToggleEquipWeapon()
    {
        unarmedArm.SetActive(false);
        armedArm.SetActive(true);
        weapon.SetActive(true);
    }
}
