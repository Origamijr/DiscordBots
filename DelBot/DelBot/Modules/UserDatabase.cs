using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DelBot.Modules {

    public class UserDatabase : ModuleBase<SocketCommandContext> {

        [Command("remember")]
        public async Task RememberAsync() {

            JObject profiles;

            try {
                using (StreamReader sr = File.OpenText("Profiles.json")) {
                    profiles = (JObject) JToken.ReadFrom(new JsonTextReader(sr));
                }
            } catch {
                await ReplyAsync("Database not found. Creating new database...");

                profiles = new JObject();
            }
        }
    }

}
