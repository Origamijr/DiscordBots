using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestBot.Modules {
    public class Ping : ModuleBase<SocketCommandContext> {
        [Command("say")]
        public async Task SayAsync([Remainder]string s) {
            await ReplyAsync("I got no strings");
        }

        [Command("source")]
        public async Task SourceAsync() {
            await ReplyAsync("source code desu:\n" +
                "https://github.com/Origamijr/DiscordBots");
        }

        [Command("roll")]
        public async Task SourceAsync(int dice, int plus = 0) {
            if (Context.Channel.Name == "dice-rolls") {
                if (dice != 4 && dice != 6 && dice != 10 && dice != 8 && dice != 12 && dice != 20) {
                    await ReplyAsync("That's not a dice silly");
                    return;
                }
                Random random = new Random();
                int num = random.Next() % dice + 1;
                await ReplyAsync("" + num + " + " + plus + " = " + (num + plus));
            }
        }

        [Command("r")]
        public async Task SourceAsync(string prompt = "1d20") {
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
                    dice = int.Parse(prompt.Substring(dIndex + 1, plusIndex - dIndex - 1));
                    plus = int.Parse(prompt.Substring(plusIndex + 1));
                } else {
                    dice = int.Parse(prompt.Substring(dIndex + 1));
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

                if (mult != 1 || plusIndex != -1) {
                    ret += " = " + (sum + plus);
                }

                await ReplyAsync(user + ": " + ret);
            } catch (Exception e) {
                await ReplyAsync("Learn to type noob.");
            }
            //}
        }
    }
}
