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

    public class KnownHttpHeaders
    {
		public const string ContentType = "Content-Type";

        struct KnownHttpHeaderIndexComparer : IComparer<KnownHttpHeader>
        {
            public int Compare(KnownHttpHeader x, KnownHttpHeader y) {
                return x.Index - y.Index;
            }
        }

		readonly List<KnownHttpHeader> headers;
        readonly IComparer<KnownHttpHeader> comparer = new KnownHttpHeaderIndexComparer();
        readonly bool isReadonly;

        public static KnownHttpHeaders FxHeaders = new KnownHttpHeaders(new List<KnownHttpHeader> {
                Header(HttpWorkerRequest.HeaderAccept, "Accept"),
                Header(HttpWorkerRequest.HeaderAcceptCharset, "Accept-Charset"),
                Header(HttpWorkerRequest.HeaderAcceptEncoding, "Accept-Encoding"),
                Header(HttpWorkerRequest.HeaderAcceptLanguage, "Accept-Language"),
                Header(HttpWorkerRequest.HeaderAcceptRanges, "Accept-Ranges"),
                Header(HttpWorkerRequest.HeaderAge, "Age"),
                Header(HttpWorkerRequest.HeaderAllow, "Allow"),
                Header(HttpWorkerRequest.HeaderAuthorization, "Authorization"),
                Header(HttpWorkerRequest.HeaderCacheControl, "Cache-Control"),
                Header(HttpWorkerRequest.HeaderConnection, "Connection"),
                Header(HttpWorkerRequest.HeaderContentEncoding, "Content-Encoding"),
                Header(HttpWorkerRequest.HeaderContentLanguage, "Content-Language"),
                Header(HttpWorkerRequest.HeaderContentLength, "Content-Length"),
                Header(HttpWorkerRequest.HeaderContentLocation, "Content-Location"),
                Header(HttpWorkerRequest.HeaderContentMd5, "Content-MD5"),
                Header(HttpWorkerRequest.HeaderContentRange, "Content-Range"),
                Header(HttpWorkerRequest.HeaderContentType, ContentType),
                Header(HttpWorkerRequest.HeaderCookie, "Cookie"),
                Header(HttpWorkerRequest.HeaderDate, "Date"),
                Header(HttpWorkerRequest.HeaderEtag, "ETag"),
                Header(HttpWorkerRequest.HeaderExpect, "Except"),
                Header(HttpWorkerRequest.HeaderExpires, "Expires"),
                Header(HttpWorkerRequest.HeaderFrom, "From"),
                Header(HttpWorkerRequest.HeaderHost,"Host"),
                Header(HttpWorkerRequest.HeaderIfMatch,"If-Match"),
                Header(HttpWorkerRequest.HeaderIfModifiedSince,"If-Modified-Since"),
                Header(HttpWorkerRequest.HeaderIfNoneMatch,"If-None-Match"),
                Header(HttpWorkerRequest.HeaderIfRange ,"If-Range"),
                Header(HttpWorkerRequest.HeaderIfUnmodifiedSince ,"If-Unmodified-Since"),
                Header(HttpWorkerRequest.HeaderKeepAlive ,"Keep-Alive"),
                Header(HttpWorkerRequest.HeaderLastModified,"Last-Modified"),
                Header(HttpWorkerRequest.HeaderLocation,"Location"),
                Header(HttpWorkerRequest.HeaderMaxForwards ,"Max-Forwards"),
                Header(HttpWorkerRequest.HeaderPragma ,"Pragma"),
                Header(HttpWorkerRequest.HeaderProxyAuthenticate ,"Proxy-Authenticate"),
                Header(HttpWorkerRequest.HeaderProxyAuthorization ,"Proxy-Authorization"),
                Header(HttpWorkerRequest.HeaderRange ,"Range"),
                Header(HttpWorkerRequest.HeaderReferer,"Referer"),
                Header(HttpWorkerRequest.HeaderRetryAfter,"Retry-After"),
                Header(HttpWorkerRequest.HeaderServer, "Server"),
                Header(HttpWorkerRequest.HeaderSetCookie, "Set-Cookie"),
                Header(HttpWorkerRequest.HeaderTe, "TE"),
                Header(HttpWorkerRequest.HeaderTrailer, "Trailer"),
                Header(HttpWorkerRequest.HeaderTransferEncoding, "Transfer-Encoding"),
                Header(HttpWorkerRequest.HeaderUpgrade, "Upgrade"),
                Header(HttpWorkerRequest.HeaderUserAgent, "User-Agent"),
                Header(HttpWorkerRequest.HeaderVary, "Vary"),
                Header(HttpWorkerRequest.HeaderVia, "Via"),
                Header(HttpWorkerRequest.HeaderWarning, "Warning"),
                Header(HttpWorkerRequest.HeaderWwwAuthenticate, "WWW-Authenticate"),
            }, true);

		static KnownHttpHeader Header(int index, string name) {
			return new KnownHttpHeader(index, name);
		}

        KnownHttpHeaders(List<KnownHttpHeader> headers, bool isReadonly) { 
            this.headers = headers; 
			this.headers.Sort(comparer);
            this.isReadonly = isReadonly;
        }

        public string this[int index] {
            get {
                return headers[headers.BinarySearch(new KnownHttpHeader(index, string.Empty), comparer)].Header;
            }
        }
    }
}
