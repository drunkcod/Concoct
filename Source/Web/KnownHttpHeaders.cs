using System;
using System.Collections.Generic;
using System.Web;

namespace Concoct.Web
{
    public struct KnownHttpHeader
    {
        public readonly int Index;
        public readonly string Header;

        public KnownHttpHeader(int index, string header) {
            Index  = index;
            Header = header;
        }
    }

    public class KnownHttpHeaders : IEnumerable<KnownHttpHeader>
    {
        struct KnownHttpHeaderIndexComparer : IComparer<KnownHttpHeader>
        {
            public int Compare(KnownHttpHeader x, KnownHttpHeader y) {
                return x.Index - y.Index;
            }
        }


        readonly List<KnownHttpHeader> headers;
        readonly IComparer<KnownHttpHeader> comparer = new KnownHttpHeaderIndexComparer();
        readonly bool isReadonly;

        public static KnownHttpHeaders FxHeaders = new KnownHttpHeaders {
                {HttpWorkerRequest.HeaderAccept,"Accept" },
                {HttpWorkerRequest.HeaderAcceptCharset,"Accept-Charset" },
                {HttpWorkerRequest.HeaderAcceptEncoding,"Accept-Encoding" },
                {HttpWorkerRequest.HeaderAcceptLanguage,"Accept-Language" },
                {HttpWorkerRequest.HeaderAcceptRanges,"Accept-Ranges" },
                {HttpWorkerRequest.HeaderAge,"Age" },
                {HttpWorkerRequest.HeaderAllow 	,"Allow" },
                {HttpWorkerRequest.HeaderAuthorization,"Authorization" },
                {HttpWorkerRequest.HeaderCacheControl,"Cache-Control" },
                {HttpWorkerRequest.HeaderConnection,"Connection" },
                {HttpWorkerRequest.HeaderContentEncoding,"Content-Encoding" },
                {HttpWorkerRequest.HeaderContentLanguage,"Content-Language" },
                {HttpWorkerRequest.HeaderContentLength,"Content-Length" },
                {HttpWorkerRequest.HeaderContentLocation,"Content-Location" },
                {HttpWorkerRequest.HeaderContentMd5,"Content-MD5" },
                {HttpWorkerRequest.HeaderContentRange 	,"Content-Range" },
                {HttpWorkerRequest.HeaderContentType ,"Content-Type" },
                {HttpWorkerRequest.HeaderCookie,"Cookie" },
                {HttpWorkerRequest.HeaderDate ,"Date" },
                {HttpWorkerRequest.HeaderEtag,"ETag" },
                {HttpWorkerRequest.HeaderExpect,"Except" },
                {HttpWorkerRequest.HeaderExpires,"Expires" },
                {HttpWorkerRequest.HeaderFrom ,"From" },
                {HttpWorkerRequest.HeaderHost,"Host" },
                {HttpWorkerRequest.HeaderIfMatch,"If-Match" },
                {HttpWorkerRequest.HeaderIfModifiedSince,"If-Modified-Since" },
                {HttpWorkerRequest.HeaderIfNoneMatch,"If-None-Match" },
                {HttpWorkerRequest.HeaderIfRange ,"If-Range" },
                {HttpWorkerRequest.HeaderIfUnmodifiedSince ,"If-Unmodified-Since" },
                {HttpWorkerRequest.HeaderKeepAlive ,"Keep-Alive" },
                {HttpWorkerRequest.HeaderLastModified,"Last-Modified" },
                {HttpWorkerRequest.HeaderLocation,"Location" },
                {HttpWorkerRequest.HeaderMaxForwards ,"Max-Forwards" },
                {HttpWorkerRequest.HeaderPragma ,"Pragma" },
                {HttpWorkerRequest.HeaderProxyAuthenticate ,"Proxy-Authenticate" },
                {HttpWorkerRequest.HeaderProxyAuthorization ,"Proxy-Authorization" },
                {HttpWorkerRequest.HeaderRange ,"Range" },
                {HttpWorkerRequest.HeaderReferer,"Referer" },
                {HttpWorkerRequest.HeaderRetryAfter ,"Retry-After" },
                {HttpWorkerRequest.HeaderServer ,"Server" },
                {HttpWorkerRequest.HeaderSetCookie ,"Set-Cookie" },
                {HttpWorkerRequest.HeaderTe ,"TE" },
                {HttpWorkerRequest.HeaderTrailer ,"Trailer" },
                {HttpWorkerRequest.HeaderTransferEncoding ,"Transfer-Encoding" },
                {HttpWorkerRequest.HeaderUpgrade ,"Upgrade" },
                {HttpWorkerRequest.HeaderUserAgent ,"User-Agent" },
                {HttpWorkerRequest.HeaderVary,"Vary" },
                {HttpWorkerRequest.HeaderVia ,"Via" },
                {HttpWorkerRequest.HeaderWarning ,"Warning" },
                {HttpWorkerRequest.HeaderWwwAuthenticate ,"WWW-Authenticate" },
            }.AsReadonly();

        KnownHttpHeaders(List<KnownHttpHeader> headers, bool isReadonly) { 
            this.headers = headers; 
            this.isReadonly = isReadonly;
        }

        public KnownHttpHeaders() : this(new List<KnownHttpHeader>(), false) { }

        public void Add(int index, string header) {
            if(isReadonly)
                throw new InvalidOperationException("I'm readonly.");
            headers.Add(new KnownHttpHeader(index, header));
            headers.Sort(comparer);
        }

        public string this[int index] {
            get {
                return headers[headers.BinarySearch(new KnownHttpHeader(index, string.Empty), comparer)].Header;
            }
        }

        KnownHttpHeaders AsReadonly() { return new KnownHttpHeaders(headers, true); }

        public IEnumerator<KnownHttpHeader> GetEnumerator() {
            return headers.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return headers.GetEnumerator();
        }
    }
}
