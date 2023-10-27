#define DEBUG_EVENT_TRIGGER
#if UNITY_EDITOR || DEBUG_EVENT_TRIGGER
#define _DEBUG_EVENT_TRIGGER
#endif
using System.Collections.Generic;


namespace Lockstep 
{
    public delegate void GlobalEventHandler(object param);
    public delegate void NetMsgHandler(object param);

    public partial class EventHelper 
    {
        private static Dictionary<int, List<GlobalEventHandler>> m_allListeners = new Dictionary<int, List<GlobalEventHandler>>();
        private static Queue<MsgInfo> m_allPendingMsgs = new Queue<MsgInfo>();
        private static Queue<ListenerInfo> m_allPendingListeners = new Queue<ListenerInfo>();
        private static Queue<EEvent> m_allNeedRemoveTypes = new Queue<EEvent>();

        private static bool IsTriggingEvent;

        public static void RemoveAllListener(EEvent type)
        {
            if (IsTriggingEvent) 
            {
                m_allNeedRemoveTypes.Enqueue(type);
                return;
            }

            m_allListeners.Remove((int) type);
        }

        public static void AddListener(EEvent type, GlobalEventHandler listener)
        {
            if (IsTriggingEvent) 
            {
                m_allPendingListeners.Enqueue(new ListenerInfo(true, type, listener));
                return;
            }

            var itype = (int) type;
            if (m_allListeners.TryGetValue(itype, out var tmplst)) 
            {
                tmplst.Add(listener);
            }
            else
            {
                var lst = new List<GlobalEventHandler>();
                lst.Add(listener);
                m_allListeners.Add(itype, lst);
            }
        }

        public static void RemoveListener(EEvent type, GlobalEventHandler listener)
        {
            if (IsTriggingEvent)
            {
                m_allPendingListeners.Enqueue(new ListenerInfo(false, type, listener));
                return;
            }

            var itype = (int) type;
            if (m_allListeners.TryGetValue(itype, out var tmplst)) 
            {
                if (tmplst.Remove(listener)) 
                {
                    if (tmplst.Count == 0) 
                    {
                        m_allListeners.Remove(itype);
                    }

                    return;
                }
            }
        }

        public static void Trigger(EEvent type, object param = null)
        {
            if (IsTriggingEvent)
            {
                m_allPendingMsgs.Enqueue(new MsgInfo(type, param));
                return;
            }

            var itype = (int) type;
            if (m_allListeners.TryGetValue(itype, out var tmplst)) 
            {
                IsTriggingEvent = true;
                foreach (var listener in tmplst.ToArray()) 
                { 
                    //TODO 替换成其他更好的方式 避免gc
                    listener?.Invoke(param);
                }
            }

            IsTriggingEvent = false;
            while (m_allPendingListeners.Count > 0) 
            {
                var msgInfo = m_allPendingListeners.Dequeue();
                if (msgInfo.isRegister) 
                {
                    AddListener(msgInfo.type, msgInfo.param);
                }
                else 
                {
                    RemoveListener(msgInfo.type, msgInfo.param);
                }
            }

            while (m_allNeedRemoveTypes.Count > 0) 
            {
                var rmType = m_allNeedRemoveTypes.Dequeue();
                RemoveAllListener(rmType);
            }

            while (m_allPendingMsgs.Count > 0) 
            {
                var msgInfo = m_allPendingMsgs.Dequeue();
                Trigger(msgInfo.type, msgInfo.param);
            }
        }

        public struct MsgInfo 
        {
            public EEvent type;
            public object param;

            public MsgInfo(EEvent type, object param)
            {
                this.type = type;
                this.param = param;
            }
        }

        public struct ListenerInfo 
        {
            public bool isRegister;
            public EEvent type;
            public GlobalEventHandler param;

            public ListenerInfo(bool isRegister, EEvent type, GlobalEventHandler param){
                this.isRegister = isRegister;
                this.type = type;
                this.param = param;
            }
        }
    }
}