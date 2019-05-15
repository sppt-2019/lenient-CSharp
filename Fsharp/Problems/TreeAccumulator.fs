module Fsharp.Problems.TreeAccumulator

open Fsharp.TreeGenerator
open System.Threading.Tasks

let leaves a =
    let rec leavesAccum n leaves =
        match n with
        | (Leaf i) -> i :: leaves
        | Node (left, right, _) -> leavesAccum left (leavesAccum right leaves)
    leavesAccum a []

let lazyLeaves a =
    let lst = lazy(leaves a)
    lst.Value

let parallelLeaves t =
    let rec asyncAccum (t:tree) = async {        
        match t with
        | (Leaf i) -> return [i]
        | Node (left, right, _) -> return [(asyncAccum left); (asyncAccum right)]
                                       |> Async.Parallel
                                       |> Async.RunSynchronously
                                       |> List.concat
    }
    Async.RunSynchronously (asyncAccum t)

let tplParallelLeaves t = 
    let rec leavesAccum n =
        match n with
        | (Leaf i) -> [i] 
        | Node (left, right, _) ->
            let l = Task.Factory.StartNew<int64 list> (fun () ->  leavesAccum left)
            let r = Task.Factory.StartNew<int64 list> (fun () ->  leavesAccum right)
            Task.WaitAll(l, r)
            List.append l.Result r.Result
                                
    leavesAccum t

let lazyParallel t =
    let lst = lazy(parallelLeaves t)
    lst.Value
    
let lazyTPLLeaves t =
    let lst = lazy(tplParallelLeaves t)
    lst.Value
    
let lenientLeaves t =
    let rec lenientLeavesAccum n = async {
        match n with
        | Leaf i -> return [i]
        | Node (left, right, _) ->
            let! l = Async.StartChild (lenientLeavesAccum left)
            let! r = Async.StartChild (lenientLeavesAccum right)
            let! lr = l
            let! rr = r
            return List.concat [lr ; rr]
    }
    
    Async.RunSynchronously (lenientLeavesAccum t)