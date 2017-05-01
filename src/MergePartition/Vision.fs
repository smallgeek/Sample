module Vision

  open System
  open System.IO
  open Microsoft.ProjectOxford.Vision
  open Microsoft.ProjectOxford.Vision.Contract

  let recognize (imageFilePath:Uri) = async {
    let visionServiceClient = VisionServiceClient(Secret.Key)

    use imageFileStream = File.OpenRead(imageFilePath.LocalPath)
    System.Diagnostics.Debug.WriteLine(sprintf "Recognize: %s" imageFilePath.LocalPath)
    let! ocrResult = visionServiceClient.RecognizeTextAsync(imageFileStream, "ja")
    return ocrResult, imageFilePath.LocalPath
  }

  let toWords (results: OcrResults, path: string) =
    seq {
      if results.Regions.Length = 0 then
        yield "", path
      else
        for region in results.Regions do
          let value = 
            region.Lines
            |> Seq.collect (fun line -> line.Words)
            |> Seq.map (fun word -> word.Text)
            |> Seq.reduce (fun s acc -> s + acc)
          yield value, path
    }
