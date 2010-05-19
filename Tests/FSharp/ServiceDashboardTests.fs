module Concoct.Samples.ServiceDashboardTests
open System.Collections.Generic
open System.Net
open System.Web
open System.Web.Mvc
open System.IO
open System.Xml.XPath
open Concoct
open Concoct.Samples.ServiceDashboard
open Xlnt.Web.Mvc
open NUnit.Framework

type BeforeAllAttribute = NUnit.Framework.TestFixtureSetUpAttribute
type AfterAllAttribute = NUnit.Framework.TestFixtureTearDownAttribute
type ItAttribute = NUnit.Framework.TestAttribute

let [<Literal>] BaseUrl = "http://localhost/Samples.ServiceDashboard/"

let ReadAllText (response:HttpWebResponse) =
    use reader = new StreamReader(response.GetResponseStream())
    reader.ReadToEnd()

let mutable Sample = null :> MvcHost

let [<BeforeAll>] Start_ServiceDashboard_host() =
    Sample <- MvcHost.Create(IPEndPoint(IPAddress.Any, 80), "Samples.ServiceDashboard", typeof<MvcApplication>)
    Sample.Starting.Add(fun _ -> 
        let controllerFactory = BasicControllerFactory()
        controllerFactory.Register(typeof<Concoct.Samples.ServiceDashboard.MvcApplication>.Assembly.GetTypes())
        ControllerBuilder.Current.SetControllerFactory(controllerFactory))
    Sample.Start()

let [<AfterAll>] Stop_ServiceDashboard_host() =
    Sample.Stop()

let [<It>] display_greeting_as_index_action() =
    let result = ReadAllText |> Http.get BaseUrl 
    Assert.That(result, Is.EqualTo("Hello MVC World!"), result)

let [<It>] Should_expose_a_list_of_services() =
    let result = Http.get (BaseUrl + "Services") (fun x -> new XPathDocument(x.GetResponseStream()))
    let count = result.CreateNavigator().Select("/services/service").Count
    let app = Sample.Application :?> MvcApplication
    Assert.That(count, Is.EqualTo(app.Services.Count))

let [<It>] Should_have_self_links_for_each_service_containt_its_id() =
    let result = Http.get (BaseUrl + "Services") (fun x -> new XPathDocument(x.GetResponseStream()))

    let links = 
        result.CreateNavigator().Select("/services/service")
        |> Seq.cast<XPathNavigator>
        |> Seq.map (fun x -> (x.GetAttribute("id", ""), x.SelectSingleNode("link[@rel='self']/@href").Value))

    let app = Sample.Application :?> MvcApplication
    let services = Set(app.Services |> Seq.map (fun x -> x.Id))

    Assert.That(Seq.length links, Is.EqualTo(app.Services.Count), "Service(s) missing")
    links |> Seq.iter (fun (id, link) ->
        Assert.That(services.Contains(id), "Missing link to \"" + id + "\"")
        Assert.That(link, Is.StringContaining(id)))