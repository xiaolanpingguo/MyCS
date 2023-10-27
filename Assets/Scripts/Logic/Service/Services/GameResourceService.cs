using System;
using System.Collections.Generic;
using System.Reflection;
using Lockstep.Game;
using UnityEngine;

namespace Lockstep.Game 
{
    public class GameResourceService : BaseGameService, IGameResourceService
    {
        public string pathPrefix = "Prefabs/";
        private Dictionary<int, GameObject> m_id2Prefab = new Dictionary<int, GameObject>();

        public object LoadPrefab(int id)
        {
            return _LoadPrefab(id);
        }

        GameObject _LoadPrefab(int id)
        {
            if (m_id2Prefab.TryGetValue(id, out var val))
            {
                return val;
            }

            var config = _gameConfigService.GetEntityConfig(id);
            if (string.IsNullOrEmpty(config.PrefabPath))
            {
                return null;
            }

            var prefab = (GameObject)Resources.Load(pathPrefix + config.PrefabPath);
            m_id2Prefab[id] = prefab;
            PhysicSystem.Instance.RigisterPrefab(id, id < 10 ? (int) EColliderLayer.Hero : (int) EColliderLayer.Enemy);
            return prefab;
        }
    }
}