module Concoct.Samples.ServiceDashboardTests
open System.Net
open System.Web
open System.Web.Mvc
open System.IO
open Concoct
open Xlnt.Web.Mvc
open NUnit.Framework

let [<Literal>] BaseUrl = "http://localhost/Samples.ServiceDashboard/"
let ReadAllText (response:HttpWebResponse) =
    use reader = new StreamReader(response.GetResponseStream())
    reader.ReadToEnd()

let [<Test>] should_display_greeting_as_index_action() =
    use sample = MvcHost.Create(IPEndPoint(IPAddress.Any, 80), "Samples.ServiceDashboard", typeof<Concoct.Samples.ServiceDashboard.MvcApplication>)
    sample.Starting.Add(fun _ -> 
        let controllerFactory = BasicControllerFactory()
        controllerFactory.Register(typeof<Concoct.Samples.ServiceDashboard.MvcApplication>.Assembly.GetTypes())
        ControllerBuilder.Current.SetControllerFactory(controllerFactory))
    sample.Start()
    let result = Http.get BaseUrl ReadAllText
    Assert.That(result, Is.EqualTo("Hello MVC World!"))