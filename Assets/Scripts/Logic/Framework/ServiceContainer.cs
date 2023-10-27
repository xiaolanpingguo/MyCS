using System;
using System.Collections.Generic;
using System.Linq;

namespace Lockstep.Game {
    public class ServiceContainer : IServiceContainer 
    {
        protected Dictionary<Type, IService> m_allServices = new Dictionary<Type, IService>();

        public IService[] GetAllServices()
        {
            return m_allServices.Values.ToArray();
        }

        public void RegisterService(IService service, bool overwriteExisting = true)
        {
            var interfaceTypes = service.GetType().FindInterfaces((type, criteria) =>
                    type.GetInterfaces().Any(t => t == typeof(IService)), service)
                .ToArray();

            foreach (var type in interfaceTypes) 
            {
                if (!m_allServices.ContainsKey(type))
                {
                    m_allServices.Add(type, service);
                }
                else if (overwriteExisting) 
                {
                    m_allServices[type] = service;
                }
            }
        }

        public T GetService<T>() where T : IService
        {
            var key = typeof(T);
            if (!m_allServices.ContainsKey(key)) 
            {
                return default(T);
            }
            else 
            {
                return (T) m_allServices[key];
            }
        }
    }
}