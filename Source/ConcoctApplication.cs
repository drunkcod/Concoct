using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Concoct.Web.Routing;
using Xlnt.Stuff;

namespace Concoct
{
    public class ApplicationHost : MarshalByRefObject 
    { 
        MvcHost host;

        public void Start(ConcoctConfiguration config)  {
            try {
                var site = Assembly.LoadFrom(config.ApplicationAssemblyPath);
                var types = site.GetTypes();
                var httpApplicationType = types.Where(x => x.IsTypeOf<HttpApplication>()).First();
                
                FileRouteHandler.MapPath = path => path.Replace("~", config.WorkingDirectory);

                host = MvcHost.Create(
                    config.GetEndPoint(), 
                    config.VirtualDirectoryOrPrefix, 
                    config.WorkingDirectory, 
                    httpApplicationType);

                host.Start();

            } catch(ReflectionTypeLoadException loadError) {
                Console.Error.WriteLine("Error applications.");
                foreach(var item in loadError.LoaderExceptions) {
                    Console.Error.WriteLine(item);
                }
                throw new ApplicationException("Failed to load application", loadError);
            }
        }

        public void Stop() { host.Stop(); }
    }

    public class ConcoctApplication : MarshalByRefObject
    {        
        readonly ConcoctConfiguration config;
        ApplicationHost host;

        public ConcoctApplication(ConcoctConfiguration config) {
            this.config = config;
        }

        public void OnStart(string[] args) {
            host = CreateHost();
            host.Start(config);
        }

        ApplicationHost CreateHost() {
            var ad = AppDomain.CreateDomain("Host Domain", null, new AppDomainSetup {
                PrivateBinPath = Path.Combine(config.WorkingDirectory, "bin"),
                ApplicationBase = config.WorkingDirectory
            });
            return (ApplicationHost)ad.CreateInstanceAndUnwrap(typeof(ApplicationHost).Assembly.FullName, typeof(ApplicationHost).FullName);
        }

        public void OnStop() {
            host.Stop();
        }
    }
}
