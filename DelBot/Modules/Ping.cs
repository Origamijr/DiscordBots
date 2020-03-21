using DelBot.Databases;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DelBot.Modules {

    public class Ping : ModuleBase<SocketCommandContext> {

        [Command("ping")]
        public async Task PingAsync() {
            await Program.EnqueueMessage("pong", Context.Channel);
        }

        [Command("stahp")]
        public async Task StahpAsync() {
            Program.MessageQueue.Clear();
            await Program.EnqueueMessage("ok", Context.Channel);
        }

        [Command("hello")]
        public async Task HelloAsync() {
            int hour = (DateTime.Now.Hour + 20) % 24;
            string user = Context.User.Mention;

            if (Context.Message.Author.Username == "Del") {
                if (hour < 8) {
                    await Program.EnqueueMessage("Good Mornin-\nOh wait.", Context.Channel);
                } else if (hour < 14) {
                    await Program.EnqueueMessage("Good Afternoo-\nOh wait.", Context.Channel);
                } else {
                    await Program.EnqueueMessage("Good Evenin-\nOh wait.", Context.Channel);
                }
            } else {
                if (hour < 8) {
                    await Program.EnqueueMessage("Good Morning, " + user + "-dono.", Context.Channel);
                } else if (hour < 14) {
                    await Program.EnqueueMessage("Good Afternoon, " + user + "-dono.", Context.Channel);
                } else {
                    await Program.EnqueueMessage("Good Evening, " + user + "-dono.", Context.Channel);
                }
            }
        }

        [Command("say")]
        public async Task SayAsync([Remainder]string s = null) {
            string msg;
            int firstQuotePos = s.IndexOf("\"");
            int secondQuotePos = s.IndexOf("\"", (firstQuotePos < s.Length - 1) ? firstQuotePos + 1 : firstQuotePos);
            int splitPos = s.IndexOf("||");
            while (firstQuotePos >= 0 && secondQuotePos >= 0 && splitPos > firstQuotePos && splitPos < secondQuotePos) {
                splitPos = s.IndexOf("||", (splitPos < s.Length - 1) ? splitPos + 1 : splitPos);
            }
            if (splitPos > 0 && s.Length > splitPos + 2) {
                do {
                    msg = Utilities.TrimSpaces(s.Substring(0, splitPos));
                    if (msg.Length > 2 && msg[0] == '"' && msg[msg.Length - 1] == '"') msg = msg.Substring(1, msg.Length - 2);
                    await Program.EnqueueMessage(msg.Replace('\n', ' '), Context.Channel);
                    s = s.Substring(splitPos + 2);
                    firstQuotePos = s.IndexOf("\"");
                    secondQuotePos = s.IndexOf("\"", (firstQuotePos < s.Length - 1) ? firstQuotePos + 1 : firstQuotePos);
                    splitPos = s.IndexOf("||");
                    while (firstQuotePos >= 0 && secondQuotePos >= 0 && splitPos > firstQuotePos && splitPos < secondQuotePos) {
                        splitPos = s.IndexOf("||", (splitPos < s.Length - 1) ? splitPos + 1 : splitPos);
                    }
                } while (splitPos > 0 && s.Length > splitPos + 2);

                msg = Utilities.TrimSpaces(s);
                if (msg.Length > 2 && msg[0] == '"' && msg[msg.Length - 1] == '"') msg = msg.Substring(1, msg.Length - 2);
                await Program.EnqueueMessage(msg.Replace('\n', ' '), Context.Channel);
            } else {
                msg = Utilities.TrimSpaces(s);
                if (msg.Length > 2 && msg[0] == '"' && msg[msg.Length - 1] == '"') msg = msg.Substring(1, msg.Length - 2);
                await Program.EnqueueMessage(msg.Replace('\n', ' '), Context.Channel);
            }
        }

        [Command("repeat")]
        [Alias("rep")]
        public async Task RepeatAsync([Remainder]string s = null) {
            var messages = await Context.Channel.GetMessagesAsync(100).FlattenAsync();
            int i = 0;
            int back = 0;
            bool goingBack = false;

            if (s != null) {
                var repeatQueries = Utilities.ParamSplit(s);
                string repeats = "";

                foreach (var repeatQuery in repeatQueries) {
                    if (repeatQuery.Length > 2 && "->" == repeatQuery.Substring(0, 2) && int.TryParse(repeatQuery.Substring(2), out back)) {
                        goingBack = true;
                    }
                    foreach (var message in messages) {
                        if (i > 0) {
                            if (goingBack && back == i) {
                                repeats += message.Content + " ";
                                break;
                            } else if (message.Content.Contains(repeatQuery)) {
                                repeats += message.Content + " ";
                                break;
                            }
                        }
                        i++;
                    }
                    i = 0;
                    goingBack = false;
                }

                if (repeats == "") {
                    await Program.EnqueueMessage("My apologies. No message was found.", Context.Channel);
                } else {
                    await Program.EnqueueMessage(repeats, Context.Channel);
                }
            } else {
                foreach (var message in messages) {
                    if (i != 0) {
                        await Program.EnqueueMessage(message.Content, Context.Channel);
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
                await Program.EnqueueMessage("`" + w + "`", Context.Channel);
            }
        }

        [Command("source")]
        public async Task SourceAsync() {
            await Program.EnqueueMessage("Here is a link to my source code:\n" +
                "https://github.com/Origamijr/DiscordBots", Context.Channel);
        }

        [Command("verbatim")]
        public async Task VerbatimAsync() {
            if (StaticStates.verbatim = !StaticStates.verbatim) {
                await Program.EnqueueMessage("Verbatim mode on.", Context.Channel);
            } else {
                await Program.EnqueueMessage(Context.User.Mention + "-dono, verbatim mode has been turned off.", Context.Channel);
            }
        }

        [Command("help")]
        public async Task HelpAsync([Remainder]string s = null) {
            if (s == null) {
                await Program.EnqueueMessage("```" +
                    ">>ping                 - pong\n" +
                    ">>hello                - say hi to Del\n" +
                    ">>say <phrase>         - tell Del to say ANYTHING *cough* verbatim\n" +
                    ">>repeat <p1> <p2> ... - Get Del to repeat a message containg the given keywords *CAUTION*\n" +
                    ">>stahp                - clear the message queue\n" +
                    ">>source               - get a link to Del's source code\n" +
                    "\n" +
                    ">>remember <phrase>              - get Del to remember a phrase\n" +
                    ">>remember <varname> := <phrase> - get Del to remember a phrase with a keyword\n" +
                    ">>remember <varname>??           - get Del to recall a phrase with a keyword\n" +
                    ">>rememberArr <p1> <p2> ...      - get Del to remember multiple phrases\n" +
                    ">>invade <@user>                 - Del invades another user's wordbank" +
                    ">>cookie                         - Del gives you a cookie\n" +
                    "\n" +
                    ">>verbatim                                  - Toggle verbatim mode\n" +
                    ">>if <e1> <op> <e2> then <if> [else <else>] - Del will conditionally say something\n" +
                    ">>evaluate <integer expression>             - evaluate the given expression\n" +
                    "\n" +
                    "Modules: (use >>help \"[module name]\"\n" +
                    "\trpg    - WIP rpg mechanics\n" +
                    "\tcfg    - PCFG generator\n" +
                    "\tpoker5 - 5-draw poker\n```", Context.Channel);
            } else {
                var args = Utilities.ParamSplit(s);
                if (args.Count == 1) {
                    switch (args[0]) {
                        case "rpg":
                            await Program.EnqueueMessage("```" +
                                ">>rpg init <str> <vit> <int> <dex> - initialize an rpg profile\n" +
                                ">>rpg stats                        - display your current stats\n" +
                                ">>rpg attack <player>              - attacks the specified player\n```", Context.Channel);
                            break;

                        case "cfg":
                            await Program.EnqueueMessage("```" +
                                 ">>cfg <V1 V2...> <T1 T2...> <v [P]->vt;...> <S> - create a context-free grammar\n" +
                                 ">>cfg.set <name>                                - remember the last created grammar\n" +
                                 ">>cfg.sim <steps> {name=last}                   - simulate a grammar for a number of steps\n```", Context.Channel);
                            break;

                        case "poker5":
                            await Program.EnqueueMessage("```" +
                                 ">>poker5.create                 - Create a game.\n" +
                                 ">>poker5.join <game ID>         - Join a game via ID (Init Phase)\n" +
                                 ">>poker5.start                  - Start the game\n" +
                                 ">>poker5.check                  - Pass bet without action [Abbr. >>p.ch]\n" +
                                 ">>poker5.raise <amount>         - Raise the current bet [Abbr. >>p.r]\n" +
                                 ">>poker5.call                   - Match the current bet [Abbr. >>p.ca]\n" +
                                 ">>poker5.fold                   - Cowardice [Abbr. >>p.f]\n" +
                                 ">>poker5.exchange [i1] [i2] ... - Exchange cards at indices i1, i2, ... [Abbr. >>p.e]\n" +
                                 ">>poker5.terminate              - Terminate current game\n```", Context.Channel);
                            break;
                    }
                }
            }
        }
    }
}
