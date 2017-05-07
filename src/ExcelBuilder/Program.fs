open Excel

[<EntryPoint>]
let main (argv: string []) =

  let a1 = cell 1
  let b1 = cell 2

  let c1 = a1 @+ b1
  let d1 = a1 @- b1

  printfn "C1: 1 + 2 = %i" c1.Value
  printfn "D1: 1 - 2 = %i" d1.Value

  a1.Value <- 3
  printfn "C1: 3 + 2 = %i" c1.Value
  printfn "D1: 3 - 2 = %i" d1.Value

  a1.Value <- 5
  printfn "C1: 5 + 2 = %i" c1.Value
  printfn "D1: 5 - 2 = %i" d1.Value

  b1.Value <- 1
  printfn "C1: 5 + 1 = %i" c1.Value
  printfn "D1: 5 - 1 = %i" d1.Value

  0
