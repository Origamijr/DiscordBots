using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestBot.Modules {
    public class Ping : ModuleBase<SocketCommandContext> {
        [Command("hi")]
        public async Task PingAsync(string arg1 = null, string arg2 = null, string arg3 = null, string arg4 = null, string arg5 = null, 
            string arg6 = null, string arg7 = null, string arg8 = null, string arg9 = null, string arg10 = null, 
            string arg11 = null, string arg12 = null, string arg13 = null, string arg14 = null, string arg15 = null, 
            string arg16 = null, string arg17 = null, string arg18 = null, string arg19 = null, string arg20 = null) {


            await ReplyAsync("Hello. I exist to to say I exist.");
        }
    }
}
