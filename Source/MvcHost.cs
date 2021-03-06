﻿using System;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;

namespace Concoct
{
    public class MvcHost : IDisposable, IServiceController
    {
        const string ProxiesAssemblyName = "Concoct.Proxies";

        class NullApplication : IConcoctApplication
        {
            public void Start() { }
        }

        readonly HttpListenerAcceptor acceptor;
        readonly IConcoctApplication application;

        public static MvcHost Create(IPEndPoint bindTo, string virtualPath, string physicalPath) {
            return Create(bindTo, virtualPath, physicalPath, new NullApplication());
        }

        public static MvcHost Create(IPEndPoint bindTo, string virtualPath, string physicalPath, Type applicationType) {
            return Create(bindTo, virtualPath, physicalPath, CreateApplicationProxy(applicationType));
        }

        static MvcHost Create(IPEndPoint bindTo, string virtualPath, string physicalPath, IConcoctApplication application) {
            var uri = new Uri(virtualPath, UriKind.RelativeOrAbsolute);
            return new MvcHost(new HttpListenerAcceptor(
                    bindTo,
                    uri,
                    new MvcRequestHandler(uri.IsAbsoluteUri ? uri.AbsolutePath : virtualPath, physicalPath)),
                application);
        }

        MvcHost(HttpListenerAcceptor acceptor, IConcoctApplication application) {
            this.acceptor = acceptor;
            this.application = application;
        }

        public object Application { get { return application; } }
        
        public MvcRequestHandler RequestHandler { get { return (MvcRequestHandler)acceptor.RequestHandler; } }

        public event EventHandler<EventArgs> Starting;

        public void Start() {
            application.Start();
            OnStarting();
            acceptor.Start();
        }

        public void Stop() {
            acceptor.Stop();
        }

        void OnStarting() {
            var handler = Starting;
            if(handler != null)
                handler(this, EventArgs.Empty);
        }

        void IDisposable.Dispose() { Stop(); }

        static IConcoctApplication CreateApplicationProxy(Type httpApplicationType) {
            var proxies = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(ProxiesAssemblyName), AssemblyBuilderAccess.Run);
            var module = proxies.DefineDynamicModule("Main");
            var proxy = ApplicationBuilder.CreateIn(module, httpApplicationType);
            proxy.DynamicEventWireUp("Application_Start", x => x.Start());
            return proxy.CreateType();
        }
    }
}
