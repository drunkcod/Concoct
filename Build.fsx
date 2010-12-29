#r "Tools\DotNetZip\Ionic.Zip.Reduced.dll"
open System.Diagnostics
open System.IO
open System.Reflection
open System.Xml
open System
open Ionic.Zip

let clean =
  let safeDirectoryDelete path = 
    try Directory.Delete(path, true); true
    with 
    | :? DirectoryNotFoundException -> true
    | _ -> false 
  Seq.forall safeDirectoryDelete

let build solution args =
  let fxPath = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()  
  let msBuild4 = Path.Combine(fxPath, @"..\v4.0.30319\MSBuild.exe")
  let build =
    Process.Start(
      ProcessStartInfo(
        FileName = msBuild4,
        Arguments = solution + " /nologo " + args,
        UseShellExecute = false))
  build.WaitForExit()
  build.ExitCode = 0

let package() =
    let version = AssemblyName.GetAssemblyName("Bin\Concoct.exe").Version.ToString()
    use zip = new ZipFile()
    zip.AddDirectory("Bin", "Bin") |> ignore
    zip.Save(@"Bin\Concoct-" + version + ".zip")
    true

let r = 
    clean ["Build";"Bin"] 
    && build "Concoct.sln" "/m /p:Configuration=Release"
    && package()
if not r then
    Console.Error.WriteLine("Build failed.")