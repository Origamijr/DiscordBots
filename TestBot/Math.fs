module Math
open Utils

let rec ExprFinder nums result =
    let rec ExprFinderPermHelper nums result ind =
        let ExprFinderOpHelper num rest result =
            match ExprFinder rest (result - num) with
            | Some expr -> Some ("(" + expr + "+" + (string num) + ")")
            | None -> match ExprFinder rest (result + num) with
                      | Some expr -> Some ("(" + expr + "-" + (string num) + ")")
                      | None -> match ExprFinder rest (result / num) with
                                | Some expr when result % num = 0 -> Some ("(" + expr + "*" + (string num) + ")")
                                | _ -> match ExprFinder rest (result * num) with
                                       | Some expr -> Some ("(" + expr + "/" + (string num) + ")")
                                       | None -> None
        let num, rest = ListExtract nums ind
        match num with
        | Some n -> match ExprFinderOpHelper n rest result with
                    | Some expr -> Some expr
                    | None when ind < nums.Length - 1 -> ExprFinderPermHelper nums result (ind + 1)
                    | None -> None
        | None -> None
    match nums with
    | [x] when x = result -> Some (string x)
    | _::_ -> ExprFinderPermHelper nums result 0
    | _ -> None