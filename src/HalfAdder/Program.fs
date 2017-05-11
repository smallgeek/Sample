open FSharp.Control.Reactive.Observable
open System.Reactive.Subjects

[<EntryPoint>]
let main argv = 

  let a = new BehaviorSubject<int>(0)
  let b = new BehaviorSubject<int>(0)

  let orGate  = zip a b |> map (fun (x, y) -> x ||| y)
  let andGate = zip a b |> map (fun (x, y) -> x &&& y)
  let notGate = andGate |> map (fun x -> ~~~ x)
  let s = zip orGate notGate |> map (fun (x, y) -> x &&& y)
  let c = andGate

  c |> subscribe (fun x -> x |> printf "C %i ") |> ignore
  s |> subscribe (fun x -> x |> printfn "S %i") |> ignore

  // Truth table
  //a.OnNext(false); b.OnNext(false)
  a.OnNext 0; b.OnNext 1
  a.OnNext 1; b.OnNext 0
  a.OnNext 1; b.OnNext 1

  0
