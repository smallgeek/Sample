module Excel

  open FSharp.Control.Reactive
  open System
  open System.Reactive.Subjects
  open System.Reactive.Disposables

  type Cell<'a>(value: 'a) =
    inherit SubjectBase<'a>()

    let mutable value = value

    let gate = new obj()
    let observers = new ResizeArray<IObserver<'a>>()
    let mutable isStopped = false
    let mutable ex: Exception = null
    let mutable isDisposed = false

    let checkDisposed () = if isDisposed then raise <| ObjectDisposedException("")

    override val HasObservers = observers.Count > 0
    override val IsDisposed = lock gate (fun () -> isDisposed)

    member this.Value
      with get () = 
        lock gate (
          fun () -> 
            checkDisposed()
            if ex <> null then raise ex
            value
        )
      and set (v) =
        lock gate (
          fun () ->
            checkDisposed()
            if ex <> null then raise ex
            value <- v
        )

    /// すべての Observer に完了を通知します。
    override this.OnCompleted() =
      let mutable os: IObserver<'a>[] = null
      lock gate (
        fun () ->
          checkDisposed()
          if isStopped = false then
            os <- observers.ToArray()
            observers.Clear()
            isStopped <- true
      )

      if os <> null then
        for o in os do
          o.OnCompleted()

    /// すべての Observer に異常終了を通知します。
    override this.OnError(error: Exception) =
      if error = null then raise <| ArgumentNullException("error")

      let mutable os: IObserver<'a>[] = null
      lock gate (
        fun () ->
          checkDisposed()
          if isStopped = false then
            os <- observers.ToArray()
            observers.Clear()
            isStopped <- true
            ex <- error
      )

      if os <> null then
        for o in os do
          o.OnError(error)

    /// すべての Observer に要素の到着を通知します。
    override this.OnNext(v: 'a) =
      let mutable os: IObserver<'a>[] = null
      lock gate (
        fun () ->
          checkDisposed()

          if isStopped = false then
            value <- v
            os <- observers.ToArray()
      )

      if os <> null then
        for o in os do
          o.OnNext(v)
      
    override this.Subscribe(observer: IObserver<'a>) =
      if observer = null then raise <| new ArgumentNullException("observer")

      let mutable disposable: IDisposable = null

      lock gate (fun () ->
        checkDisposed()

        if isStopped = false then
          observers.Add(observer)
          observer.OnNext(value)
          
          disposable <- 
          { new IDisposable with
              member x.Dispose() =
                if observer <> null then
                  lock gate (fun () ->
                    if isDisposed = false && observer <> null then
                      observers.Remove(observer) |> ignore
                 )
          }
      )

      if disposable <> null then
        disposable
      else
        if ex <> null then
          observer.OnError(ex)
        else
          observer.OnCompleted()

        Disposable.Empty

    override this.Dispose() =
      lock gate (fun () ->
        isDisposed <- true
        observers.Clear()
        value <- Unchecked.defaultof<'a>
        ex <- null
      )

  let cell (x: 'a) = new Cell<'a>(x)

  type ExcelBuilder() =
    member __.Bind(m: IObservable<'a>, f: _ -> IObservable<'b>) =
                Observable.bind f m
    member __.Return(x) = 
                Observable.asObservable (new Cell<_>(x))

  let excel = new ExcelBuilder()

