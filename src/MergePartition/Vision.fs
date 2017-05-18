module Vision

open System
open System.IO
open Microsoft.ProjectOxford.Vision
open Microsoft.ProjectOxford.Vision.Contract

type OcrResults with
  member this.ToText =
    let source = seq {
      if this.Regions.Length = 0 then
        yield ""
      else
        for region in this.Regions do
          let value = 
            region.Lines
            |> Seq.collect (fun line -> line.Words)
            |> Seq.map (fun word -> word.Text)
            |> Seq.reduce (fun acc text -> acc + text)
          yield value
    }
    source |> Seq.reduce (fun acc s -> s + "\r\n" + acc)

type RecognizeResults = { Text: string; Path: string } with
  member this.IsDetected = this.Text |> String.IsNullOrEmpty |> not

let recognize (imageFilePath:Uri) = async {
  let visionServiceClient = VisionServiceClient(Secret.Key)
  use imageFileStream = File.OpenRead(imageFilePath.LocalPath)

  System.Diagnostics.Debug.WriteLine(sprintf "Recognize: %s" imageFilePath.LocalPath)

  let! ocrResult = visionServiceClient.RecognizeTextAsync(imageFileStream, "ja")
  return { Text = ocrResult.ToText; Path = imageFilePath.LocalPath }
}
