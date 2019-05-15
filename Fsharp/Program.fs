open Fsharp
open Fsharp.Problems.MatrixSummer
open Fsharp.Problems.TreeAccumulator
open Fsharp.Problems
open LenientBenchmark
open System
open System.Collections.Generic
open System.IO
open TreeGenerator

type OS =
        | OSX            
        | Windows
        | Linux

let getOS = 
        match int Environment.OSVersion.Platform with
        | 4 | 128 -> Linux
        | 6       -> OSX
        | _       -> Windows

let OpenFile fileName =
    if File.Exists(fileName) then
        File.Delete(fileName)
    let file = new StreamWriter(fileName)
    file

let outputLinpackResults fileName (seqResults:Dictionary<int, BenchmarkResult>) (mrResults:Dictionary<int, BenchmarkResult>)
    (pResults:Dictionary<int,BenchmarkResult>) (tResults:Dictionary<int,BenchmarkResult>) =
    let fn = Path.Combine ("results", fileName)
    let f = OpenFile fn
    f.WriteLine "Problem Size,Sequential,Sequential Error,Map Reduce,Map Reduce Error,Parallel Foreach,Parallel Foreach Error, Tasks, Tasks Error"

    for res in seqResults do
        let mr = mrResults.[res.Key]
        let p = pResults.[res.Key]
        let t = tResults.[res.Key]
        let output = (sprintf "%i,%f,%f,%f,%f,%f,%f,%f,%f"
                        res.Key res.Value.Mean res.Value.StandardDeviation mr.Mean mr.StandardDeviation p.Mean
                        p.StandardDeviation t.Mean t.StandardDeviation)
        f.WriteLine(output)
        
    f.Flush()
    f.Close()
    
let outputTreeResults fileName (seqResults:Dictionary<int, BenchmarkResult>) (awResults:Dictionary<int, BenchmarkResult>)
    (tplResults:Dictionary<int,BenchmarkResult>) (lResults:Dictionary<int,BenchmarkResult>) =
    let fn = Path.Combine("results", fileName)
    let f = OpenFile fn
    f.WriteLine "Problem Size,Sequential,Sequential Error,Async Workflow,Async Workflow Error,TPL,TPL Error;Lenient;Lenient Error"

    for res in seqResults do
        let fj = awResults.[res.Key]
        let tpl = tplResults.[res.Key]
        let l = lResults.[res.Key]
        let output = (sprintf "%i,%f,%f,%f,%f,%f,%f,%f,%f"
                        res.Key res.Value.Mean res.Value.StandardDeviation fj.Mean fj.StandardDeviation tpl.Mean
                        tpl.StandardDeviation l.Mean l.StandardDeviation)
        f.WriteLine(output)
        
    f.Flush()
    f.Close()

let runMatrixSum () =
    let ms = MatrixSummer()
    let problems = [|1..12|] |> Array.map (fun n ->
        let nPow = 2.0f ** (float32 n)
        ms.Setup (int nPow))
    let msSeq = new McCollinRunner<int64 list list, int64>(new Func<int64 list list, int64>(ms.SumSequential), problems)
    let msMR = new McCollinRunner<int64 list list, int64>(new Func<int64 list list, int64>(ms.SumMapReduce), problems)
    let msP = new McCollinRunner<int64 list list, int64>(new Func<int64 list list, int64>(ms.SumParallel), problems)
    let msT = new McCollinRunner<int64 list list, int64>(new Func<int64 list list, int64>(ms.SumTasks), problems)
    
    msSeq.Run (100, (fun m -> m.Length))
    msMR.Run (100, (fun m -> m.Length))
    msP.Run (100, (fun m -> m.Length))
    msT.Run (100, (fun m -> m.Length))
    
    outputLinpackResults "fsharp-matrix.csv" msSeq.Results msMR.Results msP.Results msT.Results
    
    
let runMatrixSumRandom () =
    let ms = MatrixSummer()
    let problemSizes = [|1..12|] |> Array.map (fun n -> int (2.0f ** (float32 n)))
    let problems =
        problemSizes
        |> Array.map ms.Setup
    let msRandSeq = new MorellRunner<int64 list list, int64> (new Func<int64 list list, int64> (ms.SumSequential),
                                                              problems, problemSizes)
    let msRandMR = new MorellRunner<int64 list list, int64> (new Func<int64 list list, int64> (ms.SumMapReduce),
                                                              problems, problemSizes)
    let msRandP = new MorellRunner<int64 list list, int64> (new Func<int64 list list, int64> (ms.SumParallel),
                                                              problems, problemSizes)
    let msRandT = new MorellRunner<int64 list list, int64> (new Func<int64 list list, int64> (ms.SumTasks),
                                                              problems, problemSizes)
    
    msRandSeq.Run 100
    msRandMR.Run 100
    msRandP.Run 100
    msRandT.Run 100
    
    outputLinpackResults "fsharp-matrix-rand.csv" msRandSeq.Results msRandMR.Results msRandP.Results msRandT.Results

let runAccumulation () =
    let rnd = System.Random()
    let tg = TreeGenerator()
    let problems = [|1..5|] |> Array.map (fun n -> tg.CreateBinaryTree n (fun () -> rnd.NextLong()))
    let accumSeq = new McCollinRunner<tree, int64 list>(new Func<tree, int64 list>(leaves), problems)
    let accumAW = new McCollinRunner<tree, int64 list>(new Func<tree, int64 list>(parallelLeaves), problems)
    let accumTPL = new McCollinRunner<tree, int64 list>(new Func<tree, int64 list>(tplParallelLeaves), problems)
    let accumL = new McCollinRunner<tree, int64 list>(new Func<tree, int64 list>(lenientLeaves), problems)
    
    let getNoLeaves t =
        match t with
        | Node (_, _, n) -> n
        | Leaf (_) -> 1
    
    accumSeq.Run (100, (fun t -> getNoLeaves t))
    accumAW.Run (100, (fun t -> getNoLeaves t))
    accumTPL.Run (100, (fun t -> getNoLeaves t))
    accumL.Run (100, (fun t -> getNoLeaves t))
    
    outputTreeResults "fsharp-accum.csv" accumSeq.Results accumAW.Results accumTPL.Results accumL.Results

let runAccumulationRandom () =
    let rnd = System.Random()
    let problemSizes = [|1..5|] |> Array.map (fun n -> int (10.0f ** float32 n))
    let tg = TreeGenerator()
    let treeGen = (fun s -> tg.CreateTree s (fun () -> rnd.NextLong()))
    let problems =
        problemSizes
        |> Array.map treeGen
    
    let accumSeq = new MorellRunner<tree, int64 list>(new Func<tree, int64 list>(leaves), problems, problemSizes)
    let accumAW = new MorellRunner<tree, int64 list>(new Func<tree, int64 list>(leaves), problems, problemSizes)
    let accumTPL = new MorellRunner<tree, int64 list>(new Func<tree, int64 list>(leaves), problems, problemSizes)
    let accumL = new MorellRunner<tree, int64 list>(new Func<tree, int64 list>(lenientLeaves), problems, problemSizes)
    
    accumSeq.Run 100
    accumAW.Run 100
    accumTPL.Run 100
    accumL.Run 100
    
    outputTreeResults "fsharp-accum-rand.csv" accumSeq.Results accumAW.Results accumTPL.Results accumL.Results

let runSummation () =
    let rnd = System.Random()
    let tg = TreeGenerator()
    let problems = [|1..5|] |> Array.map (fun n -> tg.CreateBinaryTree n (fun () -> rnd.NextLong()))
    let accumSeq = new McCollinRunner<tree, int64>(new Func<tree, int64>(TreeSummer.sumLeaves), problems)
    let accumAW = new McCollinRunner<tree, int64>(new Func<tree, int64>(TreeSummer.parallelLeaves), problems)
    let accumTPL = new McCollinRunner<tree, int64>(new Func<tree, int64>(TreeSummer.tplParallelLeaves), problems)
    let accumL = new McCollinRunner<tree, int64>(new Func<tree, int64>(TreeSummer.lenientSum), problems)
    
    let getNoLeaves t =
        match t with
        | Node (_, _, n) -> n
        | Leaf (_) -> 1
    
    accumSeq.Run (100, (fun t -> getNoLeaves t))
    accumAW.Run (100, (fun t -> getNoLeaves t))
    accumTPL.Run (100, (fun t -> getNoLeaves t))
    accumL.Run (100, (fun t -> getNoLeaves t))
    
    outputTreeResults "fsharp-sum.csv" accumSeq.Results accumAW.Results accumTPL.Results accumL.Results

let runSummationRandom () =
    let rnd = System.Random()
    let problemSizes = [|1..5|] |> Array.map (fun n -> int (3.0f ** float32 n))
    let tg = TreeGenerator()
    let treeGen = (fun s -> tg.CreateTree s (fun () -> rnd.NextLong()))
    let problems =
        problemSizes
        |> Array.map treeGen
    
    let accumSeq = new MorellRunner<tree, int64>(new Func<tree, int64>(TreeSummer.sumLeaves), problems, problemSizes)
    let accumAW = new MorellRunner<tree, int64>(new Func<tree, int64>(TreeSummer.parallelLeaves), problems, problemSizes)
    let accumTPL = new MorellRunner<tree, int64>(new Func<tree, int64>(TreeSummer.tplParallelLeaves), problems, problemSizes)
    let accumL = new MorellRunner<tree, int64>(new Func<tree, int64>(TreeSummer.lenientSum), problems, problemSizes)
    
    accumSeq.Run 100
    accumAW.Run 1
    accumTPL.Run 100
    accumL.Run 100
    
    outputTreeResults "fsharp-sum-rand.csv" accumSeq.Results accumAW.Results accumTPL.Results accumL.Results

[<EntryPoint>]
let main argv =
    printfn "Welcome to spPT103f19 concurrency benchmarks in F#!"
    
//    printfn "Running Accumulation"
//    runAccumulation ()
//    printfn "Running Accumulation - Random"
//    runAccumulationRandom ()
    printfn "Running Summation"
    runSummation ()
    printfn "Running Summation - Random"
    runSummationRandom ()
//    printfn "Running Matrix Sum"
//    runMatrixSum ()
//    printfn "Running Matrix Sum - Random"
//    runMatrixSumRandom ()


    printfn "Benchmarking has completed. Press ENTER to exit."
    if getOS = Windows then
        System.Console.ReadLine() |> ignore
        0 // return an integer exit code
    else
        0 // return an integer exit code
