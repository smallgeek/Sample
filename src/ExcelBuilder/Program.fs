open Excel

[<EntryPoint>]
let main (argv: string []) =

  let a1 = cell 1
  let b1 = cell 2
  let c1 = excel {
    let! x = a1
    let! y = b1
    return x + y
  }

  printfn "%i" c1.Value

  a1.Value <- 3  // 3 + 2
  printfn "%i" c1.Value

  a1.Value <- 5  // 5 + 2
  printfn "%i" c1.Value

  b1.Value <- 4  // 5 + 4
  printfn "%i" c1.Value

  0
