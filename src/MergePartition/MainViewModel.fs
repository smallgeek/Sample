namespace ViewModels

open System
open System.IO
open System.Windows
open System.Windows.Input
open Reactive.Bindings
open FSharp.Control.Reactive.Observable
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
    |> filter (fun path -> Directory.Exists(path))
    |> subscribe (fun path -> downloader.Local <- path)
    |> ignore

    cameraDirectory
    |> subscribe (fun path -> downloader.Remote <- path)
    |> ignore

    fileMoveDirectory
    |> filter (fun path -> Directory.Exists(path))
    |> subscribe (fun path -> watcher.Path <- path)
    |> ignore

    isEnabled
    |> subscribe (fun v -> downloader.IsEnabled <- v; watcher.EnableRaisingEvents <- v)
    |> ignore

  let remoteSource = downloader.Downloaded |> map (fun e -> Uri e)
  let localSource = watcher.Created |> map (fun e -> Uri e.FullPath)

  let detected, undetected =
    remoteSource 
    |> merge localSource
    |> flatmapAsync recognize
    |> publishPartition (fun results -> results.IsDetected)

  let detectedText = 
    detected
    |> map (fun r -> r.Text)
    |> toReadOnlyProp

  let detectedImage = 
    detected
    |> map (fun r -> r.Path)
    |> toReadOnlyProp

  let undetectedText =
    undetected 
    |> map (fun r -> r.Text)
    |> toReadOnlyProp

  let undetectedImage =
    undetected
    |> map (fun r -> r.Path)
    |> toReadOnlyProp

  member val DownloadDirectory = downloadDirectory
  member val CameraDirectory = cameraDirectory
  member val FileMoveDirectory = fileMoveDirectory

  member val DetectedText = detectedText
  member val UndetectedText = undetectedText
  member val DetectedImage = detectedImage
  member val UndetectedImage = undetectedImage

  member val IsEnabled = isEnabled
