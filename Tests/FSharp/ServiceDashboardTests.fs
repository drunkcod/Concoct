module Concoct.Samples.ServiceDashboardTests
open System.Net
open System.Web
open System.Web.Mvc
open System.IO
open Concoct
open Xlnt.Web.Mvc
open NUnit.Framework

type BeforeAll = NUnit.Framework.TestFixtureSetUpAttribute
type AfterAll = NUnit.Framework.TestFixtureTearDownAttribute

let [<Literal>] BaseUrl = "http://localhost/Samples.ServiceDashboard/"
let ReadAllText (response:HttpWebResponse) =
    use reader = new StreamReader(response.GetResponseStream())
    reader.ReadToEnd()

let mutable sample = null :> MvcHost

let [<BeforeAll>] Start_ServiceDashboard_host() =
    sample <- MvcHost.Create(IPEndPoint(IPAddress.Any, 80), "Samples.ServiceDashboard", typeof<Concoct.Samples.ServiceDashboard.MvcApplication>)
    sample.Starting.Add(fun _ -> 
        let controllerFactory = BasicControllerFactory()
        controllerFactory.Register(typeof<Concoct.Samples.ServiceDashboard.MvcApplication>.Assembly.GetTypes())
        ControllerBuilder.Current.SetControllerFactory(controllerFactory))
    sample.Start()

let [<AfterAll>] Stop_ServiceDashboard_host() =
    sample.Stop()

let [<Test>] should_display_greeting_as_index_action() =
    let result = ReadAllText |> Http.get BaseUrl 
    Assert.That(result, Is.EqualTo("Hello MVC World!"), result)