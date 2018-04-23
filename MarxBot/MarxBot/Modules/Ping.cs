using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MarxBot.Modules {
    public class Ping : ModuleBase<SocketCommandContext> {
        [Command("ping")]
        public async Task PingAsync() {
            await ReplyAsync("$$pong");
        }

        [Command("pong")]
        public async Task PongAsync() {
            await ReplyAsync("ping pong");
        }

        [Command("say")]
        public async Task SayAsync([Remainder]string s) {
            await ReplyAsync(s);
        }
    }
}
