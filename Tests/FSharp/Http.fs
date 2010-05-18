module Concoct.Http
open System.Net

    let get (url:string) responseHandler = 
        let request = WebRequest.Create(url) :?> HttpWebRequest
        responseHandler(request.GetResponse() :?> HttpWebResponse)