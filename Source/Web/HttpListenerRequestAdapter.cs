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
        readonly string applicationPath;
        readonly Func<Uri,string> makeAppRelativePath;
        readonly NameValueCollection serverVariables = new NameValueCollection();
        readonly FormDataParser formParser = new FormDataParser();


        public HttpListenerRequestAdapter(HttpListenerRequest request, string applicationPath, Func<Uri, string> makeAppRelativePath) {
            this.request = request;
            this.applicationPath = applicationPath;
            this.makeAppRelativePath = makeAppRelativePath;
        }

        public override string[] AcceptTypes { get { return request.AcceptTypes; } }
        public override string ApplicationPath { get { return applicationPath; } }

        public override string AppRelativeCurrentExecutionFilePath {
            get { return makeAppRelativePath(request.Url); }
        }
        public override Encoding ContentEncoding {
            get { return request.ContentEncoding; }
            set { throw new NotSupportedException(); }
        }
        public override string PathInfo { get { return string.Empty; } }
        public override string RawUrl { get { return request.Url.AbsolutePath; } }
        public string HttpVersion { get { return request.ProtocolVersion.ToString(); } }
        public override string HttpMethod { get { return request.HttpMethod; } }
        public override string ContentType {
            get { return request.ContentType; }
            set { throw new NotSupportedException(); }
        }
        public override System.IO.Stream InputStream { get { return request.InputStream; } }

        public override NameValueCollection Form {
            get {
                ParseFormAndFiles();
                return formParser.Fields;
            }
        }

        public override Uri Url { get {return request.Url; } }
        public override NameValueCollection QueryString { get { return request.QueryString; } }
        public override HttpFileCollectionBase Files { 
            get {
                ParseFormAndFiles();
                return formParser.Files; 
            } 
        }
        public override NameValueCollection Headers { get { return request.Headers; } }
        public override NameValueCollection ServerVariables { get { return serverVariables; } }
        public override void ValidateInput() { }

        public void ParseFormAndFiles() {
            if(formParser.HasResult) return;
            formParser.ParseFormAndFiles(request);
        }
    }
}