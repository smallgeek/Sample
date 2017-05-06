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

  let downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Download")
  let fileMovePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "FileMove")

  let downloadDirectory = new ReactiveProperty<string>(downloadPath)
  let cameraDirectory = new ReactiveProperty<string>(@"DCIM\101_FUJI")
  let fileMoveDirectory = new ReactiveProperty<string>(fileMovePath)

  let isEnabled = new ReactiveProperty<bool>()

  let downloader = new FlashAirDownloader("flashair_sg", Local = downloadDirectory.Value, Remote = cameraDirectory.Value)
  let watcher = new FileSystemWatcher(fileMoveDirectory.Value)

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

  let detected, undetected =
    remoteSource 
    |> Observable.merge localSource
    |> Observable.flatmapAsync recognize
    |> Observable.publishPartition (fun results -> not results.IsDetected)

  let detectedText = 
    detected
    |> Observable.map (fun r -> r.Text)
    |> Observable.toReadOnlyProp

  let detectedImage = 
    detected
    |> Observable.map (fun r -> r.Path)
    |> Observable.toReadOnlyProp

  let undetectedText =
    undetected 
    |> Observable.map (fun r -> r.Text)
    |> Observable.toReadOnlyProp

  let undetectedImage =
    undetected
    |> Observable.map (fun r -> r.Path)
    |> Observable.toReadOnlyProp

  member val DownloadDirectory = downloadDirectory
  member val CameraDirectory = cameraDirectory
  member val FileMoveDirectory = fileMoveDirectory

  member val DetectedText = detectedText
  member val UndetectedText = undetectedText
  member val DetectedImage = detectedImage
  member val UndetectedImage = undetectedImage

  member val IsEnabled = isEnabled
