﻿using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DelBot.Databases;

namespace DelBot.Modules {

    public class BasicDatabaseCall : ModuleBase<SocketCommandContext> {

        string rememberTag = "remember";
        string rememberVarTag = "rememberVar";
        string rememberArrTag = "rememberArr";
        string dbName = "./Databases/BasicDB.json";

        // Hidden command
        [Command("purge")]
        public async Task PurgeAsync(string s = null) {

            if (Context.Message.Author.Username == "Alumina") {
                if (s == null) {
                    if (!(JsonDatabase.PurgeFile(dbName))) {
                        await ReplyAsync("Someone is reading the database. Unable to purge data at the moment.");
                        return;
                    }

                    await ReplyAsync("All records have been deleted. The perfect crime.");
                } else if (s[1] == '@') {
                    s = s.Substring(0, 2) + "!" + s.Substring(2);

                    if (!(JsonDatabase.PurgeUser(dbName, s))) {
                        await ReplyAsync("I don't know what you were expecting, Kevin. For some random patched together 100 lines of code to work correctly? Get real Kevin.");
                        return;
                    }

                    await ReplyAsync("All records partaining " + s + " has been deleted.");
                }
            } else {
                await ReplyAsync("My apologies. Only Alumina-dono can execute this command");
            }
        }

        // hidden command
        [Command("all")]
        public async Task AllAsync() {

            if (Context.Message.Author.Username == "Alumina") {
                List<string> userIds = JsonDatabase.ListHigh(dbName);

                if (userIds.Count == 0) {
                    await ReplyAsync("No one has told me to remember anything yet. Let's build some pleasant memories together Alumina-dono.");
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

                await ReplyAsync(msg);

            } else {
                await ReplyAsync("My apologies. Only Alumina-dono can execute this command");
            }
        }

        [Command("remember")]
        [Alias("rem")]
        public async Task RememberAsync([Remainder]string s = null) {

            string user = Context.User.Mention;
            string userId = "" + Utilities.GetId(user);

            JsonDatabase db = JsonDatabase.Open(dbName);

            if (!(db.IsOpen())) {
                await ReplyAsync("My apologies " + user + "-dono. Someone else is accessing the database...is what I'd like to say, but judging by the multiple failures thus far I cn safely say Kevin did something wrong.");
                return;
            }

            if (s == null) {
                // Read from default storage location
                string retrievedStr = db.AccessString(new List<string> { userId, rememberTag });


                if (StaticStates.verbatim) {
                    if (retrievedStr == null) {
                        await ReplyAsync(Utilities.RandomString(64));
                    } else {
                        await ReplyAsync(retrievedStr);
                    }
                } else if (Context.Message.Author.Username == "Del") {
                    if (retrievedStr == null) {
                        await ReplyAsync("I do not think that nobody hasn't taken the initiative to tell me to autonomously remember something for myself.");
                    } else {
                        await ReplyAsync("I remembered \"" + retrievedStr + "\". Are you proud of me?");
                    }
                } else {
                    if (retrievedStr == null) {
                        await ReplyAsync("You have not told me to remember anything yet, " + user + "-dono.");
                    } else {
                        await ReplyAsync("" + user + "-dono, You told me to remember \"" + retrievedStr + "\".");
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
                            if (Context.Message.Author.Username == "Del") {
                                await ReplyAsync("I guess I'll remember \"" + value + "\" with keywork \"" + varName + "\"");
                            } else {
                                await ReplyAsync("Remembered \"" + value + "\" with keywork \"" + varName + "\" for you " + user + "-dono.");
                            }
                        }
                    } else {
                        await ReplyAsync("My apologizes " + user + "-dono. An edge case occured that Kevin expected to only happen in debugging.");
                    }
                } else if (s.Substring(s.Length - 2) == "??") {
                    string varName = s.Substring(0, s.Length - 2);

                    // recall given keyword
                    string retrievedStr = db.AccessString(new List<string> { userId, rememberVarTag, varName });
                    
                    if (StaticStates.verbatim) {
                        if (retrievedStr == null) {
                            await ReplyAsync(Utilities.RandomString(64));
                        } else {
                            await ReplyAsync(retrievedStr);
                        }
                    } else if (Context.Message.Author.Username == "Del") {
                        if (retrievedStr == null) {
                            await ReplyAsync("I do not think that nobody hasn't taken the initiative to tell me to autonomously remember something for myself using the keyword \"" + varName + "\".");
                        } else {
                            await ReplyAsync("I remembered \"" + retrievedStr + "\" using the keyword \"" + varName + "\". Are you proud of me?");
                        }
                    } else {
                        if (retrievedStr == null) {
                            await ReplyAsync("You have not told me to remember anything yet using the keyword \"" + varName + "\", " + user + "-dono.");
                        } else {
                            await ReplyAsync("" + user + "-dono, You told me to remember \"" + retrievedStr + "\" using the keyword \"" + varName + "\".");
                        }
                    }
                } else {
                    // remember into default storage location
                    if (db.WriteString(new List<string> { userId, rememberTag }, s)) {
                        if (!StaticStates.verbatim) {
                            if (Context.Message.Author.Username == "Del") {
                                await ReplyAsync("I guess I'll remember \"" + s + "\"");
                            } else {
                                await ReplyAsync("Remembered \"" + s + "\" for you " + user + "-dono.");
                            }
                        }
                    } else {
                        await ReplyAsync("My apologizes " + user + "-dono. An edge case occured that Kevin expected to only happen in debugging.");
                    }
                }
            }

            if (!(db.Close())) {
                await ReplyAsync("My apologizes " + user + "-dono. Kevin messed up the program and tried to modify a database that never existed.");
            }
        }


        [Command("rememberArr")]
        public async Task RememberArrAsync([Remainder]string s = null) {

            string user = Context.User.Mention;
            string userId = "" + Utilities.GetId(user);

            JsonDatabase db = JsonDatabase.Open(dbName);
            
            if (!(db.IsOpen())) {
                await ReplyAsync("My apologies " + user + "-dono. Someone else is accessing the database...is what I'd like to say, but judging by the multiple failures thus far I cn safely say Kevin did something wrong.");
                return;
            }

            if (s == null) {
                string[] retrievedArr = db.AccessArray(new List<string> { userId, rememberArrTag });

                if (Context.Message.Author.Username == "Del") {
                    if (retrievedArr == null) {
                        await ReplyAsync("I do not think that nobody hasn't taken the initiative to tell me to autonomously remember something for myself.");
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
                        await ReplyAsync(msg);
                    }
                } else {
                    if (retrievedArr == null) {
                        await ReplyAsync("You have not told me to remember anything yet, " + user + "-dono.");
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
                        await ReplyAsync(msg);
                    }
                }

                if (!(db.Close())) {
                    await ReplyAsync("My apologizes " + user + "-dono. Kevin messed up the program and tried to modify a database that never existed.");
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
                        await ReplyAsync(msg);
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
                        await ReplyAsync(msg);
                    }
                } else {
                    await ReplyAsync("My apologizes " + user + "-dono. Kevin does not understand LINQ.");
                }

                if (!(db.Close())) {
                    await ReplyAsync("My apologizes " + user + "-dono. Kevin messed up the program and tried to modify a database that never existed.");
                }
            }
        }
    }
}