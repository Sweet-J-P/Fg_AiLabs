using System;
using UnityEngine;

public class EyebrowsHandler : MonoBehaviour
{

    [SerializeField] private Transform[] eyebrows;

    private void OnEnable()
    {
        SimpleGuardFsm.OnSimpleGuardStateChange += ChangeMood;
    }

    private void OnDisable()
    {
        SimpleGuardFsm.OnSimpleGuardStateChange -= ChangeMood;
    }

    private void ChangeMood(SimpleGuardFsm.SimpleGuardStates  state)
    {

        switch (state)
        {
            case SimpleGuardFsm.SimpleGuardStates.Chase:
                NewMood();
                break;
            case SimpleGuardFsm.SimpleGuardStates.Return:
                NewMood();
                break;
        }
    }

    private void NewMood()
    {
        print("changed mood?");
        foreach (var eyebrow in eyebrows)
        {
            float rotation = eyebrow.localEulerAngles.z;
            rotation *= -1f;
            eyebrow.localEulerAngles = new Vector3(0f, 0f, rotation);
        }
    }
}