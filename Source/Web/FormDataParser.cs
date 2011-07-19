using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Concoct.IO;

namespace Concoct.Web
{
    public class FormDataParser
    {
        const int BufferSize = 1 << 20;
        public const string ContentTypeFormUrlEncoded = "application/x-www-form-urlencoded";
        public const string ContentTypeMultipartFormData = "multipart/form-data";
        static readonly Regex FilenamePattern = new Regex("filename=\"(?<filename>.+?)\"", RegexOptions.Compiled);
        static readonly Regex NamePattern = new Regex("name=\"(?<name>.+?)\"", RegexOptions.Compiled);

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
            var conentType = request.ContentType;
            if(conentType == null)
                return false;

            if(conentType.StartsWith(ContentTypeFormUrlEncoded))
                WithBodyBytes(request, ParseFormUrlEncoded);
            else if(conentType.StartsWith(ContentTypeMultipartFormData))
                ParseMultiPart(request);
            else 
                return false;

            return true;
        }

        void ParseFormUrlEncoded(byte[] bytes, int count) {
            var data = HttpUtility.UrlDecode(bytes, 0, count, Encoding.UTF8);
            foreach(var item in data.Split(new []{ '&' }, StringSplitOptions.RemoveEmptyEntries)){
                var parts = item.Split('=');
                fields.Add(parts[0], parts[1]);
            }
        }

        void ParseMultiPart(IRequestStream request) {
            var multiPartStream = new MultiPartStream(GetBoundary(request.ContentType));
            multiPartStream.PartReady += (sender, e) => {
                var disposition = e.Part["Content-Disposition"];
                var name = NamePattern.Match(disposition).Groups["name"].Value;
                var hasFileName = FilenamePattern.Match(disposition);
                if(hasFileName.Success)
                    files.Add(name, new BasicHttpPostedFile(
                        hasFileName.Groups["filename"].Value, 
                        e.Part["Content-Type"],
                        e.Part.Body));
                else
                    fields.Add(name, Encoding.UTF8.GetString(e.Part.Body));
            };
            var buffer = new byte[BufferSize];
            for(int remaining = (int)request.ContentLength64, block; 
                remaining != 0
                && (block = request.InputStream.Read(buffer, 0, Math.Min(remaining, buffer.Length))) != 0;) 
            {
                remaining -= block;
                multiPartStream.Process(buffer, 0, block);
            }
        }

        void WithBodyBytes(IRequestStream request, Action<byte[], int> action) {
            var bytes = new byte[request.ContentLength64];
            action(bytes, request.InputStream.ReadBlock(bytes, 0, bytes.Length));
        }

        string GetBoundary(string contentType) {
            const string boundary = "boundary=";
            var parts = contentType.Split(';');
            for(var i = 1; i != parts.Length; ++i) {
                var x = parts[i].TrimStart();
                if(x.StartsWith(boundary)) 
                    return x.Substring(boundary.Length);
            }
            throw new InvalidOperationException(string.Format("no boundary found in [{0}]", contentType));
        }
    }
}
