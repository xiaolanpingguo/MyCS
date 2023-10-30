//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.InputSystem;


////public enum Button : uint
////{
////    None = 0,
////    Sprint = 1 << 0,
////    Crouch = 1 << 1,
////    Fire = 1 << 2,
////    ADS = 1 << 3,
////    Reload = 1 << 4,
////    WeaponSwap = 1 << 5,

////    AnyButtonMask = uint.MaxValue
////}

////public struct ButtonBitField
////{
////    public uint flags;

////    public readonly bool IsSet(Button button)
////    {
////        return (flags & (uint)button) > 0;
////    }

////    public void Or(Button button, bool val)
////    {
////        if (val)
////        {
////            flags |= (uint)button;
////        }
////    }

////    public void Set(Button button, bool val)
////    {
////        if (val)
////        {
////            flags |= (uint)button;
////        }
////        else
////        {
////            flags &= ~(uint)button;
////        }
////    }
////}

////public struct UserCommand
////{
////    public ButtonBitField buttons;
////    public Vector2 inputMove;
////}


//public class InputManager
//{
//    private InputSource m_inputActions;
//    private InputSource.GameplayActions m_gameplayActions;
//    private ButtonBitField m_currentFrameButtons;

//    public UserCommand CurrentUserCommand;

//    public void Init()
//    {
//        m_inputActions = new InputSource();
//        m_inputActions.Enable();
//    }

//    public void Update()
//    {
//        CurrentUserCommand = default;
//        CurrentUserCommand.inputMove = Vector2.ClampMagnitude(m_gameplayActions.Move.ReadValue<Vector2>(), 1f);

//        m_currentFrameButtons = default;
//        m_currentFrameButtons.Or(Button.Sprint, m_gameplayActions.Sprint.IsPressed());
//        m_currentFrameButtons.Or(Button.Crouch, m_gameplayActions.Crouch.IsPressed());
//        m_currentFrameButtons.Or(Button.Fire, m_gameplayActions.Fire.IsPressed());
//        m_currentFrameButtons.Or(Button.ADS, m_gameplayActions.Fire.IsPressed());
//        m_currentFrameButtons.Or(Button.Reload, m_gameplayActions.Reload.IsPressed());
//        m_currentFrameButtons.Or(Button.WeaponSwap, m_gameplayActions.WeaponSwap.IsPressed());

//        CurrentUserCommand.buttons.flags |= m_currentFrameButtons.flags;
//    }

//    public static void EnterGameplay()
//    {
//        // We'll update the input manually before reading them in the update loop
//        InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
//    }

//    public static void ExitGameplay()
//    {
//        // Let the inputs system update itself because we won't be updating it manually
//        InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
//    }

//    public static void UpdateInputSystem()
//    {
//        if (InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsManually)
//        {
//            InputSystem.Update();
//        }
//    }
//}
