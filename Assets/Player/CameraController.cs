using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private float m_XSensitivity = .2f;
    private float m_YSensitivity = .2f;
    private float m_MinimumX = -90F;
    private float m_MaximumX = 90F;
    private bool m_LockCursor = true;

    Vector2 CurrentLook;

    [SerializeField] PlayerInputManager inputManager;

    public void Start()
    {
        Camera.main.transform.GetComponent<FollowTarget>().targetTransform = transform.GetChild(0);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PlayerInputManager.OnGamePaused += SetCursorLock;
    }

    public void LookRotation(Transform character, Transform CamPiv)
    {
        Vector2 mouseInput = inputManager.MouseInput;
        mouseInput.x *= m_XSensitivity;
        mouseInput.y *= -m_YSensitivity;

        CurrentLook.x += mouseInput.x;
        CurrentLook.y = Mathf.Clamp(CurrentLook.y += mouseInput.y, -90, 90);

        CamPiv.localRotation = Quaternion.Euler(CurrentLook.y, CurrentLook.x, 0);

    }

    public void SetCursorLock(bool value)
    {
        m_LockCursor = value;
        if (m_LockCursor)
        {//we force unlock the cursor if the user disable the cursor locking helper
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }



    private Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, m_MinimumX, m_MaximumX);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
        
    }
}
