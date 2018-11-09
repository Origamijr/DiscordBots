using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DelBot.CFGUtils;
using DelBot.Databases;
using Discord.Commands;

namespace DelBot.Modules {
    public class NLPCommands : ModuleBase<SocketCommandContext> {

        const string dbName = "Databases/Grammars.json";
        const string lastTag = "ShortTermCFGMemory";
        const string variableTag = "V";
        const string terminalTag = "T";
        const string ruleTag = "R";
        const string startTag = "S";

        [Command("cfg")]
        public async Task CreateCFGAsync(string varStr, string termStr, string ruleStr, string sttStr) {

            var variables = new List<string>(varStr.Split(' '));
            var terminals = new List<string>(termStr.Split(' '));
            var rules = ruleStr.Split(';');

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
        }

        [Command("cfg.sim")]
        public async Task SimulateCFG(int steps, string cfgTag = lastTag) {

            // get grammar
            string[] variables = UserDatabase.ReadArray(dbName, new List<string> { cfgTag, variableTag });
            string[] terminals = UserDatabase.ReadArray(dbName, new List<string> { cfgTag, terminalTag });
            string[] rules = UserDatabase.ReadArray(dbName, new List<string> { cfgTag, ruleTag });
            string start = UserDatabase.ReadString(dbName, new List<string> { cfgTag, startTag });

            if (variables != null && terminals != null && rules != null && start != null) {
                CFG g = CFG.MakeCFG(new List<string>(variables), new List<string>(terminals), rules, start);
                await ReplyAsync("Here's a sample expansion: ");
                var stepList = g.Simulate(steps);
                foreach (var step in stepList) {
                    Program.MessageQueue.Enqueue(Tuple.Create(step, Context.Channel));
                }
            } else {
                await ReplyAsync("404 Grammar not found.");
            }
        }

        [Command("cfg.set")]
        public async Task SetCFG(string cfgTag) {

            // get grammar
            string[] variables = UserDatabase.ReadArray(dbName, new List<string> { lastTag, variableTag });
            string[] terminals = UserDatabase.ReadArray(dbName, new List<string> { lastTag, terminalTag });
            string[] rules = UserDatabase.ReadArray(dbName, new List<string> { lastTag, ruleTag });
            string start = UserDatabase.ReadString(dbName, new List<string> { lastTag, startTag });

            if (variables != null && terminals != null && rules != null && start != null) {
                UserDatabase.WriteArray(dbName, new List<string> { cfgTag, variableTag }, variables);
                UserDatabase.WriteArray(dbName, new List<string> { cfgTag, terminalTag }, terminals);
                UserDatabase.WriteArray(dbName, new List<string> { cfgTag, ruleTag }, rules);
                UserDatabase.WriteString(dbName, new List<string> { cfgTag, startTag }, start);
                await ReplyAsync("New grammar stored as \"" + cfgTag + "\"");
            } else {
                await ReplyAsync("Please enter a cfg to begin with.");
            }
        }
    }
}
