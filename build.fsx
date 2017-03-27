#r "packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.NpmHelper
open System.IO

let buildDir = "./build"
let clientDir = "./client"
let clientAssetDir = clientDir @@ "public"
let assetBuildDir = buildDir @@ "public"

Target "Clean" (fun _ -> CleanDir buildDir)
Target "BuildApp" (fun _ ->
          !! "src/**/*.fsproj"
            |> MSBuildRelease buildDir "Build"
            |> Log "AppBuild-Output: "
)
Target "Client" (fun _ ->
        System.IO.Directory.CreateDirectory(assetBuildDir) |> ignore
        let npmFilePath =
          environVarOrDefault "NPM_FILE_PATH" defaultNpmParams.NpmFilePath
        Npm (fun p ->
                {
                  p with
                    Command = Install Standard
                    WorkingDirectory = clientDir
                    NpmFilePath = npmFilePath
                  })
        Npm (fun p ->
                {
                  p with
                    Command = (Run "build")
                    WorkingDirectory = clientDir
                    NpmFilePath = npmFilePath
                  })
        CopyRecursive clientAssetDir assetBuildDir true |> ignore
)

"Clean"
  ==> "BuildApp"
  ==> "Client"
RunTargetOrDefault "Client"
