module Hash

open System
open System.Text
open System.Text.Json
open System.Security.Cryptography
open Microsoft.FSharp.NativeInterop


let private encoder (data: ReadOnlySpan<byte>) =
  Convert.ToBase64String(data).AsMemory()

let readOnly (span: Span<'a>) : ReadOnlySpan<'a> = Span<_>.op_Implicit(span)
let private toReadOnlySpan (arr: 'a array) = 
  readOnly (arr.AsSpan())

let toHexString (s: ReadOnlySpan<byte>) =
  Convert.ToHexString(s)

let createHmacSha3_256 key =
  new HMACSHA3_256 (key)

let hash key (block: Domain.Block<'a>)  = 
  let bytes = JsonSerializer.Serialize (block) |> Encoding.UTF8.GetBytes

  let hashFn = (createHmacSha3_256 key)

  let computedHash = hashFn.ComputeHash (bytes, 0, bytes.Length)

  hashFn.Clear()

  { block with Hash = Memory<_>.op_Implicit(computedHash.AsMemory ()) }

let rando = Random(int32(DateTime.UtcNow.Ticks))
let random64bitHex () =
  let buff = NativePtr.stackalloc<byte> 64 |> NativePtr.toVoidPtr
  let theSpan = Span<byte>(buff, 64)
  rando.NextBytes(theSpan)
  readOnly theSpan
