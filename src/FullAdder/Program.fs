open Adder
open System.Reactive.Subjects
open FSharp.Control.Reactive.Observable

[<EntryPoint>]
let main argv = 
  
  let a = new BehaviorSubject<int>(0)
  let b = new BehaviorSubject<int>(0)
  let x = new BehaviorSubject<int>(0)

  let upperHalfAdder = HalfAdder()
  let lowerHalfAdder = HalfAdder()

  a |> subscribe upperHalfAdder.InputA |> ignore
  b |> subscribe upperHalfAdder.InputB |> ignore
  x |> subscribe lowerHalfAdder.InputB |> ignore
  upperHalfAdder.Sum |> subscribe lowerHalfAdder.InputA |> ignore

  let s = lowerHalfAdder.Sum
  let c = zip upperHalfAdder.Carry lowerHalfAdder.Carry |> map (fun (x, y) -> x ||| y)

  c |> subscribe (fun x -> x |> printf "C %i ") |> ignore
  s |> subscribe (fun x -> x |> printfn "S %i") |> ignore

  // Truth table
  //a.OnNext 0; b.OnNext 0; x.OnNext 0
  a.OnNext 0; b.OnNext 0; x.OnNext 1
  a.OnNext 0; b.OnNext 1; x.OnNext 0
  a.OnNext 0; b.OnNext 1; x.OnNext 1
  a.OnNext 1; b.OnNext 0; x.OnNext 0
  a.OnNext 1; b.OnNext 0; x.OnNext 1
  a.OnNext 1; b.OnNext 1; x.OnNext 0
  a.OnNext 1; b.OnNext 1; x.OnNext 1

  0
