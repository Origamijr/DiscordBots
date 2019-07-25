﻿module MessageReceivedHandler

open Discord
open Discord.WebSocket
open Utils
open System.Threading.Tasks

let rnd = System.Random ()

// User Messages ===============================================================

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

let HandleUserMessage (message : SocketUserMessage) =
    match message.Content.ToLower () with
    | "//ping" -> 
        match message.Author.Username with
        | "Alumina" -> 
            AddReactions message [|"🇵";"🇴";"🇳";"🇬";"🤖"|]
            None
        | _ -> None
    | Regex @"^.*drop.*f.*chat.*$" _ -> Some "F"
    | _ -> ThrowRandomMessage message.Channel.Name

// Bot Messages ================================================================

let HandleSChanMessage (message : SocketUserMessage) =
    let lastMessage = 
        match GetLastMessages message.Channel 2 with
        | [_; lm] -> Some lm
        | _ -> None
    match message.Content.ToLower () with
    | "f" -> 
        match lastMessage with
        | Some lm ->
            match (lm.Author.Username, lm.Content) with
            | ("TestBot", "F") ->
                AddReactions message [|"🐌"|]
                None
            | (_,_) -> None
        | None -> None
    | Regex @"^.*testbot.*cuck.*$" _ -> 
        AddReactions message [|"🇼";"🇭";"🇾";"😢"|]
        None
    | Regex @"^.*blaze.*it.*$" _ ->
           AddReactions message [|"🔥"|]
           None
    | _ -> None
    
let HandleBotMessage (message : SocketUserMessage) =
    match message.Author.Username with
    | "TestBot" -> None
    | "Shit-chan" -> HandleSChanMessage message
    | _ -> None


// Handle Response =============================================================

let GetResponse (message : SocketMessage) =
    match message.Author.IsBot with
    | true -> HandleBotMessage (message :?> SocketUserMessage)
    | false -> HandleUserMessage (message :?> SocketUserMessage)

let SendResponse response (channel : ISocketMessageChannel) =
    channel.SendMessageAsync response :> Task

let HandleMessageReceived (message: SocketMessage) =
    printf "%d: %s\n" message.Author.Id message.Content
    match (GetResponse message) with
    | Some response -> SendResponse response message.Channel
    | None -> Task.CompletedTask