using System.Collections.Generic;
using Lockstep.Game;
using Lockstep.Logging;
using Lockstep.Math;
using Lockstep.Serialization;
using Lockstep.Util;
using NetMsg.Common;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Lockstep.Game 
{
    public enum Button : uint
    {
        None = 0,
        Sprint = 1 << 0,
        Crouch = 1 << 1,
        Jump = 1 << 2,
        Fire = 1 << 3,
        ADS = 1 << 4,
        Reload = 1 << 5,
        WeaponSwap = 1 << 6,

        AnyButtonMask = uint.MaxValue
    }

    public struct ButtonBitField
    {
        public uint flags;

        public readonly bool IsSet(Button button)
        {
            return (flags & (uint)button) > 0;
        }

        public void Or(Button button, bool val)
        {
            if (val)
            {
                flags |= (uint)button;
            }
        }

        public void Set(Button button, bool val)
        {
            if (val)
            {
                flags |= (uint)button;
            }
            else
            {
                flags &= ~(uint)button;
            }
        }
    }

    public struct UserCommand
    {
        public ButtonBitField buttons;
        public Vector2 inputMove;
    }

    public class GameInputService : IInputService
    {
        public static PlayerInput CurGameInput = new PlayerInput();

        private InputSource m_inputActions;
        private InputSource.GameplayActions m_gameplayActions;
        private ButtonBitField m_currentFrameButtons;

        public UserCommand CurrentUserCommand;

        public void Awake()
        {
            m_inputActions = new InputSource();
            m_inputActions.Enable();
            m_gameplayActions = m_inputActions.Gameplay;
            EnterGameplay();
        }

        public void Update()
        {
            if (InputSystem.settings.updateMode != InputSettings.UpdateMode.ProcessEventsManually)
            {
                return;
            }

            InputSystem.Update();

            CurrentUserCommand = default;
            CurrentUserCommand.inputMove = Vector2.ClampMagnitude(m_gameplayActions.Move.ReadValue<Vector2>(), 1f);
            CurGameInput.inputUV = Vector2.ClampMagnitude(m_gameplayActions.Move.ReadValue<Vector2>(), 1f).ToLVector2();

            Vector2 look = m_gameplayActions.Look.ReadValue<Vector2>();
            if (look != Vector2.zero)
            {
                CurGameInput.InputLook = m_gameplayActions.Look.ReadValue<Vector2>().ToLVector2();
            }

            m_currentFrameButtons = default;
            m_currentFrameButtons.Or(Button.Sprint, m_gameplayActions.Sprint.IsPressed());
            m_currentFrameButtons.Or(Button.Crouch, m_gameplayActions.Crouch.IsPressed());
            m_currentFrameButtons.Or(Button.Jump, m_gameplayActions.Jump.IsPressed());
            m_currentFrameButtons.Or(Button.Fire, m_gameplayActions.Fire.IsPressed());
            m_currentFrameButtons.Or(Button.ADS, m_gameplayActions.ADS.IsPressed());
            m_currentFrameButtons.Or(Button.Reload, m_gameplayActions.Reload.IsPressed());
            m_currentFrameButtons.Or(Button.WeaponSwap, m_gameplayActions.WeaponSwap.IsPressed());

            CurrentUserCommand.buttons.flags |= m_currentFrameButtons.flags;
            CurGameInput.ButtonFlags = CurrentUserCommand.buttons;
        }

        public static void EnterGameplay()
        {
            // We'll update the input manually before reading them in the update loop
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
        }

        public static void ExitGameplay()
        {
            // Let the inputs system update itself because we won't be updating it manually
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
        }

        public void Execute(InputCmd cmd, object entity)
        {
            var input = new Deserializer(cmd.content).Parse<PlayerInput>();
            var playerInput = entity as PlayerInput;
            playerInput.mousePos = input.mousePos;
            playerInput.inputUV = input.inputUV;
            playerInput.InputLook = input.InputLook;
            playerInput.isInputFire = input.isInputFire;
            playerInput.skillId = input.skillId;
            playerInput.isSpeedUp = input.isSpeedUp;
            UnityEngine.Debug.Log($" playerInput.inputUV:{playerInput.inputUV}");
        }

        public List<InputCmd> GetInputCmds()
        {
            if (CurGameInput.Equals(PlayerInput.Empty))
            {
                return null;
            }

            return new List<InputCmd>() 
            {
                new InputCmd() 
                {
                    content = CurGameInput.ToBytes()
                }
            };
        }

        public List<InputCmd> GetDebugInputCmds()
        {
            return new List<InputCmd>()
            {
                new InputCmd() 
                {
                    content = new PlayerInput() 
                    {
                        inputUV = new LVector2(LRandom.Range(-1,2),LRandom.Range(-1,2)),
                        skillId = LRandom.Range(0,3)
                    }.ToBytes()
                }
            };
        }
    }
}