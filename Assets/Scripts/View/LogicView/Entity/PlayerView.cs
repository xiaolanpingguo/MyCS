using Lockstep.Game;
using Lockstep.Math;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Lockstep.Game 
{
    public class PlayerView : EntityView, IPlayerView 
    {
        // Move
        public float MoveSpeed = 5f;
        public float RunningSpeedMultiple = 2f;
        public float ActualSpeed;

        // camera
        public float MouseSensitivity = 2.4f;
        private float m_angleY;
        private float m_angleX;
        private Transform m_cameraTrans;

        // jump
        public float InitJumpSpeed = 5f;
        private bool m_isGrounded = true;
        private float m_jumpSpeed = 0f;

        // crouch
        public float CrouchHeight = 1f;
        private bool m_isCrouching = false;
        private Vector3 m_defaultCrouchCenter;
        private float m_defaultCrouchHeight;

        private CollisionFlags m_collisionFlags;

        public Player Player;
        protected bool isDead => entity?.isDead ?? true;

        private CharacterController m_characterController;

        public override void BindEntity(BaseEntity e, BaseEntity oldEntity = null)
        {
            base.BindEntity(e, oldEntity);
            Player = e as Player;
            gameObject.AddComponent<CharacterController>();
            m_characterController = gameObject.GetComponent<CharacterController>();

            m_angleY = transform.eulerAngles.y;
            m_cameraTrans = Camera.main.transform;
            m_angleX = m_cameraTrans.eulerAngles.x;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        protected override void Update()
        {
            Move();
            TurnAndLook();
            Jump();
            Crouch();
        }

        private void Move()
        {
            var playerInput = Player.input;

            ActualSpeed = MoveSpeed;
            bool isRunning = playerInput.ButtonFlags.IsSet(Button.Sprint);
            if (isRunning)
            {
                ActualSpeed *= RunningSpeedMultiple;
            }

            Vector3 move = new Vector3(playerInput.inputUV.x, 0, playerInput.inputUV.y);
            UnityEngine.Debug.Log($"Move:{move}");
            //Vector3 move = new Vector3(0.5f, 0, 0.5f);
            move.Normalize();
            move = move * Time.deltaTime * ActualSpeed;
            move = transform.TransformDirection(move);
            m_characterController.Move(move);
            if (playerInput.inputUV.x <= 0.1f && playerInput.inputUV.y <= 0.1f)
            {
                ActualSpeed = 0;
            }
        }

        private void TurnAndLook()
        {
            var playerInput = Player.input;

            LVector2 inputLook = playerInput.InputLook;
            m_angleY = m_angleY + inputLook.x * MouseSensitivity;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, m_angleY, transform.eulerAngles.z);

            float lookAngle = -inputLook.y * MouseSensitivity;
            m_angleX = Mathf.Clamp(m_angleX + lookAngle, -90f, 90f);
            m_cameraTrans.eulerAngles = new Vector3(m_angleX, m_cameraTrans.eulerAngles.y, m_cameraTrans.eulerAngles.z);
        }

        private void Jump()
        {
            var playerInput = Player.input;

            bool isJumpPressed = playerInput.ButtonFlags.IsSet(Button.Jump);
            if (isJumpPressed && m_isGrounded)
            {
                m_isGrounded = false;
                m_jumpSpeed = InitJumpSpeed;;
            }

            // in air
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
            if (m_isGrounded && m_collisionFlags == CollisionFlags.None)
            {
                m_isGrounded = false;
            }
        }

        private void Crouch()
        {
            var playerInput = Player.input;

            bool isCrouchPressed = playerInput.ButtonFlags.IsSet(Button.Crouch);
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
}