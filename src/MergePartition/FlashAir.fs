module FlashAir

open System.IO
open System.Net.Http
open System
open Microsoft.FSharp.Control

type FileInfo = {
    Directory: string
    Name: string
  }

type FlashAirClient(ssid: string) =
  let url = Uri("http://" + ssid)
  let httpClient = new HttpClient(BaseAddress = url)

  member this.List(dir: string) =
    let command = "command.cgi?op=100&DIR=/" + dir

    let getAsync = async {
      let! response = httpClient.GetAsync(command)
      let! content = response.Content.ReadAsStringAsync()
      return content
    }
    let result = getAsync |> Async.RunSynchronously
    let lines = result.Split('\n')

    lines 
    |> Seq.filter (fun s -> String.exists (fun c -> c = ',') s)
    |> Seq.map (fun s -> let fields = s.Split(',')
                         { Directory = fields.[0]; Name = fields.[1] })

  member this.Download(target: FileInfo, dst: string) =
    let command = target.Directory + "/" + target.Name
    let dst = Path.Combine(dst, target.Name)

    let getAsync = async {
      let! response = httpClient.GetAsync(command)
      let! buffer = response.Content.ReadAsByteArrayAsync()

      use stream = new FileStream(dst, FileMode.OpenOrCreate)
      do! stream.WriteAsync(buffer, 0, buffer.Length)
    }
    getAsync |> Async.RunSynchronously
    dst

  member this.IsUpdate() =
    let command = "command.cgi?op=102"

    let getAsync = async {
      let! response = httpClient.GetAsync(command)
      let! content = response.Content.ReadAsStringAsync()
      return content
    }
    let result = getAsync |> Async.RunSynchronously

    if result = "1" then true else false

  interface IDisposable with
    member this.Dispose() = httpClient.Dispose()

type FlashAirDownloader(ssid: string) as this =
  let downloaded = Event<string>()
  let client = new FlashAirClient(ssid)
  let cache = ResizeArray<FileInfo>()

  let watchAsync = async {
    while true do
      do! Async.Sleep 1000

      if this.IsEnabled && this.Local <> "" then
        if client.IsUpdate() then
          let newList = client.List(this.Remote)
          let diff = newList |> Seq.except cache |> Seq.toList
          if not diff.IsEmpty then
            diff
            |> Seq.map (fun s -> client.Download(s, this.Local))
            |> Seq.iter (fun s -> downloaded.Trigger s)
            cache.AddRange(diff)
  }

  do 
    watchAsync |> Async.Start

  member val IsEnabled = false with get, set
  member val Local = "" with get, set
  member val Remote = "" with get, set

  member this.Downloaded = downloaded.Publish

  interface IDisposable with
    member this.Dispose() = (client :> IDisposable).Dispose()
