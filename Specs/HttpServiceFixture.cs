using System;
using System.Net;

namespace Concoct
{
	public abstract class HttpServiceFixture 
	{		
		public void WithResponseFrom(string url, Action<WebResponse> withResponse) {
			var host = CreateService();
			try {
                host.Start();
                var request = WebRequest.Create(url);
                using(var response = request.GetResponse())
                    withResponse(response);
            } finally {
                host.Stop();
            }
		}

		protected abstract IServiceController CreateService();
	}
}
