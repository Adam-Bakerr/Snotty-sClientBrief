using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static CameraController;

public class RigidBodyCharacterController : MonoBehaviour
{

    [System.Serializable]
    public class MovementSettings
    {
        public float MaxWalkSpeed;
        public float MaxRunSpeed;
        public float Acceleration;
        public float Deceleration;

        public MovementSettings(float maxWalkSpeed, float maxRunSpeed, float accel, float decel)
        {
            MaxWalkSpeed = maxWalkSpeed;
            MaxRunSpeed = maxRunSpeed;
            Acceleration = accel;
            Deceleration = decel;
        }
        public MovementSettings(float maxWalkSpeed, float accel, float decel)
        {
            MaxWalkSpeed = maxWalkSpeed;
            Acceleration = accel;
            Deceleration = decel;
        }
    }

    [Header("Input")]
    [SerializeField] PlayerInputManager playerInputManager;

    [Header("Aiming")]
    [SerializeField] public CameraController m_camera;
    [SerializeField] Transform m_cameraPivot;

    [Header("Movement")]
    public float m_Friction = 120;
    public float m_Gravity = 400;
    public float m_JumpForce = 160;
    public LayerMask WhatIsGround;

    [Tooltip("Automatically jump when holding jump button")]
    public bool m_AutoBunnyHop = false;

    [Tooltip("How precise air control is")]
    public float m_AirControl = 0.3f;
    public MovementSettings m_GroundSettings = new MovementSettings(140, 210, 280, 200);
    public MovementSettings m_AirSettings = new MovementSettings(140, 40, 40);
    public MovementSettings m_StrafeSettings = new MovementSettings(20, 1000, 1000);


    public CharacterController m_Character;
    private Vector3 m_PlayerVelocity = Vector3.zero;

    // Used to queue the next jump just before hitting the ground.
    private bool m_JumpQueued = false;


    private void Update()
    {
        QueueJump();

        // Set movement state.
        if (m_Character.isGrounded)
        {
            GroundMove();
        }
        else
        {
            AirMove();
        }

        // Rotate the character and camera.
        m_camera.LookRotation(transform, m_cameraPivot);

        // Move the character.
        m_Character.Move(m_PlayerVelocity * Time.deltaTime);
    }

    // Queues the next jump.
    private void QueueJump()
    {
        if (m_AutoBunnyHop)
        {
            m_JumpQueued = playerInputManager.isJumping;
            return;
        }

        if (playerInputManager.isJumpPressed && !m_JumpQueued)
        {
            m_JumpQueued = true;
        }

        if (playerInputManager.isJumpReleased)
        {
            m_JumpQueued = false;
        }
    }

    // Handle air movement.
    private void AirMove()
    {
        float accel;

        var wishdir = new Vector3(playerInputManager.MoveInput.x, 0, playerInputManager.MoveInput.y);
        wishdir = m_cameraPivot.TransformDirection(wishdir);

        float wishspeed = wishdir.magnitude;
        wishspeed *= m_AirSettings.MaxWalkSpeed;

        wishdir.Normalize();

        // CPM Air control.
        float wishspeed2 = wishspeed;
        if (Vector3.Dot(m_PlayerVelocity, wishdir) < 0)
        {
            accel = m_AirSettings.Deceleration;
        }
        else
        {
            accel = m_AirSettings.Acceleration;
        }

        // If the player is ONLY strafing left or right
        if (playerInputManager.MoveInput.y == 0 && playerInputManager.MoveInput.x != 0)
        {
            if (wishspeed > m_StrafeSettings.MaxWalkSpeed)
            {
                wishspeed = m_StrafeSettings.MaxWalkSpeed;
            }

            accel = m_StrafeSettings.Acceleration;
        }

        Accelerate(wishdir, wishspeed, accel);
        if (m_AirControl > 0)
        {
            AirControl(wishdir, wishspeed2);
        }

        // Apply gravity
        m_PlayerVelocity.y -= m_Gravity * Time.deltaTime;
    }

    // Air control occurs when the player is in the air, it allows players to move side 
    // to side much faster rather than being 'sluggish' when it comes to cornering.
    private void AirControl(Vector3 targetDir, float targetSpeed)
    {
        // Only control air movement when moving forward or backward.
        if (Mathf.Abs(playerInputManager.MoveInput.y) < 0.001 || Mathf.Abs(targetSpeed) < 0.001)
        {
            return;
        }

        float zSpeed = m_PlayerVelocity.y;
        m_PlayerVelocity.y = 0;
        /* Next two lines are equivalent to idTech's VectorNormalize() */
        float speed = m_PlayerVelocity.magnitude;
        m_PlayerVelocity.Normalize();

        float dot = Vector3.Dot(m_PlayerVelocity, targetDir);
        float k = 32;
        k *= m_AirControl * dot * dot * Time.deltaTime;

        // Change direction while slowing down.
        if (dot > 0)
        {
            m_PlayerVelocity.x *= speed + targetDir.x * k;
            m_PlayerVelocity.y *= speed + targetDir.y * k;
            m_PlayerVelocity.z *= speed + targetDir.z * k;

            m_PlayerVelocity.Normalize();
        }

        m_PlayerVelocity.x *= speed;
        m_PlayerVelocity.y = zSpeed; // Note this line
        m_PlayerVelocity.z *= speed;
    }

    // Handle ground movement.
    private void GroundMove()
    {
        // Do not apply friction if the player is queueing up the next jump
        if (!m_JumpQueued)
        {
            ApplyFriction(1.0f);
        }
        else
        {
            ApplyFriction(0);
        }

        var wishdir = new Vector3(playerInputManager.MoveInput.x, 0, playerInputManager.MoveInput.y);
        wishdir = m_cameraPivot.TransformDirection(wishdir);
        wishdir.Normalize();

        var wishspeed = wishdir.magnitude;
        wishspeed *= playerInputManager.isSprinting ? m_GroundSettings.MaxRunSpeed : m_GroundSettings.MaxWalkSpeed;

        Accelerate(wishdir, wishspeed, m_GroundSettings.Acceleration);

        // Reset the gravity velocity
        m_PlayerVelocity.y = -m_Gravity * Time.deltaTime;

        if (m_JumpQueued)
        {
            m_PlayerVelocity.y = m_JumpForce;
            m_JumpQueued = false;
        }
    }

    private void ApplyFriction(float t)
    {
        // Equivalent to VectorCopy();
        Vector3 vec = m_PlayerVelocity;
        vec.y = 0;
        float speed = vec.magnitude;
        float drop = 0;

        // Only apply friction when grounded.
        if (m_Character.isGrounded)
        {
            float control = speed < m_GroundSettings.Deceleration ? m_GroundSettings.Deceleration : speed;
            drop = control * m_Friction * Time.deltaTime * t;
        }

        float newSpeed = speed - drop;

        if (newSpeed < 0)
        {
            newSpeed = 0;
        }

        if (speed > 0)
        {
            newSpeed /= speed;
        }

        m_PlayerVelocity.x *= newSpeed;
        // playerVelocity.Y *= newSpeed;
        m_PlayerVelocity.z *= newSpeed;
    }

    // Calculates acceleration based on desired speed and direction.
    private void Accelerate(Vector3 targetDir, float targetSpeed, float accel)
    {
        float currentspeed = Vector3.Dot(m_PlayerVelocity, targetDir);
        float addspeed = targetSpeed - currentspeed;
        if (addspeed <= 0)
        {
            return;
        }

        float accelspeed = accel * Time.deltaTime * targetSpeed;
        if (accelspeed > addspeed)
        {
            accelspeed = addspeed;
        }

        m_PlayerVelocity.x += accelspeed * targetDir.x;
        m_PlayerVelocity.z += accelspeed * targetDir.z;
    }
}
