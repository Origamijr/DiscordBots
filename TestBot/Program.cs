/**
 * File: Program.cs
 * Author: Kevin Huang
 * Date: 12-19-2017
 * Sources of Help: https://www.youtube.com/watch?v=kCi_3nAAk9g
 * 
 * Program to setup and run bot. Completely taken from Coding with Storm's youtube tutorial
 */

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DelBot {
    class Program {

        /**
         * Main method
         */
        static void Main(string[] args) {
            new Program().RunBotAsync().GetAwaiter().GetResult();
        }

        // Private variables holding discord objects
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        public async Task RunBotAsync() {
            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string botToken = File.ReadAllLines("../../../Tokens.txt")[2];

            // event subscriptions
            _client.Log += Log;
            
            await _client.SetGameAsync("//source");

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, botToken);

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Log(LogMessage arg) {
            Console.WriteLine(arg);

            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync() {
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage arg) {
            var message = arg as SocketUserMessage;

            if (message is null)
                return;

            int argPos = 0;

            if (!message.Author.IsBot) {
                string content = message.Content;
                bool specialInput = false;

                // Detect the only important message which merits a response
                bool daFace = false;
                if (content.Length > 2 && content[0] == '>' && content[content.Length - 1] == '>') {
                    daFace = true;
                    for (int i = 1; i < content.Length - 2; i++) {
                        if (content[i] != '_') {
                            daFace = false;
                            break;
                        }
                    }
                }

                if (daFace) {
                    specialInput = true;
                    string maFace = "<";
                    for (int i = 1; i < content.Length - 2; i++) {
                        maFace += "\\_";
                    }
                    maFace += "<";
                    await message.Channel.SendMessageAsync(maFace);
                }

                //if (Regex.Match(content, @".* drop .* f .* chat .*").Success) {
                if (content.ToLower().Contains("drop") && content.ToLower().Contains("f") && content.ToLower().Contains("chat") && content.ToLower().IndexOf("drop") < content.ToLower().IndexOf("f") && content.ToLower().IndexOf("f") < content.ToLower().IndexOf("chat")) {
                    await message.Channel.SendMessageAsync("F");
                    specialInput = true;
                }

                if (!specialInput && message.Channel.Name == "general") {
                    Random random = new Random();
                    double r = random.NextDouble();

                    if (r < 0.00001) {
                        await message.Channel.SendMessageAsync("You have gotten the rare message. Have a cookie :cookie:");
                    } else if (r < 0.01) {
                        await message.Channel.SendMessageAsync("Hello. I exist to say I exist.");
                    }
                }
            }


            if (message.HasStringPrefix("//", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) {
                var context = new SocketCommandContext(_client, message);

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess) {
                    Console.WriteLine(result.ErrorReason);
                }
            }
        }
    }
}
