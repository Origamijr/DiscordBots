module Utils

open System.Threading.Tasks
open System.Text.RegularExpressions
open Discord.WebSocket
open Discord
open System.Collections.Generic

let Await (task: Task) =
    task |> Async.AwaitTask |> Async.RunSynchronously

let (|Regex|_|) pattern input =
    let m = Regex.Match(input, pattern)
    if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
    else None

let GetLastMessages (channel : ISocketMessageChannel) numMessages =
    let messages = channel.GetMessagesAsync(numMessages) :?> IAsyncEnumerable<IEnumerable<IMessage>>
    messages.FlattenAsync<IMessage>() |> Async.AwaitTask |> Async.RunSynchronously |> List.ofSeq

let AddReactions (message : SocketUserMessage) emojis =
    let emojimap = Array.map (fun e -> new Emoji(e) :> IEmote) emojis
    Await (MessageExtensions.AddReactionsAsync ((message :> IUserMessage), emojimap))