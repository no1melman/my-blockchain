module Utils

open System

let memToString (mem: ReadOnlyMemory<byte>) =
  mem.ToArray()
  |> System.Text.Encoding.UTF8.GetString

let spanToString (mem: ReadOnlySpan<byte>) =
  System.Text.Encoding.UTF8.GetString mem
