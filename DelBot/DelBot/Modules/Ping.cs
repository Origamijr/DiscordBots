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

        [Command("hello")]
        public async Task HelloAsync() {
            int hour = (DateTime.Now.Hour + 20) % 24;
            string user = Context.User.Mention;

            if (hour < 8) {
                await ReplyAsync("Good Morning, " + user + "-dono.");
            } else if (hour < 14) {
                await ReplyAsync("Good Afternoon, " + user + "-dono.");
            } else {
                await ReplyAsync("Good Evening, " + user + "-dono.");
            }
        }

        [Command("say")]
        public async Task SayAsync([Remainder]string s) {
            await ReplyAsync(s);
        }

        [Command("split")]
        public async Task SplitAsync([Remainder]string s) {
            List<string> words = Utilities.ParamSplit(s);

            foreach (string w in words) {
                await ReplyAsync("`" + w + "`");
            }
        }
    }
}
