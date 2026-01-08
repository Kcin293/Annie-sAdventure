using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSubscription : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; } = Vector2.zero;
    public bool JumpInput { get; private set; } = false;

    public bool AttackInput { get; set; }

    public bool DeattackInput { get; set; }

    public bool InteractInput { get; private set; } = false;
    public event Action OnInteractPressed;

    PlayerInput _input = null;

    public bool InputEnabled { get; private set; } = true;

    public void SetInputEnabled(bool enabled)
    {
        InputEnabled = enabled;
    }

    private void OnEnable()
    {
        _input = new PlayerInput();
        _input.PlayerInputMap.Enable();

        _input.PlayerInputMap.Movement.performed += SetMovement;
        _input.PlayerInputMap.Movement.canceled += SetMovement;
        _input.PlayerInputMap.Jump.started += SetJump;
        _input.PlayerInputMap.Jump.canceled += SetJump;
        _input.PlayerInputMap.Attack.started += SetAttack;
        _input.PlayerInputMap.Attack.canceled += SetAttack;
        _input.PlayerInputMap.Interact.started += SetInteract;
        _input.PlayerInputMap.Interact.canceled += SetInteract;
        _input.PlayerInputMap.Deattach.started += SetDeattach;
        _input.PlayerInputMap.Deattach.canceled += SetDeattach;
    }

    private void OnDisable()
    {
        _input.PlayerInputMap.Disable();
        _input.PlayerInputMap.Movement.performed -= SetMovement;
        _input.PlayerInputMap.Movement.canceled -= SetMovement;
        _input.PlayerInputMap.Jump.started -= SetJump;
        _input.PlayerInputMap.Jump.canceled -= SetJump;
        _input.PlayerInputMap.Attack.started -= SetAttack;
        _input.PlayerInputMap.Attack.canceled -= SetAttack;
        _input.PlayerInputMap.Interact.started -= SetInteract;
        _input.PlayerInputMap.Interact.canceled -= SetInteract;
        _input.PlayerInputMap.Deattach.started -= SetDeattach;
        _input.PlayerInputMap.Deattach.canceled -= SetDeattach;
    }

    void SetMovement(InputAction.CallbackContext context)
    {
        if (!InputEnabled) return;
        MoveInput = context.ReadValue<Vector2>();
    }

    void SetJump(InputAction.CallbackContext context)
    {
        if (!InputEnabled) return;
        JumpInput = context.ReadValueAsButton();
    }

     void SetDeattach(InputAction.CallbackContext context)
    {
        if (!InputEnabled) return;
        DeattackInput = context.ReadValueAsButton();
    }

    void SetAttack(InputAction.CallbackContext context)
    {
        if (!InputEnabled) return;

        if (context.started)
            AttackInput = true;
    }

    void SetInteract(InputAction.CallbackContext context)
    {
        if (!InputEnabled) return;
        InteractInput = context.ReadValueAsButton();

        if (context.started)
            OnInteractPressed?.Invoke();
    }
}
