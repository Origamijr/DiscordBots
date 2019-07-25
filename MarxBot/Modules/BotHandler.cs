using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;

namespace MarxBot.Modules {
    public class BotHandler : ModuleBase<SocketCommandContext> {
        [Command("bash")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task BashAsync([Remainder]string s) {
            if (!Context.User.IsBot) {
                await ReplyAsync(s.Bash());
            }
        }

        private const long Alumina = 236746009688932354;
        private const long Carnegie = 236949806386380801;
        private readonly Dictionary<string, ulong[]> permissions = new Dictionary<string, ulong[]> {
            {"delbot", new ulong[] { Alumina } },
            {"testbot", new ulong[] { Alumina } },
            {"nablabot", new ulong[] { Alumina } },
            {"shit-chan", new ulong[] { Carnegie } }
        };

        private readonly Dictionary<string, string> executablePath = new Dictionary<string, string> {
            {"delbot", "dotnet DelBot/DelBot.dll"},
            {"testbot", "dotnet TestBot/TestBot.dll"},
            {"nablabot", "python NablaBot/NablaBot.py"}
        };

        private readonly Dictionary<string, string> repositoryPath = new Dictionary<string, string> {
            {"delbot", "."},
            {"testbot", "."},
            {"nablabot", "."}
        };

        [Command("kill")]
        public async Task KillAsync([Remainder]string s) {
            if (permissions.ContainsKey(s.ToLower()) && permissions[s.ToLower()].Contains(Context.User.Id)) {
                string pid = ("screen -ls | grep " + s.ToLower() + " | cut -d. -f1 | awk '{print $1}'").Bash();
                if (Regex.IsMatch(pid, @"^\d+$")) {
                    ("kill " + pid).Bash();
                    await ReplyAsync($"Killed {s}");
                } else {
                    await ReplyAsync($"{s} is already dead.");
                }
            } else {
                await ReplyAsync($"{s} is not a valid target.");
            }
        }

        [Command("update")]
        public async Task UpdateAsync([Remainder]string s) {
            if (permissions.ContainsKey(s.ToLower()) && permissions[s.ToLower()].Contains(Context.User.Id)) {
                await ReplyAsync(($"git -C {repositoryPath[s.ToLower()]} pull").Bash());
            } else {
                await ReplyAsync($"{s} is not a valid target.");
            }
        }

        [Command("start")]
        public async Task StartAsync([Remainder]string s) {
            if (permissions.ContainsKey(s.ToLower()) && permissions[s.ToLower()].Contains(Context.User.Id)) {
                string pid = ("screen -ls | grep " + s.ToLower() + " | cut -d. -f1 | awk '{print $1}'").Bash();
                if (Regex.IsMatch(pid, @"^\d+$")) {
                    await ReplyAsync($"{s} is still alive.");
                } else {
                    ($"screen -dmS {s.ToLower()} {executablePath[s.ToLower()]}").Bash();
                    await ReplyAsync($"{s} should (hopefully) be alive.");
                }
            } else {
                await ReplyAsync($"{s} is not a valid target.");
            }
        }
    }
}
