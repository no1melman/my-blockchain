module Domain

open System

type Block<'T> = 
  {
    Index: int64
    Timestamp: DateTime
    Data: 'T list
    Hash: ReadOnlyMemory<byte>
    PreviousHash: ReadOnlyMemory<byte>
    Nonce: ReadOnlyMemory<byte>
  }

type Payment =
  {
    Sender: String
    Amount: decimal
    Receiver: String
  }

type Env =
  {
    LogInformation: string * obj array -> unit
    GetConfig: string -> string
  }
