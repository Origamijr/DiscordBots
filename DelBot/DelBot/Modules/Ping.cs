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
                await ReplyAsync("Good Morning, " + user + "-sama.");
            } else if (hour < 14) {
                await ReplyAsync("Good Afternoon, " + user + "-sama.");
            } else {
                await ReplyAsync("Good Evening, " + user + "-sama.");
            }
        }

    }
}
