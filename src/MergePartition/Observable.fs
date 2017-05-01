module FSharp.Control.Reactive.Observable
  open System
  open Reactive.Bindings

  let partitionHot (predicate: _ -> bool) (source: IObservable<_>) = 
    source 
    |> Observable.publish 
    |> Observable.refCount 
    |> Observable.partition predicate

  let toProp (source: IObservable<_>) = source.ToReactiveProperty()

  let toReadOnlyProp (source: IObservable<_>) = source.ToReadOnlyReactiveProperty()

