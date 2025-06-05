module Domain

open System
open System.Text

type Block<'T> =
    { Index: int64
      Timestamp: DateTime
      Data: 'T list
      Hash: ReadOnlyMemory<byte>
      PreviousHash: ReadOnlyMemory<byte>
      Nonce: ReadOnlyMemory<byte> }

    member self.AsHex =
        let newHash = Convert.ToHexString(self.Hash.ToArray())
        let hashBytes = Encoding.UTF8.GetBytes newHash
        ReadOnlySpan<_>.op_Implicit hashBytes

type Payment =
    { Sender: String
      Amount: decimal
      Receiver: String }

type Env =
    { LogInformation: string * obj array -> unit
      GetConfig: string -> string }
