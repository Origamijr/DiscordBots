using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DelBot.Databases;
using Discord;

namespace DelBot.Modules {

    public class BasicDatabaseCall : ModuleBase<SocketCommandContext> {

        string rememberTag = "remember";
        string rememberVarTag = "rememberVar";
        string rememberArrTag = "rememberArr";
        string dbName = "runtime_db/DelBot/BasicDB.json";

        private void SendMessage(string msg) {
            Program.EnqueueMessage(msg, Context.Channel);
        }

        [Command("invade")]
        public async Task InvadeDBAsync(string s = null) {
            string name = null;
            if (s != null && (name = Utilities.GetUsername(s, Context)) != null) {
                JsonDatabase.SetDefaultKey("" + Utilities.GetId(s));
                SendMessage("Entering " + name + "'s room ( ͡° ͜ʖ ͡°)");
            } else {
                Console.WriteLine(name);
                JsonDatabase.SetDefaultKey(null);
                SendMessage("Returning to my own room.");
            }
        }

        // Hidden command
        [Command("purge")]
        public async Task PurgeAsync(string s = null) {

            if (Context.Message.Author.Username == "Alumina") {
                if (s == null) {
                    if (!(JsonDatabase.PurgeFile(dbName))) {
                        SendMessage("Someone is reading the database. Unable to purge data at the moment.");
                        return;
                    }

                    SendMessage("All records have been deleted. The perfect crime.");
                } else if (s[1] == '@') {
                    s = s.Substring(0, 2) + "!" + s.Substring(2);

                    if (!(JsonDatabase.PurgeUser(dbName, s))) {
                        SendMessage("I don't know what you were expecting, Kevin. For some random patched together 100 lines of code to work correctly? Get real Kevin.");
                        return;
                    }

                    SendMessage("All records partaining " + s + " has been deleted.");
                }
            } else {
                SendMessage("My apologies. Only Kevin can execute this command");
            }
        }

        // hidden command
        [Command("all")]
        public async Task AllAsync() {

            if (Context.Message.Author.Username == "Alumina") {
                List<string> userIds = JsonDatabase.ListHigh(dbName);

                if (userIds.Count == 0) {
                    SendMessage("No one has told me to remember anything yet. Let's build some pleasant memories together Alumina-dono.");
                    return;
                }

                List<string> users = new List<string>();

                foreach (string s in userIds) {
                    string u = Utilities.GetUsername(s, Context);
                    if (u != null) {
                        users.Add(u);
                    }
                }

                users.Sort();

                string msg = "The users I'm tracking data for are ";

                for (int i = 0; i < users.Count - 1; i++) {
                    msg += users[i] + ", ";
                }

                msg += users[users.Count - 1];

                SendMessage(msg);

            } else {
                SendMessage("My apologies. Only Alumina-dono can execute this command");
            }
        }

        [Command("remember")]
        [Alias("rem")]
        public async Task RememberAsync([Remainder]string s = null) {

            string user = Context.User.Mention;
            string userId = "" + Utilities.GetId(user);
            if (Utilities.IsSelf(user) && JsonDatabase.GetDefaultKey() != null) {
                userId = "" + Utilities.GetId(JsonDatabase.GetDefaultKey());
            }

            JsonDatabase db = JsonDatabase.Open(dbName);

            if (!(db.IsOpen())) {
                SendMessage("My apologies " + user + "-dono. Someone else is accessing the database...is what I'd like to say, but judging by the multiple failures thus far I cn safely say Kevin did something wrong.");
                return;
            }

            if (s == null) {
                // Read from default storage location
                string retrievedStr = db.AccessString(new List<string> { userId, rememberTag });


                if (StaticStates.verbatim) {
                    if (retrievedStr == null) {
                        SendMessage(Utilities.RandomString(64));
                    } else {
                        SendMessage(retrievedStr.Replace('\n', ' '));
                    }
                } else if (Utilities.IsSelf(user)) {
                    if (retrievedStr == null) {
                        if (JsonDatabase.GetDefaultKey() != null) {
                            SendMessage("Invaded " + Utilities.GetUsername(userId, Context) + "'s room and found nothing meaningful.");
                        } else {
                            SendMessage("I do not think that nobody hasn't taken the initiative to tell me to autonomously remember something for myself.");
                        }
                    } else {
                        if (JsonDatabase.GetDefaultKey() != null) {
                            SendMessage("Invaded " + Utilities.GetUsername(userId, Context) + "'s room and found a note saying \"" + retrievedStr + "\".");
                        } else {
                            SendMessage("I remembered \"" + retrievedStr + "\". Are you proud of me?");
                        }
                    }
                } else {
                    if (retrievedStr == null) {
                        SendMessage("You have not told me to remember anything yet, " + user + "-dono.");
                    } else {
                        SendMessage("" + user + "-dono, You told me to remember \"" + retrievedStr + "\".");
                    }
                }

            } else {
                // check if variable binding is involved
                int assignPos = s.IndexOf(":=");
                if (assignPos > 0 && s.Length > assignPos + 2) {
                    string varName = Utilities.TrimSpaces(s.Substring(0, assignPos));
                    string value = Utilities.TrimSpaces(s.Substring(assignPos + 2));

                    // remember into default storage location
                    if (db.WriteString(new List<string> { userId, rememberVarTag, varName }, value)) {
                        if (!StaticStates.verbatim) {
                            if (Utilities.IsSelf(user)) {
                                if (JsonDatabase.GetDefaultKey() != null) {
                                    SendMessage("Snuck a note titled \"" + varName + "\" into " + Utilities.GetUsername(userId, Context) + "'s room with the phrase \'" + value + "\".");
                                } else {
                                    SendMessage("I guess I'll remember \"" + value + "\" with keywork \"" + varName + "\"");
                                }
                            } else {
                                SendMessage("Remembered \"" + value + "\" with keywork \"" + varName + "\" for you " + user + "-dono.");
                            }
                        }
                    } else {
                        SendMessage("My apologizes " + user + "-dono. An edge case occured that Kevin expected to only happen in debugging.");
                    }
                } else if (s.Substring(s.Length - 2) == "??") {
                    string varName = s.Substring(0, s.Length - 2);

                    // recall given keyword
                    string retrievedStr = db.AccessString(new List<string> { userId, rememberVarTag, varName });
                    
                    if (StaticStates.verbatim) {
                        if (retrievedStr == null) {
                            SendMessage(Utilities.RandomString(64));
                        } else {
                            SendMessage(retrievedStr.Replace('\n', ' '));
                        }
                    } else if (Utilities.IsSelf(user)) {
                        if (retrievedStr == null) {
                            if (JsonDatabase.GetDefaultKey() != null) {
                                SendMessage("Didn't find any note titled \"" + varName + "\" in " + Utilities.GetUsername(userId, Context) + "'s room.");
                            } else {
                                SendMessage("I do not think that nobody hasn't taken the initiative to tell me to autonomously remember something for myself using the keyword \"" + varName + "\".");
                            }
                        } else {
                            if (JsonDatabase.GetDefaultKey() != null) {
                                SendMessage("Found a note titled \"" + varName + "\" in " + Utilities.GetUsername(userId, Context) + "'s room with the phrase \'" + retrievedStr + "\".");
                            } else {
                                SendMessage("I remembered \"" + retrievedStr + "\" using the keyword \"" + varName + "\". Are you proud of me?");
                            }
                        }
                    } else {
                        if (retrievedStr == null) {
                            SendMessage("You have not told me to remember anything yet using the keyword \"" + varName + "\", " + user + "-dono.");
                        } else {
                            SendMessage("" + user + "-dono, You told me to remember \"" + retrievedStr + "\" using the keyword \"" + varName + "\".");
                        }
                    }
                } else {
                    // remember into default storage location
                    if (db.WriteString(new List<string> { userId, rememberTag }, s)) {
                        if (!StaticStates.verbatim) {
                            if (Utilities.IsSelf(user)) {
                                if (JsonDatabase.GetDefaultKey() != null) {
                                    SendMessage("Posted a note with content \"" + s + "\" in " + Utilities.GetUsername(userId, Context) + "'s room.");
                                } else {
                                    SendMessage("I guess I'll remember \"" + s + "\"");
                                }
                            } else {
                                SendMessage("Remembered \"" + s + "\" for you " + user + "-dono.");
                            }
                        }
                    } else {
                        SendMessage("My apologizes " + user + "-dono. An edge case occured that Kevin expected to only happen in debugging.");
                    }
                }
            }

            if (!(db.Close())) {
                SendMessage("My apologizes " + user + "-dono. Kevin messed up the program and tried to modify a database that never existed.");
            }
        }



        [Command("evaluate")]
        [Alias("eval")]
        public async Task EvaluateAsync([Remainder]string s = null) {
            if (s == null) {
                if (!StaticStates.verbatim) {
                    SendMessage("Nothing to evaluate?");
                }
                return;
            }
            
            var messages = await Context.Channel.GetMessagesAsync(100).Flatten();
            string substituted = "";
            int back;
            int backPos = s.IndexOf(">>");
            int varPos = s.IndexOf("??");
            while ((backPos >= 0 && s.Length > backPos + 2) || varPos > 0) {
                if (backPos >= 0 && (varPos < 0 || backPos < varPos)) {
                    int l = 0;
                    while (backPos + 2 + l < s.Length && char.IsDigit(s[backPos + 2 + l])) l++;
                    if (l > 0 && int.TryParse(s.Substring(backPos + 2, l), out back) && back <= 100 && back > 0) {
                        substituted += s.Substring(0, backPos);
                        s = s.Substring(backPos + 2 + l);
                        int i = 0;
                        foreach (var message in messages) {
                            if (i > 0 && back == i) {
                                substituted += message.Content;
                                break;
                            }
                            i++;
                        }
                        backPos = s.IndexOf(">>");
                        varPos = s.IndexOf("??");
                    } else {
                        substituted += "err";
                        backPos = -1;
                    }
                } else {
                    int l = 1;
                    while (varPos - l > 0 && s[varPos - l - 1] != ' ' && Utilities.OperatorPrecedence(s[varPos - l - 1]) == 0) l++;
                    var user = Context.User.Mention;
                    string userId = "" + Utilities.GetId(user);
                    if (Utilities.IsSelf(user) && JsonDatabase.GetDefaultKey() != null) {
                        userId = "" + Utilities.GetId(JsonDatabase.GetDefaultKey());
                    }
                    string retrievedStr = JsonDatabase.ReadString(dbName, new List<string> { userId, rememberVarTag, s.Substring(varPos - l, l) });
                    substituted += s.Substring(0, varPos - l);
                    s = s.Substring(varPos + 2);
                    if (retrievedStr != null) {
                        substituted += retrievedStr;
                        backPos = s.IndexOf(">>");
                        varPos = s.IndexOf("??");
                    } else {
                        substituted += "err";
                        varPos = -1;
                    }
                }
            }
            Console.WriteLine(substituted + s);
            string postfix = Utilities.ConvertToPostFix(substituted + s);

            if (postfix != null) {
                try {
                    int result = Utilities.CalculatePostfixExpression(postfix);
                    if (StaticStates.verbatim) {
                        SendMessage("" + result);
                    } else {
                        SendMessage("Here is the result: " + result);
                    }
                } catch (FormatException) {
                    SendMessage(":rage: That is not a valid expression.");
                }
            } else {
                SendMessage(":rage: That is not a valid expression.");
            }
        }



        [Command("if")]
        public async Task IfAsync([Remainder]string s) {
            List<string> parts = Utilities.ParamSplit(s.Replace('\n', ' '));
            List<string> operators = new List<string>(new string[] { "is", "lessthan", "less", "isn't", "lesseq", "greaterthan", "greater", "greatereq", "=", "!=", ">", ">=", "<", "<=", "==" });

            if (parts.Count >= 5 && operators.Contains(parts[1].ToLower()) && parts[3].ToLower() == "then") {
                string expr1 = parts[0];
                string op = parts[1];
                string expr2 = parts[2];
                string remainder = "";

                // cut to teh remainder
                int i = s.IndexOf(expr1) + expr1.Length;
                remainder = s.Substring(i);
                i = remainder.IndexOf(op) + op.Length;
                remainder = remainder.Substring(i);
                i = remainder.IndexOf(expr1) + expr1.Length;
                remainder = remainder.Substring(i);
                i = remainder.IndexOf(parts[3]) + parts[3].Length;
                remainder = remainder.Substring(i);

                // find else
                string ifBlock = "";
                string elseBlock = "";
                int elseInd;
                for (elseInd = 5; elseInd < parts.Count; elseInd++) {
                    i = remainder.IndexOf(parts[elseInd - 1]) + parts[elseInd - 1].Length;
                    ifBlock += remainder.Substring(0, i);
                    remainder = remainder.Substring(i);
                    if (parts[elseInd] == "else") {
                        i = remainder.IndexOf("else") + 4;
                        elseBlock = remainder.Substring(i);
                        break;
                    }
                }
                if (elseInd >= parts.Count) ifBlock += remainder;

                var messages = await Context.Channel.GetMessagesAsync(100).Flatten();
                int back;
                if (expr1.Length > 2 && ">>" == expr1.Substring(0, 2) && int.TryParse(expr1.Substring(2), out back)) {
                    i = 0;
                    foreach (var message in messages) {
                        if (i > 0 && back == i) {
                            expr1 = message.Content;
                            break;
                        }
                        i++;
                    }
                } else if (expr1.Length > 2 && "??" == expr1.Substring(expr1.Length - 2)) {
                    var user = Context.User.Mention;
                    string userId = "" + Utilities.GetId(user);
                    if (Utilities.IsSelf(user) && JsonDatabase.GetDefaultKey() != null) {
                        userId = "" + Utilities.GetId(JsonDatabase.GetDefaultKey());
                    }
                    expr1 = JsonDatabase.ReadString(dbName, new List<string> { userId, rememberVarTag, expr1.Substring(0, expr1.Length - 2) });
                }

                if (expr2.Length > 2 && ">>" == expr2.Substring(0, 2) && int.TryParse(expr2.Substring(2), out back)) {
                    i = 0;
                    foreach (var message in messages) {
                        if (i > 0 && back == i) {
                            expr2 = message.Content;
                            break;
                        }
                        i++;
                    }
                } else if (expr2.Length > 2 && "??" == expr2.Substring(expr2.Length - 2)) {
                    var user = Context.User.Mention;
                    string userId = "" + Utilities.GetId(user);
                    if (Utilities.IsSelf(user) && JsonDatabase.GetDefaultKey() != null) {
                        userId = "" + Utilities.GetId(JsonDatabase.GetDefaultKey());
                    }
                    expr2 = JsonDatabase.ReadString(dbName, new List<string> { userId, rememberVarTag, expr2.Substring(0, expr2.Length - 2) });
                }

                Console.WriteLine("comparing " + expr1 + " and " + expr2);
                if (expr1 != null && expr2 != null) {
                    bool cond;
                    int int1, int2;
                    if (int.TryParse(expr1, out int1) && int.TryParse(expr2, out int2)) {
                        switch (op.ToLower()) {
                            case "is":
                            case "=":
                            case "==":
                                cond = int1 == int2;
                                break;
                            case "isn't":
                            case "!=":
                                cond = int1 != int2;
                                break;
                            case "lessthan":
                            case "less":
                            case "<":
                                cond = int1.CompareTo(int2) < 0;
                                break;
                            case "lesseq":
                            case "<=":
                                cond = int1.CompareTo(int2) <= 0;
                                break;
                            case "greaterthan":
                            case "greater":
                            case ">":
                                cond = int1.CompareTo(int2) > 0;
                                break;
                            case "greatereq":
                            case ">=":
                                cond = int1.CompareTo(int2) >= 0;
                                break;
                            default:
                                cond = false;
                                break;
                        }
                    } else {
                        switch (op.ToLower()) {
                            case "is":
                            case "=":
                            case "==":
                                cond = expr1 == expr2;
                                break;
                            case "isn't":
                            case "!=":
                                cond = expr1 != expr2;
                                break;
                            case "lessthan":
                            case "less":
                            case "<":
                                cond = expr1.CompareTo(expr2) < 0;
                                break;
                            case "lesseq":
                            case "<=":
                                cond = expr1.CompareTo(expr2) <= 0;
                                break;
                            case "greaterthan":
                            case "greater":
                            case ">":
                                cond = expr1.CompareTo(expr2) > 0;
                                break;
                            case "greatereq":
                            case ">=":
                                cond = expr1.CompareTo(expr2) >= 0;
                                break;
                            default:
                                cond = false;
                                break;
                        }
                    }

                    if (cond) {
                        SendMessage(Utilities.TrimSpaces(ifBlock));
                    } else if (elseInd < parts.Count - 1) {
                        SendMessage(Utilities.TrimSpaces(elseBlock));
                    }
                }

            } else if (!StaticStates.verbatim) {
                SendMessage("...what?");
            }
        }


        [Command("rememberArr")]
        public async Task RememberArrAsync([Remainder]string s = null) {

            string user = Context.User.Mention;
            string userId = "" + Utilities.GetId(user);

            JsonDatabase db = JsonDatabase.Open(dbName);
            
            if (!(db.IsOpen())) {
                SendMessage("My apologies " + user + "-dono. Someone else is accessing the database...is what I'd like to say, but judging by the multiple failures thus far I can safely say Kevin did something wrong.");
                return;
            }

            if (s == null) {
                string[] retrievedArr = db.AccessArray(new List<string> { userId, rememberArrTag });

                if (Context.Message.Author.Username == "Del") {
                    if (retrievedArr == null) {
                        SendMessage("I do not think that nobody hasn't taken the initiative to tell me to autonomously remember something for myself.");
                    } else {
                        string msg = "I remembered ";
                        for (int i = 0; i < retrievedArr.Length; i++) {
                            if (retrievedArr.Length <= 2 && i != 1) {
                                msg += retrievedArr[i];
                            } else if (retrievedArr.Length > 2 && i < retrievedArr.Length - 1) {
                                msg += retrievedArr[i] + ", ";
                            } else {
                                msg += " and " + retrievedArr[i];
                            }
                        }
                        msg += ". Are you extra proud of me?";
                        SendMessage(msg);
                    }
                } else {
                    if (retrievedArr == null) {
                        SendMessage("You have not told me to remember anything yet, " + user + "-dono.");
                    } else {
                        string msg = "I remembered ";
                        for (int i = 0; i < retrievedArr.Length; i++) {
                            if (retrievedArr.Length <= 2 && i != 1) {
                                msg += retrievedArr[i];
                            } else if (retrievedArr.Length > 2 && i < retrievedArr.Length - 1) {
                                msg += retrievedArr[i] + ", ";
                            } else {
                                msg += " and " + retrievedArr[i];
                            }
                        }
                        msg += " for you, " + user + "-dono.";
                        SendMessage(msg);
                    }
                }

                if (!(db.Close())) {
                    SendMessage("My apologizes " + user + "-dono. Kevin messed up the program and tried to modify a database that never existed.");
                }
            } else {
                string[] arr = Utilities.ParamSplit(s).ToArray();

                if (db.WriteArray(new List<string> { userId, rememberArrTag }, arr)) {
                    if (Context.Message.Author.Username == "Del") {
                        string msg = "I guess I'll remember ";
                        for (int i = 0; i < arr.Length; i++) {
                            if (arr.Length <= 2 && i != 1) {
                                msg += arr[i];
                            } else if (arr.Length > 2 && i < arr.Length - 1) {
                                msg += arr[i] + ", ";
                            } else {
                                msg += " and " + arr[i];
                            }
                        }
                        msg += ".";
                        SendMessage(msg);
                    } else {
                        string msg = "Remembered ";
                        for (int i = 0; i < arr.Length; i++) {
                            if (arr.Length <= 2 && i != 1) {
                                msg += arr[i];
                            } else if (arr.Length > 2 && i < arr.Length - 1) {
                                msg += arr[i] + ", ";
                            } else {
                                msg += " and " + arr[i];
                            }
                        }
                        msg += " for you, " + user + "-dono.";
                        SendMessage(msg);
                    }
                } else {
                    SendMessage("My apologizes " + user + "-dono. Kevin does not understand LINQ.");
                }

                if (!(db.Close())) {
                    SendMessage("My apologizes " + user + "-dono. Kevin messed up the program and tried to modify a database that never existed.");
                }
            }
        }
    }
}