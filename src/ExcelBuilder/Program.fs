open FSharp.Control.Reactive
open System
open System.Reactive.Subjects

let cell (x: 'a) = new BehaviorSubject<'a>(x)

type ExcelBuilder() =
  member __.Bind(m: IObservable<'a>, f: _ -> IObservable<'b>) =
              Observable.bind f m
  member __.Return(x) = 
              Observable.asObservable (new BehaviorSubject<_>(x))

let excel = new ExcelBuilder()

[<EntryPoint>]
let main (argv: string []) =

  let a1 = 1 |> cell
  let b1 = 2 |> cell
  let c1 = excel {
    let! x = a1
    let! y = b1
    return x + y
  }

  c1 |> Observable.subscribe (fun x -> printfn "C1 %i" x) |> ignore

  a1.OnNext 3
  a1.OnNext 5

  stdin.Read() |> ignore

  0
