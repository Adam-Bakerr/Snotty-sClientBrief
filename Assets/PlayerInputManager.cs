using Assets;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    public Vector2 MoveInput = Vector2.zero;
    public Vector2 MouseInput = Vector2.zero;
    public bool isJumping = false;
    public bool isJumpPressed = false;
    public bool isJumpReleased = false;
    public bool isSprinting = false;
    public void OnLook(InputAction.CallbackContext context)
    {
        MouseInput = context.ReadValue<Vector2>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.action.WasPressedThisFrame();
        isJumpReleased = context.action.WasReleasedThisFrame();
        isJumping = context.action.IsPressed(); 
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.action.IsPressed();
    }
}
