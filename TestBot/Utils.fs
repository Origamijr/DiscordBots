module Utils

open System.Threading.Tasks
open System.Text.RegularExpressions
open Discord.WebSocket
open Discord
open System.Collections.Generic

// General Utils

let Await (task: Task) =
    task |> Async.AwaitTask |> Async.RunSynchronously

let (|Regex|_|) pattern input =
    let m = Regex.Match(input, pattern)
    if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
    else None

let ListExtract l i =
    let rec ListExtractHelper l i curr acc =
        match l with
        | hd::tl when i = curr -> (Some hd), (List.append (List.rev acc) tl)
        | hd::tl -> ListExtractHelper tl i (curr + 1) (hd::acc)
        | [] -> None, List.rev acc
    ListExtractHelper l i 0 []
    
let StoI (str : string) =
    let success, i = System.Int32.TryParse (Regex.Replace (str, "[^0-9]", ""))
    match success with
    | true -> Some i
    | false -> None
    
let ExtractNums (str : string) =
    let tokens = str.Split [|' '|] |> List.ofArray
    List.map (fun x -> match x with Some thing -> thing | None -> 0) (List.filter (fun x -> match x with None -> false | _ -> true) (List.map (StoI) tokens))

// Discord Utils

let GetLastMessages (channel : ISocketMessageChannel) numMessages =
    let messages = channel.GetMessagesAsync(numMessages) :?> IAsyncEnumerable<IEnumerable<IMessage>>
    messages.FlattenAsync<IMessage>() |> Async.AwaitTask |> Async.RunSynchronously |> List.ofSeq

let AddReactions (message : SocketUserMessage) emojis =
    let emojimap = Array.map (fun e -> new Emoji(e) :> IEmote) emojis
    Await (MessageExtensions.AddReactionsAsync ((message :> IUserMessage), emojimap))

let AddEscapes (message : string) =
    let s1 = message.Replace (@"\", @"\\")
    let s2 = s1.Replace ("*", @"\*")
    let s3 = s2.Replace ("_", @"\_")
    s3.Replace ("|", @"\|")