using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Lockstep.Game;
using Lockstep.Math;
using Lockstep.Util;
using NetMsg.Common;
using UnityEngine;


namespace Lockstep.Game
{
    [Serializable]
    public class EntityConfig 
    {
        public virtual object Entity { get; }
        public string PrefabPath;

        public void CopyTo(object dst)
        {
            if (Entity.GetType() != dst.GetType()) 
            {
                return;
            }

            FieldInfo[] fields = dst.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields) 
            {
                var type = field.FieldType;
                if (typeof(INeedBackup).IsAssignableFrom(type)) 
                {
                    CopyTo(field.GetValue(dst), field.GetValue(Entity));
                }
                else 
                {
                    field.SetValue(dst, field.GetValue(Entity));
                }
            }
        }

        void CopyTo(object dst, object src)
        {
            if (src.GetType() != dst.GetType()) 
            {
                return;
            }

            FieldInfo[] fields = dst.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields) 
            {
                var type = field.FieldType;
                field.SetValue(dst, field.GetValue(src));
            }
        }
    }

    [Serializable]
    public class EnemyConfig : EntityConfig 
    {
        public override object Entity => entity;
        public Enemy entity = new Enemy();
    }

    [Serializable]
    public class PlayerConfig : EntityConfig 
    {
        public override object Entity => entity;
        public Player entity = new Player();
    }

    [Serializable]
    public class SpawnerConfig : EntityConfig 
    {
        public override object Entity => entity;
        public Spawner entity = new Spawner();
    }
    
    [Serializable]
    public class CollisionConfig 
    {
        public LVector3 Pos;
        public LFloat WorldSize = new LFloat(60);
        public LFloat MinNodeSize = new LFloat(1);
        public LFloat Loosenessval = new LFloat(true, 1250);

        public LFloat Percent = new LFloat(true, 100);
        public int Count = 100;

        public int WhowTreeId = 0;

        public Vector2 ScrollPos;
        public bool IsShow = true;
        public bool[] CollisionMatrix = new bool[(int) EColliderLayer.EnumCount * (int) EColliderLayer.EnumCount];

        private string[] m_colliderLayerNames;

        public string[] ColliderLayerNames
        {
            get {
                if (m_colliderLayerNames == null || m_colliderLayerNames.Length == 0)
                {
                    var lst = new List<string>();
                    for (int i = 0; i < (int) EColliderLayer.EnumCount; i++)
                    {
                        lst.Add(((EColliderLayer) i).ToString());
                    }

                    m_colliderLayerNames = lst.ToArray();
                }

                return m_colliderLayerNames;
            }
        }

        public void SetColliderPair(int a, int b, bool val)
        {
            CollisionMatrix[a * (int) EColliderLayer.EnumCount + b] = val;
            CollisionMatrix[b * (int) EColliderLayer.EnumCount + a] = val;
        }

        public bool GetColliderPair(int a, int b)
        {
            return CollisionMatrix[a * (int) EColliderLayer.EnumCount + b];
        }
    }


    [CreateAssetMenu(menuName = "GameConfig")]
    public class GameConfig : ScriptableObject
    {
        public List<PlayerConfig> Player = new List<PlayerConfig>();
        public List<EnemyConfig> Enemies = new List<EnemyConfig>();
        public List<SpawnerConfig> Spawner = new List<SpawnerConfig>();
        public List<AnimatorConfig> Animators = new List<AnimatorConfig>();
        public List<SkillBoxConfig> Skills = new List<SkillBoxConfig>();

        public CollisionConfig CollisionConfig;
        public string RecorderFilePath;
        public string DumpStrPath;
        public Msg_G2C_GameStartInfo ClientModeInfo = new Msg_G2C_GameStartInfo();

        public void DoAwake()
        {
            foreach (var skill in Skills)
            {
                skill.CheckInit();
            }
        }

        private T GetConfig<T>(List<T> lst, int id) where T: EntityConfig
        {
            if (id < 0 || id >= lst.Count)
            {
                Debug.LogError("Miss " + typeof(T)  + " "+ id);
                return null;
            }

            return lst[id];
        }

        public EntityConfig GetEnemyConfig(int id)
        {
            return  GetConfig(Enemies, id);
        }

        public EntityConfig GetPlayerConfig(int id)
        {
            return  GetConfig(Player, id);
        }

        public EntityConfig GetSpawnerConfig(int id)
        {
            return  GetConfig(Spawner, id);
        }

        public AnimatorConfig GetAnimatorConfig(int id)
        {
            return (id < 0 ||id >= Animators.Count) ? null : Animators[id];
        }

        public SkillBoxConfig GetSkillConfig(int id)
        {
            return (id < 0 ||id >= Skills.Count) ? null : Skills[id];
        }
    }
}