using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DelBot.HauteAuction {
    /*
    Types:
    int - arithmetic objects
    string - everything else

    Operators:
    (expr) - Evaluate : executes interior expression and replaces with return value
    {coll,iter,map,fold} - Collection : map must be an expression using coll, and fold must take two arguements
    
     */

    class AuctionExecuter {
        private AuctionResources resources;
        private AuctionGameflow flow;
        private List<AuctionItem> botQueue;
        private List<AuctionItem> fanfareQueue;
        private List<AuctionItem> actionQueue;
        private List<AuctionItem> sellQueue;
        private List<AuctionItem> presenceList;

        public AuctionExecuter(AuctionResources r, AuctionGameflow f) {
            this.resources = r;
            this.flow = f;
        }

        public List<string> ExecuteAcquire(AuctionItem item) {
            List<string> code = this.resources.GetItemData(item.itemId).acquireEffect.ToList();
            return ExecuteItemEffect(item, code);
        }

        public List<string> ExecuteActivate(AuctionItem item, List<string> args) {
            List<string> code = this.resources.GetItemData(item.itemId).acquireEffect.ToList();
            int leftBracket, bracketLen, index;
            for (int i = 0; i < code.Count; i++) {
                // Redo this part TODO
                while ((leftBracket = code[i].LastIndexOf('[')) != -1) {
                    bracketLen = code[i].IndexOf(']', leftBracket) - leftBracket;
                    if (int.TryParse(code[i].Substring(leftBracket + 1, bracketLen - 2), out index) && index > 0 && index < args.Count) {
                        code[i] = code[i].Substring(0, leftBracket) 
                            + args[index]
                            + code[i].Substring(leftBracket + bracketLen);
                    } else {
                        Console.Error.WriteLine("Incorrect arg[i] in parsing.");
                    }
                }
            }
            return ExecuteItemEffect(item, code);
        }

        private List<string> ExecuteItemEffect(AuctionItem item, List<string> lines) {
            var ret = new List<string>();
            for (int i = 0; i < lines.Count; i++) {
                // Preprocessing substitutions
                string line = lines[i];
                line = line.Replace("this", "i" + item.id.ToString());
                line = line.Replace("self", "p" + resources.GetPlayerWithItem(item).id);
                line = line.Replace("house", "c" + resources.house.id);
                line = line.Replace("game", "g");
                
                // Execution 
                line = Evaluate(line);

                // Post execution evaluation
                if (line != null && line[0] == '\\') {
                    if (line[1] == 'j') {
                        // Jump instruction
                        int distance;
                        if (int.TryParse(line.Substring(2), out distance)) {
                            i += distance - 1;
                        }
                    }
                    // Forward to executer caller to handle
                    ret.Add(line.Substring(2));
                }
            }

            return ret;
        }

        public string Evaluate(string expr) {
            int leftParen, parenLen;
            
            // Evaluate Collection if exists
            if ((leftParen = expr.IndexOf('{')) != -1) {
                parenLen = expr.LastIndexOf('}') - leftParen + 1;
                expr = expr.Substring(0, leftParen) 
                    + ProcessCollection(expr.Substring(leftParen, parenLen))
                    + expr.Substring(leftParen + parenLen);
            }

            // Evaluate Parentheses if exists
            while ((leftParen = expr.LastIndexOf('(')) != -1) {
                parenLen = expr.IndexOf(')', leftParen) - leftParen;
                expr = expr.Substring(0, leftParen) 
                    + Evaluate(expr.Substring(leftParen + 1, parenLen - 1))
                    + expr.Substring(leftParen + parenLen + 1);
            }

            // Determine what class is being operated upon, or if a built-in function is invoked
            string key = expr.Split()[0];
            List<string> args = Utilities.ParamSplit(expr).GetRange(1, expr.Split().Length - 1);
            int operand1, operand2, operand3;
            string result = null;
            switch (key) {
                case string s when s[0] == 'i':
                    AuctionItem item = resources.GetItem(key.Substring(1));
                    result = ExecuteItem(args, item);
                    break;

                case string s when s[0] == 'p':
                    AuctionPlayer player = resources.GetPlayer(key.Substring(1));
                    result = ExecutePlayer(args, player);
                    break;

                case string s when s[0] == 'b':
                    AuctionBot bot = resources.GetBot(key.Substring(1));
                    result = ExecuteBot(args, bot);
                    break;

                case string s when s[0] == 'c':
                    AuctionCollection coll = resources.GetCollection(key.Substring(1));
                    result = ExecuteCollection(args, coll);
                    break;

                case string s when s[0] == 'g':
                    result = ExecuteGameflow(args);
                    break;

                case "+":
                    if (args.Count == 2 && int.TryParse(args[0], out operand1) && int.TryParse(args[1], out operand2)) {
                        result = (operand1 + operand2).ToString();
                    }
                    break;

                case "-":
                    if (args.Count == 2 && int.TryParse(args[0], out operand1) && int.TryParse(args[1], out operand2)) {
                        result = (operand1 - operand2).ToString();
                    }
                    break;

                case "*":
                    if (args.Count == 2 && int.TryParse(args[0], out operand1) && int.TryParse(args[1], out operand2)) {
                        result = (operand1 * operand2).ToString();
                    }
                    break;

                case "/":
                    if (args.Count == 2 && int.TryParse(args[0], out operand1) && int.TryParse(args[1], out operand2)) {
                        result = (operand1 / operand2).ToString();
                    }
                    break;

                case "=":
                    result = ((args[0] == args[1]) ? 1 : 0).ToString();
                    break;

                case "!=":
                    result = ((args[0] != args[1]) ? 1 : 0).ToString();
                    break;

                case ">":
                    if (args.Count == 2 && int.TryParse(args[0], out operand1) && int.TryParse(args[1], out operand2)) {
                        result = ((operand1 > operand2) ? 1 : 0).ToString();
                    }
                    break;

                case "<":
                    if (args.Count == 2 && int.TryParse(args[0], out operand1) && int.TryParse(args[1], out operand2)) {
                        result = ((operand1 < operand2) ? 1 : 0).ToString();
                    }
                    break;

                case "?":
                    if (args.Count == 3 && int.TryParse(args[0], out operand1) && int.TryParse(args[1], out operand2) && int.TryParse(args[2], out operand3)) {
                        result = @"\j " + ((operand1 != 0) ? operand2 : operand3);
                    }
                    break;

                case "rand":
                    if (args.Count == 2 && int.TryParse(args[0], out operand1) && int.TryParse(args[1], out operand2)) {
                        Random r = new Random();
                        result = (r.Next(operand1, operand2 + 1)).ToString();
                    } else if (args.Count == 1 && args[0][0] == '[') {
                        Random r = new Random();
                        var arr = Utilities.ParamSplit(args[0].Substring(1, args[0].Length - 2), delim: ",", grouping: "{}[]()", strictGrouper: false);
                        result = arr[(r.Next(0, arr.Count))].ToString();
                    }
                    break;

                case "print":
                    result = String.Join(' ', args);
                    break;

            }
            
            return result;
        }

        private string ProcessCollection(string expr) {
            // Extract collection parts
            var parts = Utilities.ParamSplit(expr.Substring(1, expr.Length - 2), delim: ",", grouping: "{}[]()", strictGrouper: false)
                .Select(x => x.Trim())
                .ToList();

            // Retrieve the collection
            if (VerifyId(parts[0], 'c')) {
                var coll = resources.GetCollection(parts[0]);
                parts[0] = "[" + string.Join(",", coll.Select(x => x.ToString())) + "]";
            } else if (parts[0][0] != '[') {
                parts[0] = Evaluate(parts[0]);
                if (VerifyId(parts[0], 'c')) {
                    var coll = resources.GetCollection(parts[0]);
                    parts[0] = "[" + string.Join(",", coll.Select(x => x.ToString())) + "]";
                } else {
                    return null;
                }
            }

            string accumulator = null;
            bool filtering = !(parts.Count < 5 || parts[4] == "_");
            foreach (string thing in Utilities.ParamSplit(parts[0].Substring(1, parts[0].Length - 2), delim: ",", grouping: "{}[]()", strictGrouper: false)) {
                // Check the filter
                string filterExpr = parts[2].Replace(parts[1], thing);
                filterExpr = Evaluate(filterExpr);

                if (filterExpr != null && filterExpr != "0" || parts[2] == "_") {
                    // Run the map operation
                    string iterExpr = parts[3].Replace(parts[1], thing);
                    iterExpr = Evaluate(iterExpr);

                    // Run the fold operation
                    if (!filtering) {
                        if (accumulator == null) {
                            accumulator = "[" + iterExpr;
                        } else {
                            accumulator += "," + iterExpr;
                        }
                    } else if (accumulator == null) {
                        accumulator = iterExpr;
                    } else if (iterExpr != null) {
                        accumulator = Evaluate(parts[4] + " " + accumulator + " " + iterExpr);
                    }
                }
            }

            if (!filtering) accumulator += "]";

            return accumulator;
        }

        private string ExecuteItem(List<string> args, AuctionItem item) {
            switch (args[0]) {
                case "set":
                    item.SetLocal(args[1], args[2]);
                    break;
                    
                case "get":
                    item.GetLocal(args[1]);
                    break;

                case "discard":
                    resources.GetPlayerWithItem(item).Discard(item);
                    break;

                case "id":
                    return item.itemId;
            }
            return null;
        }

        private string ExecutePlayer(List<string> args, AuctionPlayer player) {
            //int x;
            switch (args[0]) {
                case "deck":
                    return "c" + player.deck.id;

                case "discard":
                    return "c" + player.discard.id;

                case "hand":
                    return "c" + player.hand.id;

                case "revealed":
                    return "c" + player.revealed.id;

                case "cookies":
                    switch (args[1]) {
                        case "get":
                            return player.cookies.ToString();

                        case "set":
                            player.cookies = int.Parse(args[2]);
                            break;

                        case "add":
                            player.cookies += int.Parse(args[2]);
                            break;
                    }
                    break;

                case "favor":
                    AuctionBot bot = resources.GetBot(args[2].Substring(1));
                    switch (args[1]) {
                        case "get":
                            return player.GetFavor(bot).ToString();

                        case "set":
                            player.SetFavor(bot, int.Parse(args[3]));
                            break;

                        case "add":
                            player.AddFavor(bot, int.Parse(args[3]));
                            break;
                    }
                    break;
            }
            return null;
        }

        private string ExecuteBot(List<string> args, AuctionBot bot) {
            switch (args[0]) {
                case "backing":
                    switch (args[1]) {
                        case "get":
                            return bot.GetBacking().ToString();

                        case "set":
                            AuctionPlayer player;
                            if (args[2] == "0") {
                                player = null;
                            } else {
                                player = resources.GetPlayer(args[2]);
                            }
                            bot.SetBacking(player);
                            break;
                    }
                    break;
            }
            return null;
        }

        private string ExecuteCollection(List<string> args, AuctionCollection coll) {
            if (args.Count == 0) {
                return coll.ToString();
            }
            return null;
        }

        private string ExecuteGameflow(List<string> args) {
            return null;
        }

        private bool VerifyId(string expr, char type) {
            return expr[0] == type && Regex.IsMatch(expr.Substring(1), @"^\d+$");
        }
    }
}