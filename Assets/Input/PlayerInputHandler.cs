using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [HideInInspector] public InputAction moveAction;
        [HideInInspector] public InputAction jumpAction;
        [HideInInspector] public InputAction sprintAction;
        [HideInInspector] public InputAction attackAction;
        [HideInInspector] public InputAction pauseAction;

        private void Start()
        {
            moveAction = InputSystem.actions.FindAction("Move");
            jumpAction = InputSystem.actions.FindAction("Jump");
            sprintAction = InputSystem.actions.FindAction("Sprint"); 
            attackAction = InputSystem.actions.FindAction("Attack");
            pauseAction = InputSystem.actions.FindAction("Pause");
        }
    }
}
