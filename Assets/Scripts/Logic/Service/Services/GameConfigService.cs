using System.Collections.Generic;
using Lockstep.Game;
using Lockstep.Math;
using NetMsg.Common;
using UnityEngine;
using Debug = Lockstep.Logging.Debug;


namespace Lockstep.Game 
{
    public class GameConfigService : BaseGameService, IGameConfigService 
    {
        public string configPath = "GameConfig";
        private GameConfig m_config;

        public override void DoAwake(IServiceContainer container)
        {
            m_config = Resources.Load<GameConfig>(configPath);
            m_config.DoAwake();
        }

        public EntityConfig GetEntityConfig(int id)
        {
            if (id >= 100)
            {
                return m_config.GetSpawnerConfig(id - 100);
            }
            if (id >= 10)
            {
                return m_config.GetEnemyConfig(id - 10);
            }

            return m_config.GetPlayerConfig(id);
        }

        public AnimatorConfig GetAnimatorConfig(int id)
        {
            return m_config.GetAnimatorConfig(id - 1);
        }

        public SkillBoxConfig GetSkillConfig(int id)
        {
            return m_config.GetSkillConfig(id - 1);
        }

        public CollisionConfig CollisionConfig => m_config.CollisionConfig;
        public string RecorderFilePath => m_config.RecorderFilePath;
        public string DumpStrPath => m_config.DumpStrPath;
        public Msg_G2C_GameStartInfo ClientModeInfo => m_config.ClientModeInfo;
    }
}