﻿using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Concoct.IO;

namespace Concoct.Web
{
    class RequestStream : IRequestStream
    {
        public RequestStream(string contentType, int length, Stream inputStream) {
			ContentType = contentType;
			ContentLength = length;
			InputStream = inputStream;
        }

        public string ContentType { get; private set; }

        public int ContentLength { get; private set; }

        public Stream InputStream { get; private set; }
    }

	public class FormDataParser
    {
        public const string ContentTypeFormUrlEncoded = "application/x-www-form-urlencoded";
        public const string ContentTypeMultipartFormData = "multipart/form-data";
        static readonly Regex FilenamePattern = new Regex("filename=\"(?<filename>.+?)\"", RegexOptions.Compiled);
        static readonly Regex NamePattern = new Regex("name=\"(?<name>.+?)\"", RegexOptions.Compiled);

        NameValueCollection fields;
        BasicHttpFileCollection files;

        public bool HasResult { get { return fields != null; } }
        public NameValueCollection Fields { get { return fields; } }
        public HttpFileCollectionBase Files { get { return files; } }

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
                    fields.Add(name, e.Part.GetBodyText(Encoding.UTF8));
            };
            multiPartStream.Read(request.InputStream, request.ContentLength);
        }

        void WithBodyBytes(IRequestStream request, Action<byte[], int> action) {
            var bytes = new byte[request.ContentLength];
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
