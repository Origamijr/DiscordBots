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
        private readonly Dictionary<string,ulong[]> permissions = new Dictionary<string, ulong[]> {
            {"DelBot", new ulong[] { Alumina } },
            {"TestBot", new ulong[] { Alumina } },
            {"NablaBot", new ulong[] { Alumina } }
        };

        [Command("kill")]
        public async Task KillAsync([Remainder]string s) {
            if (permissions.ContainsKey(s) && permissions[s].Contains(Context.User.Id)) {
                string pid = "screen -ls | grep DelBot | cut -d. -f1 | awk '{print $1}'".Bash();
                if (Regex.IsMatch(pid, @"^\d+$")) {
                    ("kill " + pid).Bash();
                    await ReplyAsync("Killed " + s);
                } else {
                    await ReplyAsync(s + " is already dead.");
                }
            } else {
                await ReplyAsync(s + " is not a valid target.");
            }
        }

        [Command("update")]
        public async Task UpdateAsync([Remainder]string s) {
            if (permissions.ContainsKey(s) && permissions[s].Contains(Context.User.Id)) {
                switch (s) {
                    case "DelBot":
                    case "TestBot":
                    case "NablaBot":
                        "git pull".Bash();
                        break;
                    default:
                        break;
                }
            } else {
                await ReplyAsync(s + " is not a valid target.");
            }
        }

        [Command("start")]
        public async Task StartAsync([Remainder]string s) {
            if (permissions.ContainsKey(s) && permissions[s].Contains(Context.User.Id)) {
                string pid = "screen -ls | grep DelBot | cut -d. -f1 | awk '{print $1}'".Bash();
                if (Regex.IsMatch(pid, @"^\d+$")) {
                    await ReplyAsync(s + " is still alive.");
                } else {
                    switch (s) {
                        case "DelBot":
                        case "TestBot":
                        case "NablaBot":
                            ($"screen -dmS {s} dotnet {s}/{s}.dll").Bash();
                            break;
                        default:
                            break;
                    }
                    await ReplyAsync(s + " should (hopefully) be alive.");
                }
            } else {
                await ReplyAsync(s + " is not a valid target.");
            }
        }
    }
}
