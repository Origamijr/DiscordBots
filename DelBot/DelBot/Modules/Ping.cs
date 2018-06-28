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
        public async Task SayAsync([Remainder]string s) {
            await ReplyAsync(s);
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

        [Command("r")]
        public async Task SourceAsync(string prompt = "1d20+0") {
            //if (Context.Channel.Name == "dice-rolls") {

            string user = Context.User.Mention;
            try {
                int dIndex = prompt.IndexOf('d');
                int plusIndex = prompt.IndexOf('+');
                int mult = 1;
                int dice = 20;
                int plus = 0;

                if (dIndex != 0) {
                    mult = int.Parse(prompt.Substring(0, dIndex));
                }

                if (plusIndex != -1) {
                    dice = int.Parse(prompt.Substring(dIndex, plusIndex - dIndex));
                    plus = int.Parse(prompt.Substring(plusIndex));
                } else {
                    dice = int.Parse(prompt.Substring(1));
                }

                if (dice != 4 && dice != 6 && dice != 10 && dice != 8 && dice != 12 && dice != 20) {
                    await ReplyAsync("That's not a dice silly");
                    return;
                }

                string ret = "";
                Random random = new Random();
                int sum = 0;

                if (mult == 1) {
                    int r = random.Next() % dice + 1;
                    sum += r;
                    ret += "" + r;
                } else {
                    for (int i = 0; i < mult; i++) {
                        int r = random.Next() % dice + 1;
                        sum += r;
                        if (i == 0) {
                            ret += "[" + r;
                        } else if (i == mult - 1) {
                            ret += " + " + r + "]";
                        } else {
                            ret += " + " + r;
                        }
                    }
                }

                if (plusIndex != -1) {
                    ret += " + " + plus;
                }
                await ReplyAsync(user + ": " + ret);
            } catch (Exception e) {
                await ReplyAsync("Learn to type noob.");
            }
            //}
        }
    }
}
