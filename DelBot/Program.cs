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
using DelBot.HauteAuction;
using DelBot.Poker;

namespace DelBot {
    class Program {

        public static Dictionary<ISocketMessageChannel, Tuple<Queue<string>, Timer>> MessageQueues = new Dictionary<ISocketMessageChannel, Tuple<Queue<string>, Timer>>();
        public static Queue<Tuple<string, ISocketMessageChannel>> MessageQueue = new Queue<Tuple<string, ISocketMessageChannel>>();
        static Timer timer;
        static Timer pingKevin;
        static int kevinIsTrash = 0;
        static Random r = new Random();
        private static int msgTimerDelay = 3000;

        private SocketCommandContext mainContext = null;

        /**
         * Main method
         */
        static void Main(string[] args) {
            new Program().RunBotAsync().GetAwaiter().GetResult();
        }

        // Private variables holding discord objects
        private static DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        public async Task RunBotAsync() {

            //timer = new Timer();
            //timer.Start();
            //timer.Interval = msgTimerDelay;
            //timer.Elapsed += new ElapsedEventHandler(TimerTick);

            pingKevin = new Timer();
            pingKevin.Start();
            pingKevin.Interval = r.Next(1000 * 60 * 60 * 20, 1000 * 60 * 60 * 28);
            pingKevin.Elapsed += new ElapsedEventHandler(PingKevin);

            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string botToken = File.ReadAllLines("Tokens.txt")[0];

            // event subscriptions
            _client.Log += Log;
            await _client.SetGameAsync(">>help");

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, botToken);

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private void PingKevin(object sender, ElapsedEventArgs e) {
            pingKevin.Interval = r.Next(1000 * 60 * 60 * 20, 1000 * 60 * 60 * 28);
            if (mainContext != null) {
                if (kevinIsTrash == 2) {
                    mainContext.Channel.SendMessageAsync("@everyone Kevin is a trash human (or dead)");
                    kevinIsTrash = 3; 
                    _client.SetGameAsync("Kevin?");
                } else if (kevinIsTrash == 0) {
                    mainContext.Channel.SendMessageAsync("<@!236746009688932354> don't be a trash human");
                    kevinIsTrash = 1;
                    _client.SetGameAsync("Awaiting Kevin...");
                } else {
                    kevinIsTrash++;
                }
            }
        }

        private Task Log(LogMessage arg) {
            Console.WriteLine(arg);

            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync() {
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        private async Task HandleCommandAsync(SocketMessage arg) {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            if (mainContext is null)
                mainContext = context;

            if (message is null) //|| message.Author.IsBot)
                return;

            if (!message.Author.IsBot) {
                kevinIsTrash = 0;
                await _client.SetGameAsync(">>help");
            }

            //Console.WriteLine(message);

            int argPos = 0;

            if (message.HasStringPrefix(">>", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)) {

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess) {
                    Console.WriteLine(result.ErrorReason);
                }
            }
        }

        public static async Task EnqueueMessage_bak(string msg, ISocketMessageChannel msgChannel) {
            if (MessageQueue.Count == 0) {
                MessageQueue.Enqueue(Tuple.Create("", msgChannel));
                //Console.WriteLine(DateTime.Now.ToString("[h:mm:ss tt]") + " Send message: " + msg);
                timer.Stop();
                timer.Interval = msgTimerDelay / 2;
                timer.Start();
                await msgChannel.SendMessageAsync(msg);
            } else {
                MessageQueue.Enqueue(Tuple.Create(msg, msgChannel));
                //Console.WriteLine(DateTime.Now.ToString("[h:mm:ss tt]") + " Enqueue message: " + msg);
                await _client.SetGameAsync("with messages...");
            }
        }

        public static async Task EnqueueMessage(string msg, ISocketMessageChannel msgChannel) {
            if (MessageQueues.ContainsKey(msgChannel)) {
                MessageQueues[msgChannel].Item1.Enqueue(msg);
            } else {
                Queue<string> q = new Queue<string>();
                q.Enqueue("");
                Timer t = new Timer();
                t.Interval = msgTimerDelay / 2;
                t.Elapsed += new ElapsedEventHandler(TimerTick);
                t.Start();
                MessageQueues.Add(msgChannel, Tuple.Create(q, t));
                await msgChannel.SendMessageAsync(msg);
                //Console.WriteLine(DateTime.Now.ToString("[h:mm:ss tt]") + " Send message: " + msg);
            }
        }

        private void TimerTick2(object sender, ElapsedEventArgs e) {
            if (MessageQueue.Count != 0) {
                var message = MessageQueue.Dequeue();
                if (message.Item1 != "") {
                    timer.Interval = msgTimerDelay;
                    message.Item2.SendMessageAsync(message.Item1);
                    //Console.WriteLine(DateTime.Now.ToString("[h:mm:ss tt]") + " Send message: " + message.Item1);
                }
                if (MessageQueue.Count == 0) {
                    _client.SetGameAsync(">>help");
                }
            }
        }

        private static void TimerTick(object sender, ElapsedEventArgs e) {
            var channels = MessageQueues.Keys;
            foreach (var channel in channels) {
                var q = MessageQueues[channel].Item1;
                string message = q.Dequeue();
                if (message != "") {
                    MessageQueues[channel].Item2.Interval = msgTimerDelay;
                    channel.SendMessageAsync(message);
                    //Console.WriteLine(DateTime.Now.ToString("[h:mm:ss tt]") + " Send message: " + message);
                }
                if (q.Count == 0) {
                    MessageQueues[channel].Item2.Stop();
                    MessageQueues[channel].Item2.Dispose();
                    MessageQueues.Remove(channel);
                }
            }
        }
    }
}
