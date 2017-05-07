open Excel
open CellOperators

[<EntryPoint>]
let main (argv: string []) =

  let a1 = cell 1
  let b1 = cell 2

  let c1 = a1 + b1
  let d1 = a1 - b1

  printfn "C1: 1 + 2 = %A" c1
  printfn "D1: 1 - 2 = %A" d1

  a1 <-- 3
  printfn "C1: 3 + 2 = %A" c1
  printfn "D1: 3 - 2 = %A" d1

  a1 <-- 5
  printfn "C1: 5 + 2 = %A" c1
  printfn "D1: 5 - 2 = %A" d1

  b1 <-- 1
  printfn "C1: 5 + 1 = %A" c1
  printfn "D1: 5 - 1 = %A" d1

  0
