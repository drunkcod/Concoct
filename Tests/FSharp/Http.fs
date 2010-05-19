module Concoct.Http
open System.Net

    let get (url:string) responseHandler = 
        let request = WebRequest.Create(url) :?> HttpWebRequest
        try
            use response = request.GetResponse() :?> HttpWebResponse
            responseHandler(response)
        with :? WebException as e -> responseHandler(e.Response :?> HttpWebResponse)