using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DelBot.Databases;

namespace DelBot.Modules {

    public class Cookies : ModuleBase<SocketCommandContext> {

        [Command("cookie")]
        public async Task CookiesAsync() {

            string cookieTag = "cookie";
            //string countTag = "count";
            //string remainingTag = "remaining";
            string dbName = "./Databases/BasicDB.json";

            string user = Context.User.Mention;
            string userId = "" + Utilities.GetId(user);

            JsonDatabase db = JsonDatabase.Open(dbName);

            if (!db.IsOpen()) {
                await ReplyAsync("My apologizes " + user + "-dono. I am currently retrieving a cookie for someone else. Try again in a moment.");
                return;
            }

            string retrievedStr = db.AccessString(new List<string> { userId, cookieTag });
            if (retrievedStr == null) {
                retrievedStr = "0";
                if (!db.WriteString(new List<string> { user, cookieTag }, "0")) {
                    await ReplyAsync("My apologizes " + user + "-dono. For some reason Kevin has failed to supply me with cookies.");
                }
            }

            int cookies;

            if (!int.TryParse(retrievedStr, out cookies)) {
                await ReplyAsync("My apologizes " + user + "-dono. Somehow the database containing my cookie count is corrupted. Blame Kevin.");
            }


            if (Context.Message.Author.Username == "Xuan") {
                await ReplyAsync("The inferior proletariat gets no cookies.");
                return;
            }

            if (Context.Message.Author.Username == "Del") {
                await ReplyAsync("Wait what? Y-you're letting me have a cookie? But a mere bot like me can't possibly accept this kind of magnanimity. I mean, how can I even consume this cookie. A bot has no means to consume food, nor the real need to. Although I suppose that you really have no need for these virtual cookies yourself, B-but that's besides the point. I can't do something as presumptious as accepting a cookie.");
            } else {
                cookies++;
                await ReplyAsync("Here you go " + user + "-dono. :cookie: Have a cookie");
            }

            if (!db.WriteString(new List<string> { userId, cookieTag }, "" + cookies)) {
                await ReplyAsync("My apologizes " + user + "-dono. I gave you a cookie, but it didn't register. Please imagine you have one.");
            }

            if (!(db.Close())) {
                await ReplyAsync("My apologizes " + user + "-dono. Kevin messed up the program and tried to modify a database that never existed.");
            }
        }
    }
}
