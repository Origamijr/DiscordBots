using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DelBot.CFGUtils;
using DelBot.Databases;
using Discord.Commands;

namespace DelBot.Modules {
    public class NLPCommands : ModuleBase<SocketCommandContext> {
        
        string dbName = "Databases/Grammars.json";
        string lastTag = "ShortTermCFGMemory";
        string variableTag = "V";
        string terminalTag = "T";
        string ruleTag = "R";
        string startTag = "S";

        [Command("cfg")]
        public async Task CreateCFGAsync(string varStr, string termStr, string ruleStr, string sttStr, int steps = 0) {

            var variables = new List<string>(varStr.Split(" "));
            var terminals = new List<string>(termStr.Split(" "));
            var rules = ruleStr.Split(";");

            CFG g = CFG.MakeCFG(variables, terminals, rules, sttStr);
            if (g != null) {
                await ReplyAsync("```" + g.ToString() + "```");

                // Write to short term memory
                UserDatabase.WriteArray(dbName, new List<string> { lastTag, variableTag }, g.GetVariables());
                UserDatabase.WriteArray(dbName, new List<string> { lastTag, terminalTag }, g.GetTerminals());
                UserDatabase.WriteArray(dbName, new List<string> { lastTag, ruleTag }, g.GetRules());
                UserDatabase.WriteString(dbName, new List<string> { lastTag, startTag }, g.GetStart());
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
