module Fsharp.Problems.TreeSummer

open Fsharp.TreeGenerator
open System.Threading.Tasks

let (+!) (x:int64) (y:int64) = Operators.(+) x y

let rec sumLeaves a =
    match a with
    | Leaf i -> i
    | Node (left, right, _) -> (sumLeaves left) + (sumLeaves right)

let lazyLeaves a =
    let lst = lazy(sumLeaves a)
    lst.Value

let parallelLeaves t =
    let rec asyncAccum t = async {        
        match t with
        | Leaf i -> return i
        | Node (left, right, _) ->
            let ress =
                [left ; right]
                |> List.map asyncAccum
                |> Async.Parallel
                |> Async.RunSynchronously
            return ress.[0] +! ress.[1]
    }
    Async.RunSynchronously (asyncAccum t)

let tplParallelLeaves t = 
    let rec leavesAccum n =
        match n with
        | Leaf i -> i
        | Node (left, right, _) ->
            let l = Task.Factory.StartNew (fun () ->  leavesAccum left)
            let r = Task.Factory.StartNew (fun () ->  leavesAccum right)
            Task.WaitAll(l, r)
            l.Result + r.Result
                                
    leavesAccum t

let lazyParallel t =
    let lst = lazy(parallelLeaves t)
    lst.Value
    
let lazyTPLLeaves t =
    let lst = lazy(tplParallelLeaves t)
    lst.Value
    
let lenientSum t =
    let rec lenientSum (t:tree) = async {
        match t with
        | Leaf i -> return i
        | Node (left, right, _) ->
            let! l = Async.StartChild (lenientSum left)
            let! r = Async.StartChild (lenientSum right)
            let! lr = l
            let! rr = r
            return lr +! rr
    }
    Async.RunSynchronously (lenientSum t)