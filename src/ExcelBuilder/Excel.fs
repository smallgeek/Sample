module Excel

  open FSharp.Control.Reactive
  open System
  open System.Reactive.Subjects
  open System.Reactive.Disposables

  type ICell<'a> =
    inherit IObservable<'a>
    abstract member Value: 'a with get, set

  type CellObservable<'a, 'b>(m: ICell<'a>, f: 'a -> ICell<'b>) as self =
    let collectionDisposable = new CompositeDisposable()
    let gate = new obj()
    let mutable value: 'b = Unchecked.defaultof<'b>

    let subject = new BehaviorSubject<'b>(value) :> IObserver<'b>
    
    do
      // Publish like
      (self :> IObservable<'b>).Subscribe(subject) |> collectionDisposable.Add

    interface ICell<'b> with
      member this.Value 
        with get () = value
        and set (v) = value <- v

    interface IObservable<'b> with
      member self.Subscribe(observer: IObserver<'b>) =
        let sourceDisposable = new SingleAssignmentDisposable()
        let mutable isStopped = false

        let disposable = m.Subscribe(
          { new IObserver<'a> with
              member this.OnNext(v: 'a) =
                let nextObservable = f v

                let nextDisposable = new SingleAssignmentDisposable()
                nextDisposable |> collectionDisposable.Add

                let nextObserver = 
                  {
                    new IObserver<'b> with
                      member x.OnNext(v: 'b) =
                        lock gate (fun () ->
                          value <- v
                          observer.OnNext(v)
                        )

                      member x.OnError(error: Exception) =
                        lock gate (fun () -> observer.OnError(error))

                      member x.OnCompleted() =
                        if isStopped && collectionDisposable.Count = 1 then
                          lock gate (fun () -> observer.OnCompleted())
                  }

                nextDisposable.Disposable <- nextObservable.Subscribe(nextObserver)

              member this.OnError(error: Exception) =
                lock gate (fun () -> observer.OnError(error))

              member this.OnCompleted() =
                isStopped <- true
                if collectionDisposable.Count = 1 then
                  lock gate (fun () -> observer.OnCompleted())
                else
                  sourceDisposable.Dispose()
          })
        disposable

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

    interface ICell<'a> with
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
              this.OnNext(v)
          )

  let cell (x: 'a) = new Cell<'a>(x) :> ICell<'a>

  type ExcelBuilder() =

    member __.Bind(m: ICell<'a>, f: _ -> ICell<'b>) =
                CellObservable(m, f) :> ICell<'b>

    member __.Return(x: 'a) = 
                new Cell<'a>(x) :> ICell<'a>

  let excel = new ExcelBuilder()

  let inline (@+) cell1 cell2 =
    let newCell = excel {
      let! x = cell1
      let! y = cell2
      return x + y
    }
    newCell 

  let inline (@-) cell1 cell2 =
    let newCell = excel {
      let! x = cell1
      let! y = cell2
      return x - y
    }
    newCell

  let inline (@*) cell1 cell2 =
    let newCell = excel {
      let! x = cell1
      let! y = cell2
      return x * y
    }
    newCell

  let inline (@/) cell1 cell2 =
    let newCell = excel {
      let! x = cell1
      let! y = cell2
      return x / y
    }
    newCell
