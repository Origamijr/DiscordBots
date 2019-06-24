open System.IO
open System
open Discord
open Discord.Commands
open Discord.WebSocket
open Microsoft.Extensions.DependencyInjection
open System.Threading.Tasks
open Utils
open MessageReceivedHandler


let GetToken filename line =
    filename |> File.ReadAllLines |> (Seq.item line)

let BuildServices =
    let client = new DiscordSocketClient ()
    let commands = new CommandService ()
    let services = (((new ServiceCollection ()).AddSingleton (client)).AddSingleton (commands)).BuildServiceProvider ()
    (client, commands, services)

let Log (arg : LogMessage) =
    printfn "%s" arg.Message
    Task.CompletedTask

let Startup (client : DiscordSocketClient) =
    let botToken = GetToken "Tokens.txt" 2
    Await (client.LoginAsync (TokenType.Bot, botToken))
    Await (client.StartAsync ())
    Await (Task.Delay (-1))
    0

[<EntryPoint>]
let main argv = 
    let (client, _, _) = BuildServices
    client.add_Log (System.Func<_,_> (Log))
    client.add_MessageReceived (System.Func<_,_> (HandleMessageReceived))
    Await ("何" |> client.SetGameAsync)
    Startup client
