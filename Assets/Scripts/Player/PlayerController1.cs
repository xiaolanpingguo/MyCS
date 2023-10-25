using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class PlayerController1 : MonoBehaviour
{
    public float MoveSpeed = 5f;
    public float RunningSpeedMultiple = 2f;
    public float MouseSensitivity = 2.4f;
    public float InitJumpSpeed = 5f;

    private PlayerInputActions m_playerInputActions;
    private CharacterController m_characterController;

    private CollisionFlags m_collisionFlags;

    // camera
    private float m_angleY;
    private float m_angleX;
    private Transform m_cameraTrans;

    // jump
    private bool m_isGrounded = true;
    private float m_jumpSpeed = 0f;

    // crouch
    public float CrouchHeight = 1f;
    private bool m_isCrouching = false;
    private Vector3 m_defaultCrouchCenter;
    private float m_defaultCrouchHeight;

    void Awake()
    {
        m_playerInputActions = new PlayerInputActions();

        m_characterController = GetComponent<CharacterController>();
        m_angleY = transform.eulerAngles.y;
        m_cameraTrans = Camera.main.transform;
        m_angleX = m_cameraTrans.eulerAngles.x;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        m_defaultCrouchCenter = m_characterController.center;
        m_defaultCrouchHeight = m_characterController.height;
        if (CrouchHeight >= m_defaultCrouchHeight)
        {
            CrouchHeight = m_defaultCrouchHeight * 0.6f;
        }
    }

    private void OnEnable()
    {
        m_playerInputActions.Enable();
        m_playerInputActions.Gameplay.Enable();
    }

    private void OnDisable()
    {
        m_playerInputActions.Disable();
        m_playerInputActions.Gameplay.Disable();
    }

    void Update()
    {
        Move();
        TurnAndLook();
        Jump();
        Crouch();
    }

    private void Move()
    {
        Vector2 inputMove = Vector2.ClampMagnitude(m_playerInputActions.Gameplay.Move.ReadValue<Vector2>(), 1f);
        float moveSpeed = MoveSpeed;
        bool isRunning = m_playerInputActions.Gameplay.Run.IsPressed();
        if (isRunning) 
        { 
            moveSpeed *= RunningSpeedMultiple;
        }

        Vector3 move = new Vector3(inputMove.x, 0, inputMove.y);
        move.Normalize();
        move = move * Time.deltaTime * moveSpeed;
        move = transform.TransformDirection(move);
        m_characterController.Move(move);
    }

    private void TurnAndLook()
    {
        Vector2 inputLook = m_playerInputActions.Gameplay.Look.ReadValue<Vector2>() * 2f;

        m_angleY = m_angleY + inputLook.x * MouseSensitivity;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, m_angleY, transform.eulerAngles.z);

        float lookAngle= -inputLook.y * MouseSensitivity;
        m_angleX= Mathf.Clamp(m_angleX + lookAngle, -90f, 90f);
        m_cameraTrans.eulerAngles = new Vector3(m_angleX, m_cameraTrans.eulerAngles.y, m_cameraTrans.eulerAngles.z);
    }

    private void Jump()
    {
        bool isJumpPressed = m_playerInputActions.Gameplay.Jump.IsPressed();
        if (isJumpPressed && m_isGrounded)
        {
            m_isGrounded = false;
            m_jumpSpeed = InitJumpSpeed;
        }

        if (!m_isGrounded)
        {
            m_jumpSpeed = m_jumpSpeed - 9.8f * Time.deltaTime;// 9.8: gravity
            Vector3 jump = new Vector3(0, m_jumpSpeed * Time.deltaTime, 0);
            m_collisionFlags = m_characterController.Move(jump);
            if (m_collisionFlags == CollisionFlags.Below)
            {
                m_jumpSpeed = 0;
                m_isGrounded = true;
            }
        }

        // check if player in air
        if (m_isGrounded && m_collisionFlags == CollisionFlags.None)
        {
            m_isGrounded = false;
        }
    }

    private void Crouch()
    {
        bool isCrouchPressed = m_playerInputActions.Gameplay.Crouch.IsPressed() || m_playerInputActions.Gameplay.Crouch.triggered;
        if (!isCrouchPressed)
        {
            m_isCrouching = false;
            m_characterController.height = m_defaultCrouchHeight;
            m_characterController.center = m_defaultCrouchCenter;
            return;
        }

        if (m_isCrouching)
        {
            return;
        }

        Vector3 oldCenter = m_characterController.center;
        float oldHeight = m_characterController.height;
        float centerDelta = (oldHeight - CrouchHeight) / 2f;
        m_characterController.height = CrouchHeight;
        m_characterController.center = new Vector3(oldCenter.x, oldCenter.y - centerDelta, oldCenter.z);

        m_isCrouching = true;
    }
}
