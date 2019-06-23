module Utils

open System.Threading.Tasks
open System.Text.RegularExpressions

let Await(task: Task) =
    task |> Async.AwaitTask |> Async.RunSynchronously

let (|Regex|_|) pattern input =
    let m = Regex.Match(input, pattern)
    if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
    else None