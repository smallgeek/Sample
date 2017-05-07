module CellOperators

open Excel

type AddOperator = AddOperator with
  static member inline (?<-) (cell1: ICell<_>, AddOperator, cell2: ICell<_>) =
    let newCell = excel {
      let! x = cell1
      let! y = cell2
      return x + y
    }
    newCell 

  static member inline (?<-) (x, AddOperator, y) = x + y

type SubOperator = SubOperator with
  static member inline (?<-) (cell1: ICell<_>, SubOperator, cell2: ICell<_>) =
    let newCell = excel {
      let! x = cell1
      let! y = cell2
      return x - y
    }
    newCell 

  static member inline (?<-) (x, SubOperator, y) = x - y

type MulOperator = MulOperator with
  static member inline (?<-) (cell1: ICell<_>, MulOperator, cell2: ICell<_>) =
    let newCell = excel {
      let! x = cell1
      let! y = cell2
      return x * y
    }
    newCell 

  static member inline (?<-) (x, MulOperator, y) = x * y

type DivOperator = DivOperator with
  static member inline (?<-) (cell1: ICell<_>, DivOperator, cell2: ICell<_>) =
    let newCell = excel {
      let! x = cell1
      let! y = cell2
      return x / y
    }
    newCell 

  static member inline (?<-) (x, DivOperator, y) = x / y

let inline (+) x y = x ? (AddOperator) <- y
let inline (-) x y = x ? (SubOperator) <- y
let inline (*) x y = x ? (MulOperator) <- y
let inline (/) x y = x ? (DivOperator) <- y

let inline (<--) (cell: ICell<'a>) (value: 'a) = cell.Value <- value