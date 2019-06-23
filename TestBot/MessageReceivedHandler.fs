module MessageReceivedHandler

open Discord.WebSocket
open Utils
open System.Threading.Tasks

let rnd = System.Random ()

let ThrowRandomMessage channelName =
    let r = rnd.NextDouble ()
    let cookieRate = 
        match channelName with
        | "general" -> 0.0001
        | _ -> 0.0
    let existRate =
        match channelName with
        | "general" -> 0.01
        | "nsfw" -> 0.2
        | _ -> 0.0

    match (r < cookieRate, r < existRate) with
    | (true, _) -> Some "You have gotten the rare message. Have a cookie :cookie:"
    | (false, true) -> Some "Hello. I exist to say I exist."
    | (_, _) -> None

let HandleUserMessage(message : SocketMessage) =
    match message.Content.ToLower() with
    | "//ping" -> Some "pong"
    | Regex @"^.*drop.*f.*chat.*$" _ -> Some "F"
    | _ -> ThrowRandomMessage message.Channel.Name

let HandleBotMessage(message : SocketMessage) =
    None

let GetResponse(message : SocketMessage) =
    match message.Author.IsBot with
    | true -> HandleBotMessage message
    | false -> HandleUserMessage message

let SendResponse(response : string, channel : ISocketMessageChannel) =
    channel.SendMessageAsync response :> Task

let HandleMessageReceived(message: SocketMessage) =
    match (GetResponse message) with
    | Some response -> SendResponse (response, message.Channel)
    | None -> Task.CompletedTask