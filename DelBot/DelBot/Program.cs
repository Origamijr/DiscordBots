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
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.Generic;
using System.IO;

namespace DelBot {
    class Program {
        
        public static Queue<Tuple<string, ISocketMessageChannel>> MessageQueue = new Queue<Tuple<string, ISocketMessageChannel>>();
        static Timer timer;

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
            
            timer = new Timer();
            timer.Start();
            timer.Interval = 3000;
            timer.Elapsed += new ElapsedEventHandler(TimerTick);

            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string botToken = File.ReadAllLines("../../Tokens.txt")[0];

            // event subscriptions
            _client.Log += Log;
            await _client.SetGameAsync(">>help");

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

            if (message is null) //|| message.Author.IsBot)
                return;

            int argPos = 0;

            if (message.HasStringPrefix(">>", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) {

                var context = new SocketCommandContext(_client, message);

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess) {
                    Console.WriteLine(result.ErrorReason);
                }
            }
        }


        void TimerTick(object sender, ElapsedEventArgs e) {
            if (MessageQueue.Count != 0) {
                var message = MessageQueue.Dequeue();
                message.Item2.SendMessageAsync(message.Item1);
            }
        }
    }
}
