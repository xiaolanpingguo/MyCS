using System;
using System.Threading;
using Lockstep.Math;
using Lockstep.Util;
using Lockstep.Network;
using NetMsg.Common;
using UnityEngine;
using System.ComponentModel.Design;

namespace Lockstep.Game 
{
    [Serializable]
    public class Launcher : ILifeCycle 
    {
        public static Launcher Instance { get; private set; }

        public int CurTick => m_serviceContainer.GetService<ICommonStateService>().Tick;
        public bool IsRunVideo => m_constStateService.IsRunVideo;
        public bool IsVideoMode => m_constStateService.IsVideoMode;
        public bool IsClientMode => m_constStateService.IsClientMode;
        public object transform;

        public int JumpToTick = 10;
        public string RecordPath;
        public int MaxRunTick = int.MaxValue;
        public Msg_G2C_GameStartInfo GameStartInfo;
        public Msg_RepMissFrame FramesInfo;

        private ServiceContainer m_serviceContainer;
        private ManagerContainer m_mgrContainer;
        private TimeMachineContainer m_timeMachineContainer;
        private IEventRegisterService m_eventRegisterService;
        private SimulatorService m_simulatorService = new SimulatorService();
        private NetworkService m_networkService = new NetworkService();
        private IConstStateService m_constStateService;
        private OneThreadSynchronizationContext m_syncContext; 


        public void DoAwake(IServiceContainer services)
        {
            m_syncContext = new OneThreadSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(m_syncContext);
            Utils.StartServices();
            if (Instance != null) 
            {
                Debug.LogError("LifeCycle Error: Awake more than once!!");
                return;
            }

            Instance = this;
            m_serviceContainer = services as ServiceContainer;
            m_eventRegisterService = new EventRegisterService();
            m_mgrContainer = new ManagerContainer();
            m_timeMachineContainer = new TimeMachineContainer();

            //AutoCreateManagers;
            var svcs = m_serviceContainer.GetAllServices();
            foreach (var service in svcs) 
            {
                m_timeMachineContainer.RegisterTimeMachine(service as ITimeMachine);
                if (service is BaseService baseService) 
                {
                    m_mgrContainer.RegisterManager(baseService);
                }
            }

            m_serviceContainer.RegisterService(m_timeMachineContainer);
            m_serviceContainer.RegisterService(m_eventRegisterService);
        }

        public void DoStart()
        {
            foreach (var mgr in m_mgrContainer.AllMgrs)
            {
                mgr.InitReference(m_serviceContainer, m_mgrContainer);
            }

            // register events
            foreach (var mgr in m_mgrContainer.AllMgrs)
            {
                m_eventRegisterService.RegisterEvent<EEvent, GlobalEventHandler>("OnEvent_", "OnEvent_".Length, EventHelper.AddListener, mgr);
            }

            foreach (var mgr in m_mgrContainer.AllMgrs)
            {
                mgr.DoAwake(m_serviceContainer);
            }

            m_simulatorService = m_serviceContainer.GetService<ISimulatorService>() as SimulatorService;
            m_networkService = m_serviceContainer.GetService<INetworkService>() as NetworkService;
            m_constStateService = m_serviceContainer.GetService<IConstStateService>();
            m_constStateService = m_serviceContainer.GetService<IConstStateService>();
            if (IsVideoMode)
            {
                m_constStateService.SnapshotFrameInterval = 20;
                //OpenRecordFile(RecordPath);
            }

            foreach (var mgr in m_mgrContainer.AllMgrs) 
            {
                mgr.DoStart();
            }

            //Debug.Log("Before StartGame _IdCounter" + BaseEntity.IdCounter);
            //if (!IsReplay && !IsClientMode)
            //{
            //    netClient = new NetClient();
            //    netClient.Start();
            //    netClient.Send(new Msg_JoinRoom() {name = Application.dataPath});
            //}
            //else
            //{
            //    StartGame(0, playerServerInfos, localPlayerId);
            //}

            if (IsVideoMode)
            {
                EventHelper.Trigger(EEvent.BorderVideoFrame, FramesInfo);
                EventHelper.Trigger(EEvent.OnGameCreate, GameStartInfo);
            }
            else if (IsClientMode)
            {
                GameStartInfo = m_serviceContainer.GetService<IGameConfigService>().ClientModeInfo;
                EventHelper.Trigger(EEvent.OnGameCreate, GameStartInfo);
                EventHelper.Trigger(EEvent.LevelLoadDone, GameStartInfo);
            }
        }

        public void DoUpdate(float fDeltaTime)
        {
            m_syncContext.Update();
            Utils.UpdateServices();
            var deltaTime = fDeltaTime.ToLFloat();
            m_networkService.DoUpdate(deltaTime);
            if (IsVideoMode && IsRunVideo && CurTick < MaxRunTick) 
            {
                m_simulatorService.RunVideo();
                return;
            }

            if (IsVideoMode && !IsRunVideo) 
            {
                m_simulatorService.JumpTo(JumpToTick);
            }

            m_simulatorService.DoUpdate(fDeltaTime);
        }

        public void DoDestroy()
        {
            if (Instance == null) return;
            foreach (var mgr in m_mgrContainer.AllMgrs) 
            {
                mgr.DoDestroy();
            }

            Instance = null;
        }

        public void OnApplicationQuit()
        {
            DoDestroy();
        }
    }
}