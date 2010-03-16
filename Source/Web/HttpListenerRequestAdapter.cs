using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web;

namespace Concoct.Web
{
    class HttpListenerRequestAdapter : HttpRequestBase
    {
        readonly HttpListenerRequest request;
        private NameValueCollection form;

        public HttpListenerRequestAdapter(HttpListenerRequest request) {
            this.request = request;
        }

        public override string AppRelativeCurrentExecutionFilePath { get { return "~" + RawUrl; } }
        public override Encoding ContentEncoding {
            get { return request.ContentEncoding; }
            set { throw new NotSupportedException(); }
        }
        public override string PathInfo { get { return string.Empty; } }
        public override string RawUrl { get { return request.Url.AbsolutePath; } }
        public string HttpVersion { get { return request.ProtocolVersion.ToString(); } }
        public override string HttpMethod { get { return request.HttpMethod; } }
        public override NameValueCollection Form { 
            get {
                if(form != null)
                    return form;
                form = new NameValueCollection();
                if (request.ContentType != "application/x-www-form-urlencoded")
                    return form;
                var bytes = new byte[request.ContentLength64];
                var bytesRead = request.InputStream.Read(bytes, 0, bytes.Length);
                var data = HttpUtility.UrlDecode(bytes, 0, bytesRead, Encoding.UTF8);
                foreach(var item in data.Split(new []{ '&' }, StringSplitOptions.RemoveEmptyEntries)){
                    var parts = item.Split('=');
                    form.Add(parts[0], parts[1]);
                }
                return form;
            }
        }
        public override NameValueCollection QueryString { get { return request.QueryString; } }
        public override HttpFileCollectionBase Files { get { return new EmptyHttpFileCollection(); } }
        public override NameValueCollection Headers { get { return request.Headers; } }
        public override void ValidateInput() { }        
    }
}