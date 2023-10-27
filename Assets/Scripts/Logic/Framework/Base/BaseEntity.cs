using System;
using System.Collections.Generic;
using System.Linq;
using Lockstep;
using Lockstep.Collision2D;
using Lockstep.Game;
using Lockstep.Math;
using Debug = Lockstep.Logging.Debug;

namespace Lockstep.Game
{
    [Serializable]
    [NoBackup]
    public partial class BaseEntity : BaseLifeCycle, IEntity, ILPTriggerEventHandler
    {
        public int EntityId;
        public int PrefabId;
        public CTransform2D transform = new CTransform2D();

        [NoBackup] public object engineTransform;
        protected List<BaseComponent> m_allComponents;

        [ReRefBackup] public IGameStateService GameStateService { get; set; }
        [ReRefBackup] public IServiceContainer ServiceContainer { get; set; }
        [ReRefBackup] public IDebugService DebugService { get; set; }
        [ReRefBackup] public IEntityView EntityView;
        
        public T GetService<T>() where T : IService
        {
            return ServiceContainer.GetService<T>();
        }
        
        public void DoBindRef()
        {
            BindRef();
        }

        public virtual void OnRollbackDestroy()
        {
            EntityView?.OnRollbackDestroy();
            EntityView = null;
            engineTransform = null;
        }

        protected virtual void BindRef()
        {
            m_allComponents?.Clear();
        }

        protected void RegisterComponent(BaseComponent comp)
        {
            if (m_allComponents == null) 
            {
                m_allComponents = new List<BaseComponent>();
            }

            m_allComponents.Add(comp);
            comp.BindEntity(this);
        }

        public override void DoAwake()
        {
            if (m_allComponents == null) return;
            foreach (var comp in m_allComponents)
            {
                comp.DoAwake();
            }
        }

        public override void DoStart()
        {
            if (m_allComponents == null)
            {
                return;
            }

            foreach (var comp in m_allComponents)
            {
                comp.DoStart();
            }
        }

        public override void DoUpdate(LFloat deltaTime)
        {
            if (m_allComponents == null)
            {
                return;
            }

            foreach (var comp in m_allComponents)
            {
                comp.DoUpdate(deltaTime);
            }
        }

        public override void DoDestroy()
        {
            if (m_allComponents == null)
            {
                return;
            }

            foreach (var comp in m_allComponents)
            {
                comp.DoDestroy();
            }
        }

        public virtual void OnLPTriggerEnter(ColliderProxy other){ }
        public virtual void OnLPTriggerStay(ColliderProxy other){ }
        public virtual void OnLPTriggerExit(ColliderProxy other){ }
    }
}