module Fsharp.Problems.MatrixSummer

open LenientBenchmark
open System.Threading.Tasks

type MatrixSummer () =
    let (+!) (x:int64) (y:int64) = Operators.(+) x y
    let SumUnchecked x y = x +! y

    member this.Rnd = System.Random()
    member this.NextLong () =
        RandomExtensions.NextLong this.Rnd

    member this.Setup (n:int) = 
        let rec GenerateRandomList m =
            match m with
            | 0 -> [this.NextLong()]
            | i -> this.NextLong() :: GenerateRandomList (i-1)
        let rec GenerateListOfLists m =
            match m with
            | 0 -> [GenerateRandomList n]
            | i -> GenerateRandomList n :: GenerateListOfLists (i-1)
        GenerateListOfLists n

    member this.SumSequential matrix =
        let rec SumColumn c =
            match c with
            | [] -> 0L
            | c::rest -> c + (SumColumn rest)

        match matrix with
        | [] -> 0L
        | c::rest -> SumColumn c + (this.SumSequential rest)

    member this.SumMapReduce matrix =
        matrix
        |> List.map (fun c -> List.reduce SumUnchecked c)
        |> List.reduce SumUnchecked

    member this.SumParallel matrix =
        matrix      
        |> List.map (fun c -> async {return List.reduce SumUnchecked c})
        |> Async.Parallel
        |> Async.RunSynchronously
        |> Array.reduce SumUnchecked

    member this.SumTasks matrix =
        matrix      
        |> List.map (fun c -> Task.Factory.StartNew (fun () -> List.reduce SumUnchecked c))
        |> List.map (fun t -> t.Result)
        |> List.reduce SumUnchecked