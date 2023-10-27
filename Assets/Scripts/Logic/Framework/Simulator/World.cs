﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lockstep.Math;
using Lockstep.Game;
using NetMsg.Common;
using UnityEngine;
using Debug = Lockstep.Logging.Debug;
using Profiler = Lockstep.Util.Profiler;


namespace Lockstep.Game 
{
    public class World : BaseSystem 
    {
        public static World Instance { get; private set; }
        public int Tick { get; set; }
        public PlayerInput[] PlayerInputs => _gameStateService.GetPlayers().Select(a => a.input).ToArray();
        public static Player MyPlayer;
        public static object MyPlayerTrans => MyPlayer?.engineTransform;

        private List<BaseSystem> m_systems = new List<BaseSystem>();
        private bool m_hasStart = false;


        public void RollbackTo(int tick, int maxContinueServerTick, bool isNeedClear = true)
        {
            if (tick < 0) 
            {
                Debug.LogError("Target Tick invalid!" + tick);
                return;
            }

            Debug.Log($" Rollback diff:{Tick - tick} From{Tick}->{tick}  maxContinueServerTick:{maxContinueServerTick} {isNeedClear}");
            _timeMachineService.RollbackTo(tick);
            _commonStateService.SetTick(tick);
            Tick = tick;
        }

        public void OnGameCreate(IServiceContainer serviceContainer, IManagerContainer mgrContainer)
        {
            Instance = this;
            _serviceContainer = serviceContainer;

            RegisterSystems();
            if (!serviceContainer.GetService<IConstStateService>().IsVideoMode)
            {
                RegisterSystem(new TraceLogSystem());
            }

            InitReference(serviceContainer, mgrContainer);
            foreach (var mgr in m_systems)
            {
                mgr.InitReference(serviceContainer, mgrContainer);
            }

            foreach (var mgr in m_systems)
            {
                mgr.DoAwake(serviceContainer);
            }

            DoAwake(serviceContainer);
            foreach (var mgr in m_systems)
            {
                mgr.DoStart();
            }

            DoStart();
        }

        public void StartSimulate(Msg_G2C_GameStartInfo gameStartInfo, int localPlayerId)
        {
            if (m_hasStart)
            {
                return;
            }

            m_hasStart = true;
            var playerInfos = gameStartInfo.UserInfos;
            var playerCount = playerInfos.Length;
            string traceLogPath = "";
#if UNITY_STANDALONE_OSX
            traceLogPath = $"/tmp/LPDemo/Dump_{localPlayerId}.txt";
#else
            traceLogPath = $"c:/tmp/LPDemo/Dump_{localPlayerId}.txt";
#endif
            Debug.TraceSavePath = traceLogPath;

            _debugService.Trace("CreatePlayer " + playerCount);

            //create Players 
            for (int i = 0; i < playerCount; i++)
            {
                var PrefabId = 0; //TODO
                var initPos = LVector2.zero; //TODO
                var player = _gameStateService.CreateEntity<Player>(PrefabId, initPos);
                player.localId = i;
            }

            var allPlayers = _gameStateService.GetPlayers();
            MyPlayer = allPlayers[localPlayerId];
        }

        public override void DoDestroy()
        {
            foreach (var mgr in m_systems)
            {
                mgr.DoDestroy();
            }

            Debug.FlushTrace();
        }

        public override void OnApplicationQuit()
        {
            DoDestroy();
        }

        public void Step(bool isNeedGenSnap = true)
        {
            if (_commonStateService.IsPause)
            {
                return;
            }

            var deltaTime = new LFloat(true, 30);
            foreach (var system in m_systems)
            {
                if (system.enable)
                {
                    system.DoUpdate(deltaTime);
                }
            }

            Tick++;
        }

        public void RegisterSystems()
        {
            RegisterSystem(new HeroSystem());
            RegisterSystem(new EnemySystem());
            RegisterSystem(new PhysicSystem());
            RegisterSystem(new HashSystem());
        }

        public void RegisterSystem(BaseSystem mgr)
        {
            m_systems.Add(mgr);
        }
    }
}