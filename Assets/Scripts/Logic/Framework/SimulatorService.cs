#define DEBUG_FRAME_DELAY
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lockstep.ECS;
using Lockstep.Math;
using Lockstep.Serialization;
using Lockstep.Util;
using Lockstep.Game;
using NetMsg.Common;
#if UNITY_EDITOR
using UnityEngine;
#endif
using Debug = Lockstep.Logging.Debug;
using Logger = Lockstep.Logging.Logger;


namespace Lockstep.Game
{
    public class SimulatorService : BaseGameService, ISimulatorService, IDebugService
    {
        public static SimulatorService Instance { get; private set; }

        public int DebugRockbackToTick;

        public const long MinMissFrameReqTickDiff = 10;
        public const long MaxSimulationMsPerFrame = 20;
        public const int MaxPredictFrameCount = 30;

        public int PingVal => m_cmdBuffer?.PingVal ?? 0;
        public int DelayVal => m_cmdBuffer?.DelayVal ?? 0;

        // components
        public World World => m_world;
        private World m_world;
        private IFrameBuffer m_cmdBuffer;
        private HashHelper m_hashHelper;
        private DumpHelper m_dumpHelper;

        // game status
        private Msg_G2C_GameStartInfo m_gameStartInfo;
        public byte LocalActorId { get; private set; }
        private byte[] m_allActors;
        private int _actorCount => m_allActors.Length;
        private PlayerInput[] m_playerInputs => m_world.PlayerInputs;
        public bool IsRunning { get; set; }

        // frame count that need predict(TODO should change according current network's delay)
        public int FramePredictCount = 0; //~~~

        // game init timestamp
        public long _gameStartTimestampMs = -1;

        private int m_tickSinceGameStart;
        public int TargetTick => m_tickSinceGameStart + FramePredictCount;

        // input presend
        public int PreSendInputCount = 1; //~~~
        public int inputTick = 0;
        public int inputTargetTick => m_tickSinceGameStart + PreSendInputCount;

        //video mode
        private Msg_RepMissFrame m_videoFrames;
        private bool m_isInitVideo = false;
        private int m_tickOnLastJumpTo;
        private long m_timestampOnLastJumpToMs;
        private bool m_isDebugRollback = true;

        //refs 
        private IManagerContainer m_mgrContainer;
        private IServiceContainer m_serviceContainer;

        public int snapshotFrameInterval = 1;
        private bool m_hasRecvInputMsg;

        public SimulatorService()
        {
            Instance = this;
        }

        public override void InitReference(IServiceContainer serviceContainer, IManagerContainer mgrContainer)
        {
            base.InitReference(serviceContainer, mgrContainer);
            m_serviceContainer = serviceContainer;
            m_mgrContainer = mgrContainer;
        }

        public override void DoStart()
        {
            snapshotFrameInterval = 1;
            if (_constStateService.IsVideoMode) 
            {
                snapshotFrameInterval = _constStateService.SnapshotFrameInterval;
            }

            m_cmdBuffer = new FrameBuffer(this, _networkService, 2000, snapshotFrameInterval, MaxPredictFrameCount);
            m_world = new World();
            m_hashHelper = new HashHelper(m_serviceContainer, m_world, _networkService, m_cmdBuffer);
            m_dumpHelper = new DumpHelper(m_serviceContainer, m_world,m_hashHelper);
        }

        public override void DoDestroy()
        {
            IsRunning = false;
            m_dumpHelper.DumpAll();
        }

        public void OnGameCreate(int targetFps, byte localActorId, byte actorCount, bool isNeedRender = true)
        {
            FrameBuffer.__debugMainActorID = localActorId;
            var allActors = new byte[actorCount];
            for (byte i = 0; i < actorCount; i++)
            {
                allActors[i] = i;
            }

            Debug.Log($"GameCreate " + LocalActorId);

            //Init game status
            //_localActorId = localActorId;
            m_allActors = allActors;
            _constStateService.LocalActorId = LocalActorId;
            m_world.OnGameCreate(m_serviceContainer, m_mgrContainer);
            EventHelper.Trigger(EEvent.LevelLoadProgress, 1f);
        }

        public void StartSimulate()
        {
            if (IsRunning) 
            {
                Debug.LogError("Already started!");
                return;
            }

            IsRunning = true;
            if (_constStateService.IsClientMode)
            {
                _gameStartTimestampMs = LTime.realtimeSinceStartupMS;
            }

            m_world.StartSimulate(m_gameStartInfo, LocalActorId);
            Debug.Log($"World Start Simulate");
            EventHelper.Trigger(EEvent.SimulationStart, null);

            while (inputTick < PreSendInputCount) 
            {
                SendInputs(inputTick++);
            }
        }

        public void Trace(string msg, bool isNewLine = false, bool isNeedLogTrace = false)
        {
            m_dumpHelper.Trace(msg, isNewLine, isNeedLogTrace);
        }

        public void JumpTo(int tick)
        {
            if (tick + 1 == m_world.Tick || tick == m_world.Tick)
            {
                return;
            }

            tick = LMath.Min(tick, m_videoFrames.frames.Length - 1);
            var time = LTime.realtimeSinceStartupMS + 0.05f;
            if (!m_isInitVideo) 
            {
                _constStateService.IsVideoLoading = true;
                while (m_world.Tick < m_videoFrames.frames.Length)
                {
                    var sFrame = m_videoFrames.frames[m_world.Tick];
                    Simulate(sFrame, true);
                    if (LTime.realtimeSinceStartupMS > time) 
                    {
                        EventHelper.Trigger(EEvent.VideoLoadProgress, m_world.Tick * 1.0f / m_videoFrames.frames.Length);
                        return;
                    }
                }

                _constStateService.IsVideoLoading = false;
                EventHelper.Trigger(EEvent.VideoLoadDone);
                m_isInitVideo = true;
            }

            if (m_world.Tick > tick) 
            {
                RollbackTo(tick, m_videoFrames.frames.Length, false);
            }

            while (m_world.Tick <= tick) 
            {
                var sFrame = m_videoFrames.frames[m_world.Tick];
                Simulate(sFrame, false);
            }

            _viewService.RebindAllEntities();
            m_timestampOnLastJumpToMs = LTime.realtimeSinceStartupMS;
            m_tickOnLastJumpTo = tick;
        }

        public void RunVideo()
        {
            if (m_tickOnLastJumpTo == m_world.Tick) 
            {
                m_timestampOnLastJumpToMs = LTime.realtimeSinceStartupMS;
                m_tickOnLastJumpTo = m_world.Tick;
            }

            var frameDeltaTime = (LTime.timeSinceLevelLoad - m_timestampOnLastJumpToMs) * 1000;
            var targetTick = System.Math.Ceiling(frameDeltaTime / NetworkDefine.UPDATE_DELTATIME) + m_tickOnLastJumpTo;
            while (m_world.Tick <= targetTick)
            {
                if (m_world.Tick < m_videoFrames.frames.Length) 
                {
                    var sFrame = m_videoFrames.frames[m_world.Tick];
                    Simulate(sFrame, false);
                }
                else 
                {
                    break;
                }
            }
        }

        public void DoUpdate(float deltaTime)
        {
            if (!IsRunning) 
            {
                return;
            }

            if (m_hasRecvInputMsg)
            {
                if (_gameStartTimestampMs == -1) 
                {
                    _gameStartTimestampMs = LTime.realtimeSinceStartupMS;
                }
            }

            if (_gameStartTimestampMs <= 0)
            {
                return;
            }

            m_tickSinceGameStart = (int) ((LTime.realtimeSinceStartupMS - _gameStartTimestampMs) / NetworkDefine.UPDATE_DELTATIME);
            if (_constStateService.IsVideoMode) 
            {
                return;
            }

            if (DebugRockbackToTick > 0) 
            {
                GetService<ICommonStateService>().IsPause = true;
                RollbackTo(DebugRockbackToTick, 0, false);
                DebugRockbackToTick = -1;
            }

            if (_commonStateService.IsPause) 
            {
                return;
            }

            m_cmdBuffer.DoUpdate(deltaTime);

            var gameInputService = _inputService as GameInputService;
            gameInputService.Update();

            //client mode no network
            if (_constStateService.IsClientMode) 
            {
                DoClientUpdate();
            }
            else
            {
                while (inputTick <= inputTargetTick)
                {
                    SendInputs(inputTick++);
                }

                DoNormalUpdate();
            }
        }

        private void DoClientUpdate()
        {
            int maxRollbackCount = 5;
            if (m_isDebugRollback && m_world.Tick > maxRollbackCount && m_world.Tick % maxRollbackCount == 0) 
            {
                var rawTick = m_world.Tick;
                var revertCount = LRandom.Range(1, maxRollbackCount);
                for (int i = 0; i < revertCount; i++) 
                {
                    var input = new Msg_PlayerInput(m_world.Tick, LocalActorId, _inputService.GetInputCmds());
                    var frame = new ServerFrame()
                    {
                        tick = rawTick - i,
                        _inputs = new Msg_PlayerInput[] {input}
                    };

                    m_cmdBuffer.ForcePushDebugFrame(frame);
                }

                //_debugService.Trace("RollbackTo " + (m_world.Tick - revertCount));
                //if (!RollbackTo(m_world.Tick - revertCount, m_world.Tick))
                //{
                //    _commonStateService.IsPause = true;
                //    return;
                //}

                while (m_world.Tick < rawTick) 
                {
                    var sFrame = m_cmdBuffer.GetServerFrame(m_world.Tick);
                    Logging.Debug.Assert(sFrame != null && sFrame.tick == m_world.Tick,
                        $" logic error: server Frame  must exist tick {m_world.Tick}");
                    m_cmdBuffer.PushLocalFrame(sFrame);
                    Simulate(sFrame);
                    if (_commonStateService.IsPause) 
                    {
                        return;
                    }
                }
            }

            while (m_world.Tick < TargetTick) 
            {
                FramePredictCount = 0;
                var input = new Msg_PlayerInput(m_world.Tick, LocalActorId, _inputService.GetInputCmds());
                var frame = new ServerFrame()
                {
                    tick = m_world.Tick,
                    _inputs = new Msg_PlayerInput[] {input}
                };
                m_cmdBuffer.PushLocalFrame(frame);
                m_cmdBuffer.PushServerFrames(new ServerFrame[] {frame});
                Simulate(m_cmdBuffer.GetFrame(m_world.Tick));
                if (_commonStateService.IsPause) 
                {
                    return;
                }
            }
        }

        private void DoNormalUpdate()
        {
            //make sure client is not move ahead too much than server
            var maxContinueServerTick = m_cmdBuffer.MaxContinueServerTick;
            if ((m_world.Tick - maxContinueServerTick) > MaxPredictFrameCount) 
            {
                return;
            }

            var minTickToBackup = (maxContinueServerTick - (maxContinueServerTick % snapshotFrameInterval));

            // Pursue Server frames
            var deadline = LTime.realtimeSinceStartupMS + MaxSimulationMsPerFrame;
            while (m_world.Tick < m_cmdBuffer.CurTickInServer)
            {
                var tick = m_world.Tick;
                var sFrame = m_cmdBuffer.GetServerFrame(tick);
                if (sFrame == null) 
                {
                    OnPursuingFrame();
                    return;
                }

                m_cmdBuffer.PushLocalFrame(sFrame);
                Simulate(sFrame, tick == minTickToBackup);
                if (LTime.realtimeSinceStartupMS > deadline) 
                {
                    OnPursuingFrame();
                    return;
                }
            }

            if (_constStateService.IsPursueFrame) 
            {
                _constStateService.IsPursueFrame = false;
                EventHelper.Trigger(EEvent.PursueFrameDone);
            }

            // Roll back
            if (m_cmdBuffer.IsNeedRollback) 
            {
                RollbackTo(m_cmdBuffer.NextTickToCheck, maxContinueServerTick);
                CleanUselessSnapshot(System.Math.Min(m_cmdBuffer.NextTickToCheck - 1, m_world.Tick));

                minTickToBackup = System.Math.Max(minTickToBackup, m_world.Tick + 1);
                while (m_world.Tick <= maxContinueServerTick) 
                {
                    var sFrame = m_cmdBuffer.GetServerFrame(m_world.Tick);
                    Logging.Debug.Assert(sFrame != null && sFrame.tick == m_world.Tick,
                        $" logic error: server Frame  must exist tick {m_world.Tick}");
                    m_cmdBuffer.PushLocalFrame(sFrame);
                    Simulate(sFrame, m_world.Tick == minTickToBackup);
                }
            }

            //Run frames
            while (m_world.Tick <= TargetTick) 
            {
                var curTick = m_world.Tick;
                ServerFrame frame = null;
                var sFrame = m_cmdBuffer.GetServerFrame(curTick);
                if (sFrame != null) 
                {
                    frame = sFrame;
                }
                else 
                {
                    var cFrame = m_cmdBuffer.GetLocalFrame(curTick);
                    FillInputWithLastFrame(cFrame);
                    frame = cFrame;
                }

                m_cmdBuffer.PushLocalFrame(frame);
                Predict(frame, true);
            }

            m_hashHelper.CheckAndSendHashCodes();
        }

        void SendInputs(int curTick)
        {
            var input = new Msg_PlayerInput(curTick, LocalActorId, _inputService.GetInputCmds());
            var cFrame = new ServerFrame();
            var inputs = new Msg_PlayerInput[_actorCount];
            inputs[LocalActorId] = input;
            cFrame.Inputs = inputs;
            cFrame.tick = curTick;
            FillInputWithLastFrame(cFrame);
            m_cmdBuffer.PushLocalFrame(cFrame);
            //if (input.Commands != null) {
            //    var playerInput = new Deserializer(input.Commands[0].content).Parse<Lockstep.Game1.PlayerInput>();
            //    Debug.Log($"SendInput curTick{curTick} maxSvrTick{m_cmdBuffer.MaxServerTickInBuffer} m_tickSinceGameStart {m_tickSinceGameStart} uv {playerInput.inputUV}");
            //}
            if (curTick > m_cmdBuffer.MaxServerTickInBuffer) 
            {
                //TODO combine all history inputs into one Msg 
                //Debug.Log("SendInput " + curTick +" m_tickSinceGameStart " + m_tickSinceGameStart);
                m_cmdBuffer.SendInput(input);
            }
        }

        private void Simulate(ServerFrame frame, bool isNeedGenSnap = true)
        {
            Step(frame, isNeedGenSnap);
        }

        private void Predict(ServerFrame frame, bool isNeedGenSnap = true)
        {
            Step(frame, isNeedGenSnap);
        }

        private bool RollbackTo(int tick, int maxContinueServerTick, bool isNeedClear = true)
        {
            m_world.RollbackTo(tick, maxContinueServerTick, isNeedClear);
            var hash = _commonStateService.Hash;
            var curHash = m_hashHelper.CalcHash();
            if (hash != curHash) 
            {
                Debug.LogError($"tick:{tick} Rollback error: Hash isDiff oldHash ={hash}  curHash{curHash}");
#if UNITY_EDITOR
                m_dumpHelper.DumpToFile(true);
                return false;
#endif
            }

            return true;
        }

        void Step(ServerFrame frame, bool isNeedGenSnap = true)
        {
            //Debug.Log("Step: " + m_world.Tick + " TargetTick: " + TargetTick);
            _commonStateService.SetTick(m_world.Tick);
            var hash = m_hashHelper.CalcHash();
            _commonStateService.Hash = hash;
            _timeMachineService.Backup(m_world.Tick);
            DumpFrame(hash);
            hash = m_hashHelper.CalcHash(true);
            m_hashHelper.SetHash(m_world.Tick, hash);

            ProcessInputQueue(frame);
            m_world.Step(isNeedGenSnap);
            m_dumpHelper.OnFrameEnd();

            var tick = m_world.Tick;
            m_cmdBuffer.SetClientTick(tick);

            //clean useless snapshot
            if (isNeedGenSnap && tick % snapshotFrameInterval == 0)
            {
                CleanUselessSnapshot(System.Math.Min(m_cmdBuffer.NextTickToCheck - 1, m_world.Tick));
            }
        }

        private void CleanUselessSnapshot(int tick)
        {
            //TODO
        }

        private void DumpFrame(int hash)
        {
            if (_constStateService.IsClientMode) 
            {
                m_dumpHelper.DumpFrame(!m_hashHelper.TryGetValue(m_world.Tick, out var val));
            }
            else 
            {
                m_dumpHelper.DumpFrame(true);
            }
        }

        private void FillInputWithLastFrame(ServerFrame frame)
        {
            int tick = frame.tick;
            var inputs = frame.Inputs;
            var lastServerInputs = tick == 0 ? null : m_cmdBuffer.GetFrame(tick - 1)?.Inputs;
            var myInput = inputs[LocalActorId];
            //fill inputs with last frame's input (Input predict)
            for (int i = 0; i < _actorCount; i++) 
            {
                inputs[i] = new Msg_PlayerInput(tick, m_allActors[i], lastServerInputs?[i]?.Commands);
            }

            inputs[LocalActorId] = myInput;
        }

        private void ProcessInputQueue(ServerFrame frame)
        {
            var inputs = frame.Inputs;
            foreach (var playerInput in m_playerInputs)
            {
                playerInput.Reset();
            }

            foreach (var input in inputs) 
            {
                if (input.Commands == null)
                {
                    continue;
                }

                if (input.ActorId >= m_playerInputs.Length)
                {
                    continue;
                }

                var inputEntity = m_playerInputs[input.ActorId];
                foreach (var command in input.Commands)
                {
                    Logger.Trace(this, input.ActorId + " >> " + input.Tick + ": " + input.Commands.Count());
                    _inputService.Execute(command, inputEntity);
                }
            }
        }

        void OnPursuingFrame()
        {
            _constStateService.IsPursueFrame = true;
            Debug.Log($"PurchaseServering curTick:" + m_world.Tick);
            var progress = m_world.Tick * 1.0f / m_cmdBuffer.CurTickInServer;
            EventHelper.Trigger(EEvent.PursueFrameProcess, progress);
        }

        #region NetEvents

        void OnEvent_BorderVideoFrame(object param)
        {
            m_videoFrames = param as Msg_RepMissFrame;
        }

        void OnEvent_OnServerFrame(object param)
        {
            var msg = param as Msg_ServerFrames;
            m_hasRecvInputMsg = true;
            m_cmdBuffer.PushServerFrames(msg.frames);
        }

        void OnEvent_OnServerMissFrame(object param)
        {
            Debug.Log($"OnEvent_OnServerMissFrame");
            var msg = param as Msg_RepMissFrame;
            m_cmdBuffer.PushMissServerFrames(msg.frames, false);
        }

        void OnEvent_OnPlayerPing(object param)
        {
            var msg = param as Msg_G2C_PlayerPing;
            m_cmdBuffer.OnPlayerPing(msg);
        }

        void OnEvent_OnServerHello(object param)
        {
            var msg = param as Msg_G2C_Hello;
            LocalActorId = msg.LocalId;
            Debug.Log("OnEvent_OnServerHello " + LocalActorId);
        }

        void OnEvent_OnGameCreate(object param)
        {
            if (param is Msg_G2C_Hello msg)
            {
                OnGameCreate(60, msg.LocalId, msg.UserCount);
            }

            if (param is Msg_G2C_GameStartInfo smsg)
            {
                m_gameStartInfo = smsg;
                OnGameCreate(60, 0, smsg.UserCount);
            }

            EventHelper.Trigger(EEvent.SimulationInit, null);
        }

        void OnEvent_OnAllPlayerFinishedLoad(object param)
        {
            Debug.Log($"OnEvent_OnAllPlayerFinishedLoad");
            StartSimulate();
        }

        void OnEvent_LevelLoadDone(object param)
        {
            Debug.Log($"OnEvent_LevelLoadDone " + _constStateService.IsReconnecting);
            if (_constStateService.IsReconnecting || _constStateService.IsVideoMode || _constStateService.IsClientMode) 
            {
                StartSimulate();
            }
        }

        #endregion
    }
}