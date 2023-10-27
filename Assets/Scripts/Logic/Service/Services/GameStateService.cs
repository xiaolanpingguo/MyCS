using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lockstep.Collision2D;
using Lockstep.Logging;
using Lockstep.Math;
using Lockstep.Game;
using Lockstep.Serialization;
using Lockstep.Util;


namespace Lockstep.Game 
{
    public partial class GameStateService : BaseGameService, IGameStateService 
    {
        private Dictionary<int,GameState> m_tick2State = new Dictionary<int,GameState>();
        private GameState m_curGameState;
        private Dictionary<Type, IList> m_type2Entities = new Dictionary<Type, IList>();
        private Dictionary<int, BaseEntity> m_id2Entities = new Dictionary<int, BaseEntity>();
        private Dictionary<int, Serializer> m_tick2Backup = new Dictionary<int, Serializer>();

        private void AddEntity<T>(T e) where T : BaseEntity
        {
            if (typeof(T) == typeof(Player))
            {
                int i = 0;
                Debug.Log("Add Player");
            }

            var t = e.GetType();
            if (m_type2Entities.TryGetValue(t, out var lstObj)) 
            {
                var lst = lstObj as List<T>;
                lst.Add(e);
            }
            else
            {
                var lst = new List<T>();
                m_type2Entities.Add(t, lst);
                lst.Add(e);
            }

            m_id2Entities[e.EntityId] = e;
        }

        private void RemoveEntity<T>(T e) where T : BaseEntity
        {
            var t = e.GetType();
            if (m_type2Entities.TryGetValue(t, out var lstObj)) 
            {
                lstObj.Remove(e);
                m_id2Entities.Remove(e.EntityId);
            }
            else 
            {
                Debug.LogError("Try remove a deleted Entity" + e);
            }
        }

        private List<T> GetEntities<T>()
        {
            var t = typeof(T);
            if (m_type2Entities.TryGetValue(t, out var lstObj)) 
            {
                return lstObj as List<T>;
            }
            else 
            {
                var lst = new List<T>();
                m_type2Entities.Add(t, lst);
                return lst;
            }
        }

        public Enemy[] GetEnemies()
        {
            return GetEntities<Enemy>().ToArray();
        }

        public Player[] GetPlayers()
        {
            return GetEntities<Player>().ToArray();
        }

        public Spawner[] GetSpawners()
        {
            return GetEntities<Spawner>().ToArray(); //TODO Cache
        }

        public object GetEntity(int id)
        {
            if (m_id2Entities.TryGetValue(id, out var val)) 
            {
                return val;
            }

            return null;
        }

        public T CreateEntity<T>(int prefabId, LVector3 position) where T : BaseEntity, new()
        {
            var baseEntity = new T();
            _gameConfigService.GetEntityConfig(prefabId)?.CopyTo(baseEntity);
            baseEntity.EntityId = _idService.GenId();
            baseEntity.PrefabId = prefabId;
            baseEntity.GameStateService = _gameStateService;
            baseEntity.ServiceContainer = _serviceContainer;
            baseEntity.DebugService = _debugService;
            baseEntity.transform.Pos3 = position;
            _debugService.Trace($"CreateEntity {prefabId} pos {prefabId} entityId:{baseEntity.EntityId}");
            baseEntity.DoBindRef();
            if (baseEntity is Entity entity) 
            {
                PhysicSystem.Instance.RegisterEntity(prefabId, entity);
            }

            baseEntity.DoAwake();
            baseEntity.DoStart();
            _gameViewService.BindView(baseEntity);
            AddEntity(baseEntity);
            return baseEntity;
        }

        public void DestroyEntity(BaseEntity entity)
        {
            RemoveEntity(entity);
        }

        public override void Backup(int tick)
        {
            base.Backup(tick);
            m_tick2State[tick] = m_curGameState;
            Serializer writer = new Serializer();
            writer.Write(_commonStateService.Hash); //hash
            BackUpEntities(GetPlayers(), writer);
            BackUpEntities(GetEnemies(), writer);
            BackUpEntities(GetSpawners(), writer);
            m_tick2Backup[tick] = writer;
        }

        public override void RollbackTo(int tick)
        {
            base.RollbackTo(tick);
            m_curGameState = m_tick2State[tick];
            if (m_tick2Backup.TryGetValue(tick, out var backupData)) 
            {
                //.TODO reduce the unnecessary create and destroy 
                var reader = new Deserializer(backupData.Data);
                var hash = reader.ReadInt32();
                _commonStateService.Hash = hash;

                var oldId2Entity = m_id2Entities;
                m_id2Entities = new Dictionary<int, BaseEntity>();
                m_type2Entities.Clear();

                //. Recover Entities
                RecoverEntities(new List<Player>(), reader);
                RecoverEntities(new List<Enemy>(), reader);
                RecoverEntities(new List<Spawner>(), reader);

                //. Rebind Ref
                foreach (var entity in m_id2Entities.Values)
                {
                    entity.GameStateService = _gameStateService;
                    entity.ServiceContainer = _serviceContainer;
                    entity.DebugService = _debugService;
                    entity.DoBindRef();
                }

                //. Rebind Views 
                foreach (var pair in m_id2Entities)
                {
                    BaseEntity oldEntity = null;
                    if (oldId2Entity.TryGetValue(pair.Key, out var poldEntity))
                    {
                        oldEntity = poldEntity;
                        oldId2Entity.Remove(pair.Key);
                    }

                    _gameViewService.BindView(pair.Value, oldEntity);
                }
                
                //. Unbind Entity views
                foreach (var pair in oldId2Entity)
                {
                    _gameViewService.UnbindView(pair.Value);
                }
            }
            else 
            {
                Debug.LogError($"Miss backup data  cannot rollback! {tick}");
            }
        }

        public override void Clean(int maxVerifiedTick)
        {
            base.Clean(maxVerifiedTick);
        }

        void BackUpEntities<T>(T[] lst, Serializer writer) where T : BaseEntity, IBackup, new()
        {
            writer.Write(lst.Length);
            foreach (var item in lst)
            {
                item.WriteBackup(writer);
            }
        }

        List<T> RecoverEntities<T>(List<T> lst, Deserializer reader) where T : BaseEntity, IBackup, new()
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++) 
            {
                var t = new T();
                lst.Add(t);
                t.ReadBackup(reader);
            }

            m_type2Entities[typeof(T)] = lst;
            foreach (var e in lst)
            {
                m_id2Entities[e.EntityId] = e;
            }

            return lst;
        }

        public struct GameState
        {
            public LFloat RemainTime;
            public LFloat DeltaTime;
            public int MaxEnemyCount;
            public int CurEnemyCount;
            public int CurEnemyId;

            public int GetHash(ref int idx)
            {
                int hash = 1;
                hash += CurEnemyCount.GetHash(ref idx) * PrimerLUT.GetPrimer(idx++);
                hash += MaxEnemyCount.GetHash(ref idx) * PrimerLUT.GetPrimer(idx++);
                hash += CurEnemyId.GetHash(ref idx) * PrimerLUT.GetPrimer(idx++);
                return hash;
            }
        }

        public LFloat RemainTime
        {
            get => m_curGameState.RemainTime;
            set => m_curGameState.RemainTime = value;
        }

        public LFloat DeltaTime
        {
            get => m_curGameState.DeltaTime;
            set => m_curGameState.DeltaTime = value;
        }

        public int MaxEnemyCount
        {
            get => m_curGameState.MaxEnemyCount;
            set => m_curGameState.MaxEnemyCount = value;
        }

        public int CurEnemyCount
        {
            get => m_curGameState.CurEnemyCount;
            set => m_curGameState.CurEnemyCount = value;
        }

        public int CurEnemyId
        {
            get => m_curGameState.CurEnemyId;
            set => m_curGameState.CurEnemyId = value;
        }
    }
}