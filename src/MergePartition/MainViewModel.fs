namespace ViewModels

open System
open System.IO
open System.Windows
open System.Windows.Input
open Reactive.Bindings
open FSharp.Control.Reactive
open FlashAir
open Vision

open FsXaml

type MainViewModel() =

  let downloader = new FlashAirDownloader("flashair_sg")
  let watcher = new FileSystemWatcher()

  let downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Download")
  let fileMovePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "FileMove")

  let downloadDirectory = new ReactiveProperty<string>(downloadPath)
  let cameraDirectory = new ReactiveProperty<string>(@"DCIM\101_FUJI")
  let fileMoveDirectory = new ReactiveProperty<string>(fileMovePath)

  let isEnabled = new ReactiveProperty<bool>()

  do
    downloadDirectory
    |> Observable.filter (fun path -> Directory.Exists(path))
    |> Observable.subscribe (fun path -> downloader.Local <- path)
    |> ignore

    cameraDirectory
    |> Observable.subscribe (fun path -> downloader.Remote <- path)
    |> ignore

    fileMoveDirectory
    |> Observable.filter (fun path -> Directory.Exists(path))
    |> Observable.subscribe (fun path -> watcher.Path <- path)
    |> ignore

    isEnabled
    |> Observable.subscribe (fun v -> downloader.IsEnabled <- v; watcher.EnableRaisingEvents <- v)
    |> ignore

  let remoteSource = downloader.Downloaded |> Observable.map (fun e -> Uri e)
  let localSource = watcher.Created |> Observable.map (fun e -> Uri e.FullPath)

  let detect, undetect =
    Observable.merge remoteSource localSource
    |> Observable.flatmapAsync recognize
    |> Observable.flatmapSeq toWords
    |> Observable.partitionHot (fun (value, uri) -> value <> "")

  let detectedText = 
    detect 
    |> Observable.map fst
    |> Observable.toReadOnlyProp

  let detectedImage = 
    detect 
    |> Observable.map snd
    |> Observable.toReadOnlyProp

  let undetectedText =
    undetect 
    |> Observable.map fst
    |> Observable.toReadOnlyProp

  let undetectedImage =
    undetect 
    |> Observable.map snd
    |> Observable.toReadOnlyProp

  member val DownloadDirectory = downloadDirectory
  member val CameraDirectory = cameraDirectory
  member val FileMoveDirectory = fileMoveDirectory

  member val DetectedText = detectedText
  member val UndetectedText = undetectedText
  member val DetectedImage = detectedImage
  member val UndetectedImage = undetectedImage

  member val IsEnabled = isEnabled
