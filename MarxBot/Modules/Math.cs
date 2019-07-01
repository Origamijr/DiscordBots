using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MarxBot.Modules {
    public class Math : ModuleBase<SocketCommandContext> {


        // Static variables for looped commands, but I don't like using them
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
            if (n == 0) {

                await ReplyAsync("Result: 1");

            } else if (n <= 1) {

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

        [Command("invSqrt")]
        public async Task InvSqrt(float x) {
            float xhalf = 0.5f * x;
            int i = BitConverter.ToInt32(BitConverter.GetBytes(x), 0);
            i = 0x5f3759df - (i >> 1);
            x = BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
            x = x * (1.5f - xhalf * x * x);
            await ReplyAsync("Estimate: " + x);
        }
    }
}
