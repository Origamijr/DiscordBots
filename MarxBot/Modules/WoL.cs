using System;
using Discord.Commands;
using System.Threading.Tasks;

namespace MarxBot.Modules {
    public class WoL : ModuleBase<SocketCommandContext> {
        [Command("phone-home")]
        public async Task WoLAsync() {
            if (Context.User.Id == 236746009688932354) {
                ("sudo etherwake -i eth0 FC:34:97:A1:5B:8F").Bash();
                await ReplyAsync($"Home base has been contacted.");
            } else {
                await ReplyAsync($"You are not E.T.");
            }
        }
    }
}
