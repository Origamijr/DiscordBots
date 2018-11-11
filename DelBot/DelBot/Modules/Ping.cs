﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DelBot.Modules {

    public class Ping : ModuleBase<SocketCommandContext> {

        [Command("ping")]
        public async Task PingAsync() {
            await ReplyAsync("pong");
        }

        [Command("stahp")]
        public async Task StahpAsync() {
            Program.MessageQueue.Clear();
            await ReplyAsync("ok");
        }

        [Command("hello")]
        public async Task HelloAsync() {
            int hour = (DateTime.Now.Hour + 20) % 24;
            string user = Context.User.Mention;

            if (Context.Message.Author.Username == "Del") {
                if (hour < 8) {
                    await ReplyAsync("Good Mornin-\nOh wait.");
                } else if (hour < 14) {
                    await ReplyAsync("Good Afternoo-\nOh wait.");
                } else {
                    await ReplyAsync("Good Evenin-\nOh wait.");
                }
            } else {
                if (hour < 8) {
                    await ReplyAsync("Good Morning, " + user + "-dono.");
                } else if (hour < 14) {
                    await ReplyAsync("Good Afternoon, " + user + "-dono.");
                } else {
                    await ReplyAsync("Good Evening, " + user + "-dono.");
                }
            }
        }

        [Command("say")]
        public async Task SayAsync([Remainder]string s = null) {
            int splitPos = s.IndexOf("||");
            if (splitPos > 0 && s.Length > splitPos + 2) {
                do {
                    Program.EnqueueMessage(Utilities.TrimSpaces(s.Substring(0, splitPos)), Context.Channel);
                    s = s.Substring(splitPos + 2);
                    splitPos = s.IndexOf("||");
                } while (splitPos > 0 && s.Length > splitPos + 2);

                Program.EnqueueMessage(Utilities.TrimSpaces(s), Context.Channel);
            } else {
                await ReplyAsync(s);
            }
        }

        [Command("repeat")]
        [Alias("rep")]
        public async Task RepeatAsync([Remainder]string s = null) {
            var messages = await Context.Channel.GetMessagesAsync(100).Flatten();
            int i = 0;
            int back = 0;

            if (s != null) {
                var repeatQueries = Utilities.ParamSplit(s);
                string repeats = "";

                foreach (var repeatQuery in repeatQueries) {
                    foreach (var message in messages) {
                        if (i > 0) {
                            if (!int.TryParse(repeatQuery, out back)) {
                                if (repeatQuery.Length <= message.Content.Length && repeatQuery == message.Content.Substring(0, repeatQuery.Length)) {
                                    repeats += message.Content + " ";
                                    break;
                                }
                            } else if (i == back) {
                                repeats += message.Content + " ";
                                break;
                            }
                        }
                        i++;
                    }
                    i = 0;
                }

                if (repeats == "") {
                    await ReplyAsync("My apologies. No message was found.");
                } else {
                    Program.EnqueueMessage(repeats, Context.Channel);
                }
            } else {
                foreach (var message in messages) {
                    if (i != 0) {
                        Program.EnqueueMessage(message.Content, Context.Channel);
                        return;
                    }
                    i++;
                }
            }
        }

        // Hidden command
        [Command("split")]
        public async Task SplitAsync([Remainder]string s) {
            List<string> words = Utilities.ParamSplit(s);

            foreach (string w in words) {
                await ReplyAsync("`" + w + "`");
            }
        }

        [Command("source")]
        public async Task SourceAsync() {
            await ReplyAsync("Here is a link to my source code:\n" +
                "https://github.com/Origamijr/DiscordBots");
        }

        [Command("verbatim")]
        public async Task VerbatimAsync() {
            if (StaticStates.verbatim = !StaticStates.verbatim) {
                await ReplyAsync("Verbatim mode on.");
            } else {
                await ReplyAsync(Context.User.Mention + "-dono, verbatim mode has been turned off.");
            }
        }

        [Command("help")]
        public async Task HelpAsync() {
            await ReplyAsync("```" +
                ">>ping         - pong\n" +
                ">>hello        - say hi to Del\n" +
                ">>say <phrase> - tell Del to say ANYTHING *cough* verbatim\n" +
                ">>stahp        - clear the message queue\n" +
                ">>source       - get a link to Del's source code\n" +
                "\n" +
                ">>remember <phrase>         - get Del to remember a phrase\n" +
                ">>rememberArr <p1> <p2> ... - get Del to remember multiple phrases\n" +
                ">>cookie                    - Del gives you a cookie\n" +
                "\n" +
                ">>rpg init <str> <vit> <int> <dex> - initialize an rpg profile\n" +
                ">>rpg stats                        - display your current stats\n" +
                ">>rpg attack <player>              - attacks the specified player\n" +
                "\n" +
                ">>cfg <V1 V2...> <T1 T2...> <v [P]->vt;...> <S> - create a context-free grammar\n" +
                ">>cfg.set <name>                                - remember the last created grammar\n" +
                ">>cfg.sim <steps> {name=last}                   - simulate a grammar for a number of steps\n```");
        }
    }
}
