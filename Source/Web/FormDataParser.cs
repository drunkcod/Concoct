using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;

namespace Concoct.Web
{
    public interface IRequestStream 
    {
        string ContentType { get; }
        long ContentLength64 { get; }
        Stream InputStream { get; }
    }

    public class FormDataParser
    {
        public const string ContentTypeFormUrlEncoded = "application/x-www-form-urlencoded";
        public const string ContentTypeMultipartFormData = "multipart/form-data";

        class HttpListenerRequestStreamAdapter : IRequestStream
        {
            readonly HttpListenerRequest request;

            public HttpListenerRequestStreamAdapter(HttpListenerRequest request) {
                this.request = request;
            }

            public string ContentType { get { return request.ContentType; } }

            public long ContentLength64 { get { return request.ContentLength64; } }

            public Stream InputStream { get { return request.InputStream; } }
        }

        NameValueCollection fields;
        BasicHttpFileCollection files;

        public bool HasResult { get { return fields != null; } }
        public NameValueCollection Fields { get { return fields; } }
        public HttpFileCollectionBase Files { get { return files; } }

        public void ParseFormAndFiles(HttpListenerRequest request) {
            ParseFormAndFiles(new HttpListenerRequestStreamAdapter(request));
        }

        public bool ParseFormAndFiles(IRequestStream request) {
            if(HasResult) return false;

            fields = new NameValueCollection();
            files = new BasicHttpFileCollection();
            if(request.ContentType.StartsWith(ContentTypeFormUrlEncoded))
                ParseFormUrlEncoded(request);
            else if(request.ContentType.StartsWith(ContentTypeMultipartFormData))
                ParseMultiPart(request);
            else 
                return false;

            return true;
        }

        void ParseFormUrlEncoded(IRequestStream request) {
            var bytes = new byte[request.ContentLength64];
            var bytesRead = request.InputStream.Read(bytes, 0, bytes.Length);
            var data = HttpUtility.UrlDecode(bytes, 0, bytesRead, Encoding.UTF8);
            foreach(var item in data.Split(new []{ '&' }, StringSplitOptions.RemoveEmptyEntries)){
                var parts = item.Split('=');
                fields.Add(parts[0], parts[1]);
            }
        }

        void ParseMultiPart(IRequestStream request) {
            var multiPartStream = new MultiPartStream(GetBoundary(request.ContentType));
            var filenamePattern = new Regex("filename=\"(?<filename>.+?)\"");
            multiPartStream.PartReady += (sender, e) => {
                var hasFileName = filenamePattern.Match(e.Part.Headers["Content-Disposition"]);
                if(hasFileName.Success)
                    files.Add(new BasicHttpPostedFile(
                        hasFileName.Groups["filename"].Value, 
                        e.Part.Headers["Content-Type"],
                        e.Part.Body));
                else
                    fields.Add(Fields.Count.ToString(), Encoding.UTF8.GetString(e.Part.Body));
            };
            multiPartStream.Read(request.InputStream);
        }

        string GetBoundary(string contentType) {
            var parts = contentType.Split(';');
            for(var i = 1; i != parts.Length; ++i) {
                var x = parts[i].TrimStart();
                if(x.StartsWith("boundary=")) 
                    return x.Substring("boundary=".Length);
            }
            throw new InvalidOperationException(string.Format("no boundary found in [{0}]", contentType));
        }
    }
}
