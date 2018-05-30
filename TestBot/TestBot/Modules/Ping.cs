using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestBot.Modules {
    public class Ping : ModuleBase<SocketCommandContext> {
        [Command("say")]
        public async Task SayAsync([Remainder]string s) {
            await ReplyAsync(s);
        }
    }
}
