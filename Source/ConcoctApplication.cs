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

        public void Start(ConcoctConfiguration config, ILog log)  {
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

                if(config.LogRequests)
                    host.RequestHandler.BeginRequest += (_, e) => log.Info("{0} {1}", e.Request.HttpMethod, e.Request.Url);

                host.Start();
			} catch(FileNotFoundException appNotFound) {
				log.Error("Failed to locate {0}", appNotFound.FileName);
				throw new ApplicationException("Failed to load application");
            } catch(ReflectionTypeLoadException loadError) {
                log.Error("Error applications.");
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
		readonly ILog log;
        ApplicationHost host;

        public ConcoctApplication(ConcoctConfiguration config, ILog log) {
            this.config = config;
			this.log = log;
        }

        public void Start() {
            host = CreateHost();
            host.Start(config, log);
        }

        ApplicationHost CreateHost() {
            var ad = AppDomain.CreateDomain("Host Domain", null, new AppDomainSetup {
                ApplicationBase = config.WorkingDirectory,
                PrivateBinPath = config.PrivateBinPath,
				ConfigurationFile = config.ConfigurationFile
            });
            return (ApplicationHost)ad.CreateInstanceAndUnwrap(typeof(ApplicationHost).Assembly.FullName, typeof(ApplicationHost).FullName);
        }

        public void Stop() {
            host.Stop();
        }
    }
}
