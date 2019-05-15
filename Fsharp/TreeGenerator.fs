module Fsharp.TreeGenerator

type tree =
    | Node of left : tree * right : tree * numberOfLeaves : int
    | Leaf of int64

type TreeGenerator() =
    let rnd = System.Random()

    member this.CreateBinaryTree depth valueGenerator =
        match depth with
        | 1 -> Leaf (valueGenerator())
        | n -> Node(
                   left = (this.CreateBinaryTree (depth - 1) valueGenerator),
                   right = (this.CreateBinaryTree (depth - 1) valueGenerator),
                   numberOfLeaves = int (2.0f ** (float32 depth)))

    member this.CreateTree maxChildren valueGenerator =
        if maxChildren = 1 then
            Leaf(valueGenerator())
        else 
            let nNodesLeft = rnd.Next(1, maxChildren)
            let nNodesRight = maxChildren - nNodesLeft
            let l = this.CreateTree nNodesLeft valueGenerator
            let r = this.CreateTree nNodesRight valueGenerator
            Node (l, r, maxChildren)