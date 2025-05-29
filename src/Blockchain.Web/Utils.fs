module Utils

open System

let memToString (mem: ReadOnlyMemory<byte>) =
  mem.ToArray()
  |> System.Text.Encoding.UTF8.GetString
