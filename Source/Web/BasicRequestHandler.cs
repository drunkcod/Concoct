﻿using System.Net;
using System.Web;
using Concoct.Web;

namespace Concoct.Web
{
    public class BasicRequestHandler : IHttpListenerRequestHandler
    {
        readonly string virtualPath;
        readonly string physicalPath;
        readonly IHttpHandler handler;

        public BasicRequestHandler(string virtualPath, string physicalPath, IHttpHandler handler) {
            this.virtualPath = virtualPath;
            this.physicalPath = physicalPath;
            this.handler = handler;
        }

        public void Process(HttpListenerContext context) {
            var adapter = new HttpListenerContextAdapter(context, virtualPath, physicalPath);
            adapter.AsHttpContext(handler.ProcessRequest, false);
            adapter.Response.End();
        }
    }
}
