open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Http

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)

    let blockkey = builder.Configuration.GetSection("BlockKey").Get<string>()

    let app = builder.Build()

    let getEnv () : Domain.Env =
        { LogInformation =
            (app.Services.GetService(typedefof<ILoggerFactory>) :?> ILoggerFactory)
                .CreateLogger("application")
                .LogInformation
          GetConfig = fun s -> builder.Configuration.GetSection(s).Get<string>() }

    let initialBlock = Blockchain.init blockkey

    app.MapGet("/", Func<string>(fun () -> Utils.spanToString initialBlock.AsHex))
    |> ignore

    app.MapPost(
        "/",
        Func<Domain.Payment, HttpContext, string>(fun payment ctx ->
            Blockchain.addTransaction payment

            let block = Blockchain.proofOfWork ctx.RequestAborted

            Utils.spanToString block.AsHex)
    )
    |> ignore

    app.MapGet(
        "/random",
        Func<obj>(fun () ->
            let checkit = Span<byte>([| 0uy; 1uy; 0uy; 0uy |])

            {| valid = Blockchain.validateSpan (Span<_>.op_Implicit checkit)
               hash = Hash.toHexString (Hash.random64bitHex ()) |})
    )
    |> ignore

    app.Run()

    0 // Exit code
