using Lockstep.Game;
using Lockstep.Math;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Lockstep.Game 
{
    public class PlayerView : EntityView, IPlayerView 
    {
        public Player Player;
        protected bool isDead => entity?.isDead ?? true;

        private CharacterController m_characterController;

        public override void BindEntity(BaseEntity e, BaseEntity oldEntity = null)
        {
            base.BindEntity(e, oldEntity);
            Player = e as Player;
            gameObject.AddComponent<CharacterController>();
            m_characterController = gameObject.GetComponent<CharacterController>();
        }

        protected override void Update()
        {
            var playerInput = Player.input;
            Vector3 move = new Vector3(playerInput.inputUV.x, 0, playerInput.inputUV.y);
            move.Normalize();
            move = move * Time.deltaTime * 5;
            move = transform.TransformDirection(move);
            m_characterController.Move(move);
            if (playerInput.inputUV.x <= 0.1f && playerInput.inputUV.y <= 0.1f)
            {
                //ActualSpeed = 0;
            }
        }
    }
}