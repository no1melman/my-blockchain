module Blockchain

open System
open System.Text
open FSharp.NativeInterop

open Domain
open System.Threading
open System.Threading.Tasks

let mutable blocks: Block<Payment> list = []
let mutable pendingTransactions: Payment list = []
let mutable hashKey = ""

let addTransaction payment =
  pendingTransactions <- pendingTransactions @ [payment]

let hash (block: Block<'a>) = 
  Hash.hash (Encoding.UTF8.GetBytes hashKey) block

let newblock () =
  let emptyHash : ReadOnlyMemory<byte> = Encoding.UTF8.GetBytes("")
  let initial = {
    Index = List.length blocks |> (fun i -> int64(i + 1))
    Timestamp = DateTime.UtcNow
    Data = pendingTransactions
    Hash = emptyHash
    PreviousHash = (List.tryLast blocks) |> Option.map (fun a -> a.Hash) |> Option.defaultValue emptyHash
    Nonce =  Hash.random64bitHex().ToArray()
  }

  let hashedBlock = hash initial

  hashedBlock

let lastblock () =
  List.last blocks

let validateSpan (part: ReadOnlySpan<byte>) =
  let checkbuffer = NativePtr.stackalloc<byte> 4
  let checkSpan = Span<byte>(checkbuffer |> NativePtr.toVoidPtr, 4)

  checkSpan.Fill(0uy)
  part.SequenceEqual checkSpan

let validBlock (block: Block<'a>) =
  validateSpan(block.Hash.Slice(0,4).Span)

let proofOfWork (cancellationToken: CancellationToken) =
  let paraOpts = 
    let opts = ParallelOptions()
    opts.CancellationToken <- cancellationToken
    opts

  let mutable maybeValidBlock : Block<Payment> option = None 
  let _ = Parallel.For(0L, Int64.MaxValue, paraOpts, (
    fun _ state -> 
      let mutable keepGoing = true
      let mutable innerMaybeValidBlock : Block<Payment> option = None 
      while true && keepGoing && (not cancellationToken.IsCancellationRequested) do
        let potential = newblock ()
        if validBlock potential then
          innerMaybeValidBlock <- Some potential
          keepGoing <- false 
      state.Stop()
      maybeValidBlock <- innerMaybeValidBlock
      ()  
  ))

  maybeValidBlock 
  |> function 
     | Some block -> 
          pendingTransactions <- []
          blocks <- blocks @ [block]
          block
     | None -> invalidOp "block hasn't been set"
   

let init key =

  hashKey <- key
  blocks <- []
  pendingTransactions <- []

  newblock ()

