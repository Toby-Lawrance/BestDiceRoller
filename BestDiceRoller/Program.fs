// Learn more about F# at http://fsharp.org

open System
open Discord
open Discord.WebSocket

let createClient = new DiscordSocketClient


let async log msg = printfn "%s" (string msg)

let async mainAsync = 
    let _client = createClient
    _client.add_Log log

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    0 // return an integer exit code
