﻿using System;
using System.Linq;
using System.Reflection;
using System.Web;
using Xlnt.Stuff;

namespace Concoct
{
    public class ConcoctApplication 
    {        
        readonly ConcoctConfiguration config;
        MvcHost host;

        public ConcoctApplication(ConcoctConfiguration config) {
            this.config = config;
        }

        public void OnStart(string[] args) {
            try {
                var site = Assembly.LoadFrom(config.ApplicationAssemblyPath);
                var types = site.GetTypes();
                var httpApplicationType = types.Where(x => x.IsTypeOf<HttpApplication>()).First();
                host = MvcHost.Create(config.GetEndPoint(), config.VirtualDirectoryOrPrefix, Environment.CurrentDirectory, httpApplicationType);

                host.Start();
                Console.WriteLine("Listening for connections.");

            } catch(ReflectionTypeLoadException loadError) {
                Console.Error.WriteLine("Error applications.");
                foreach(var item in loadError.LoaderExceptions) {
                    Console.Error.WriteLine(item);
                }
                throw new ApplicationException("Failed to load application", loadError);
            }
        }

        public void OnStop() {
            host.Stop();
        }
    }
}