using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MarxBot.Modules {
    public class Math : ModuleBase<SocketCommandContext> {

        private static bool looping = false;

        private static int tempInt = 0;

        [Command("triangle")]
        public async Task TriangleAsync(int n) {
            if (n <= 1) {
                
                if (looping) {
                    tempInt += n;
                    looping = false;
                } else {
                    tempInt = n;
                }

                await ReplyAsync("Result: " + tempInt);

            } else {

                if (looping) {
                    tempInt += n;
                } else {
                    tempInt = n;
                    looping = true;
                }

                await ReplyAsync("Partial Sum: " + tempInt);
                await ReplyAsync("$$triangle " + (n - 1));
            }
        }

        [Command("factorial")]
        public async Task FactorialAsync(int n) {
            if (n <= 1) {

                if (looping) {
                    looping = false;
                } else {
                    tempInt = n;
                }

                await ReplyAsync("Result: " + tempInt);

            } else {

                if (looping) {
                    tempInt *= n;
                } else {
                    tempInt = n;
                    looping = true;
                }

                await ReplyAsync("Partial Product: " + tempInt);
                await ReplyAsync("$$factorial " + (n - 1));
            }
        }
    }
}
