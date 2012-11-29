using System;
using System.Collections.Specialized;
using System.IO;
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
        readonly FormDataParser formParser = new FormDataParser();
		readonly NameValueCollection serverVariables = new NameValueCollection();
		Stream inputStream;

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
		
		public override int ContentLength { get { return (int)request.ContentLength64; } }

		public override string ContentType {
            get { return request.ContentType ?? string.Empty; }
            set { throw new NotSupportedException(); }
        }

		public override string PathInfo { get { return string.Empty; } }
        public override string RawUrl { get { return request.Url.AbsolutePath; } }
        public string HttpVersion { get { return request.ProtocolVersion.ToString(); } }
        public override string HttpMethod { get { return request.HttpMethod; } }

        public override System.IO.Stream InputStream { 
			get { 
				if(inputStream == null) {
					inputStream = new MemoryStream();
					request.InputStream.CopyTo(inputStream);
					RewindInputStream();
				}
				return inputStream; 
			} 
		}

        public override NameValueCollection Form { get { return ParseFormAndFiles().Fields; } }
        public override Uri Url { get {return request.Url; } }
        public override NameValueCollection QueryString { get { return request.QueryString; } }
        public override HttpFileCollectionBase Files { get { return ParseFormAndFiles().Files; } }
        public override NameValueCollection Headers { get { return request.Headers; } }
        public override NameValueCollection ServerVariables { get { return serverVariables; } }
        public override void ValidateInput() { }

		public override string this[string key] {
			get { throw new NotSupportedException(); }
		}

        public FormDataParser ParseFormAndFiles() {
            if(!formParser.HasResult) {
				formParser.ParseFormAndFiles(new RequestStream(ContentType, ContentLength, InputStream));
				RewindInputStream();
			}
			return formParser;
			
        }

		private void RewindInputStream() {
			InputStream.Seek(0, SeekOrigin.Begin);
		}
    }
}