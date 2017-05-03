module Adder
  open Unqualified
  open System
  open System.Reactive.Subjects

  type HalfAdder() =
    let a = new BehaviorSubject<int>(0)
    let b = new BehaviorSubject<int>(0)

    let orGate  = zip a b |> map (fun (x, y) -> x ||| y)
    let andGate = zip a b |> map (fun (x, y) -> x &&& y)
    let notGate = andGate |> map (fun x -> ~~~ x)
    let s = zip orGate notGate |> map (fun (x, y) -> x &&& y)
    let c = andGate

    member this.InputA = a.OnNext
    member this.InputB = b.OnNext
    member val Sum = s
    member val Carry = c
