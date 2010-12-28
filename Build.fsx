#r "Tools\DotNetZip\Ionic.Zip.Reduced.dll"
open System.Diagnostics
open System.IO
open System.Reflection
open System.Xml
open Ionic.Zip

let clean what =
    what |> Seq.map (fun p -> try Directory.Delete(p, true); true with | :? DirectoryNotFoundException -> true | _ -> false) |> Seq.reduce (&&)

let SolutionPath = "Concoct.sln"
let MainProject = "Source\Concoct.csproj"

let build args =
  let fxPath = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()  
  let msBuild4 = Path.Combine(fxPath, @"..\v4.0.30319\MSBuild.exe")
  let build =
    Process.Start(
      ProcessStartInfo(
        FileName = msBuild4,
        Arguments = SolutionPath + " /nologo /m /p:Configuration=Release " + args,
        UseShellExecute = false))
  build.WaitForExit()
  build.ExitCode = 0
let package() =
    let version = AssemblyName.GetAssemblyName("Bin\Concoct.exe").Version.ToString()

    use zip = new ZipFile()

    zip.AddDirectory("Bin", "Bin") |> ignore
    zip.Save(@"Bin\Concoct-" + version + ".zip")
    true

clean ["Build";"Bin"]
&& build ""
&& package()