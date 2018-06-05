using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DelBot.CFGUtils;
using DelBot.Databases;
using Discord.Commands;

namespace DelBot.Modules {
    public class NLPCommands : ModuleBase<SocketCommandContext> {
        
        string dbName = "CFG/Grammars.json";

        [Command("cfg")]
        public async Task CreateCFGAsync(string varStr, string termStr, string ruleStr, string sttStr, int steps = 0) {

            var variables = new List<string>(varStr.Split(" "));
            var terminals = new List<string>(termStr.Split(" "));
            var rules = ruleStr.Split(";");

            CFG g = CFG.MakeCFG(variables, terminals, rules, sttStr);
            if (g != null) {
                await ReplyAsync("```" + g.ToString() + "```");
            } else {
                await ReplyAsync("Not a valid CFG");
            }

            if (steps != 0) {
                await ReplyAsync("Here's a sample expansion: ");
                var stepList = g.Simulate(steps);
                foreach (var step in stepList) {
                    Program.MessageQueue.Enqueue(Tuple.Create(step, Context.Channel));
                }
            }
        }
    }
}
