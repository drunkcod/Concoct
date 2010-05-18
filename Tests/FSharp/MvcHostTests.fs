module Concoct.MvcHostTests
open System.Net
open System.Web
open NUnit.Framework

type SimpleApplication() =
    inherit HttpApplication()

    [<DefaultValue>] static val mutable private started : int

    static member StartCount = SimpleApplication.started

    member this.Application_Start() = 
        SimpleApplication.started <- SimpleApplication.started + 1

let NewSimpleApplicationHost() = MvcHost.Create(IPEndPoint(IPAddress.Any, 80), "Concoct.Tests", typeof<SimpleApplication>)

let [<Test>] is_disposable() = 
    use host = NewSimpleApplicationHost()
    ()

let [<Test>] should_call_public_Application_Start_on_startup() = 
    use host = NewSimpleApplicationHost()
    let before = SimpleApplication.StartCount
    host.Start()
    Assert.That(SimpleApplication.StartCount, Is.EqualTo(before + 1))