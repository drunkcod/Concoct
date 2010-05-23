using System;
using System.Collections.Generic;

namespace Concoct.Samples.ServiceDashboard.Models
{
    class ServiceContainer
    {
        public List<ServiceEntry> Services = new List<ServiceEntry>();

    }

    static class Singleton<T> where T : new()
    {
        static T value;
        public static T Instance {
            get {
                if (value == null)
                    value = new T();
                return value;
            }
        }
    }

    public class ServiceEntry
    {
        public string Id;
    }
}