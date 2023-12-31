using Lockstep.Math;
using Lockstep.Serialization;
using Lockstep.Util;
using UnityEngine;

namespace Lockstep.Game 
{
    public partial class PlayerInput : BaseFormater,IComponent 
    {
        public static PlayerInput Empty = new PlayerInput();

        public LVector2 mousePos;
        public LVector2 inputUV;
        public LVector2 InputLook;
        public ButtonBitField ButtonFlags;
        public bool isInputFire;
        public int skillId;
        public bool isSpeedUp;

        public override void Serialize(Serializer writer)
        {
            writer.Write(mousePos);
            writer.Write(inputUV);
            writer.Write(InputLook);
            writer.Write(ButtonFlags.flags);
            writer.Write(isInputFire);
            writer.Write(skillId);
            writer.Write(isSpeedUp);
        }

        public void Reset()
        {
            mousePos = LVector2.zero;
            inputUV = LVector2.zero;
            InputLook = LVector2.zero;
            ButtonFlags.flags = 0;
            isInputFire = false;
            skillId = 0;
            isSpeedUp = false;
        }

        public override void Deserialize(Deserializer reader)
        {
            mousePos = reader.ReadLVector2();
            inputUV = reader.ReadLVector2();
            InputLook = reader.ReadLVector2();
            ButtonFlags.flags = reader.ReadUInt32();
            isInputFire = reader.ReadBoolean();
            skillId = reader.ReadInt32();
            isSpeedUp = reader.ReadBoolean();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as PlayerInput;
            return Equals(other);
        }

        public bool Equals(PlayerInput other)
        {
            if (other == null) return false;
            if (mousePos != other.mousePos) return false;
            if (inputUV != other.inputUV) return false;
            if (InputLook != other.InputLook) return false;
            if (ButtonFlags.flags != other.ButtonFlags.flags) return false;
            if (isInputFire != other.isInputFire) return false;
            if (skillId != other.skillId) return false;
            if (isSpeedUp != other.isSpeedUp) return false;
            return true;
        }

        public PlayerInput Clone()
        {
            var tThis = this;
            return new PlayerInput()
            {
                mousePos = tThis.mousePos,
                inputUV = tThis.inputUV,
                isInputFire = tThis.isInputFire,
                skillId = tThis.skillId,
                isSpeedUp = tThis.isSpeedUp,
            };
        }
    }
}