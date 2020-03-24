using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DelBot.Poker;
using Discord.Commands;
using Discord;
using System.Linq;

namespace DelBot.Modules {
    public class PokerCommands : ModuleBase<SocketCommandContext> {

        private static List<PokerGame> games = new List<PokerGame>();
        private static Dictionary<Discord.WebSocket.SocketUser, Tuple<int, int>> user2game = new Dictionary<Discord.WebSocket.SocketUser, Tuple<int, int>>();
        private static List<List<Discord.WebSocket.SocketUser>> game2users = new List<List<Discord.WebSocket.SocketUser>>();
        private static List<Discord.WebSocket.SocketGuildChannel> game2host = new List<Discord.WebSocket.SocketGuildChannel>();

        [Command("poker5.create")]
        [Alias("p5.create", "poker.create", "p.create")]
        public async Task CreatePokerGameAsync([Remainder]string s = null) {
            int gameId = 0;
            for (gameId = 0; gameId < games.Count; gameId++) {
                if (games[gameId] == null) break;
            }
            if (gameId == games.Count) {
                games.Add(null);
                game2users.Add(new List<Discord.WebSocket.SocketUser>());
                game2host.Add(null);
            }
            games[gameId] = new PokerGame();

            if (Context.Channel is Discord.WebSocket.SocketGuildChannel) {
                game2host[gameId] = (Discord.WebSocket.SocketGuildChannel)Context.Channel;
            }

            await Program.EnqueueMessage($"Successfully created a 5-draw Poker Game. Players can join the game with the following command\n```>>poker5.join {gameId}```\nWhen all players are ready, start the game with\n```>>poker5.start```", Context.Channel);

            await AddPlayer(gameId, Context.User, Context.Channel);
        }

        [Command("poker5.join")]
        [Alias("p5.join", "poker.join", "p.join")]
        public async Task JoinPokerGameAsync([Remainder]string s = null) {
            int gameId;
            var args = Utilities.ParamSplit(s);
            if (args != null && args.Count > 0 && int.TryParse(args[0], out gameId) && gameId < games.Count && games[gameId] != null) {
                await AddPlayer(gameId, Context.User, Context.Channel);
            } else {
                await Program.EnqueueMessage("Please specify a valid game ID.", Context.Channel);
            }
        }

        [Command("poker5.start")]
        [Alias("p5.start", "poker.start", "p.start")]
        public async Task StartPokerGameAsync([Remainder]string s = null) {
            int gameId = user2game[Context.User].Item1;
            if (games[gameId].StartGame()) {
                await SendAllMessage(gameId, $"Game has started!\n{GetStandings(gameId)}");
                foreach (var user in game2users[gameId]) {
                    await SendHand(user);
                }
                int currentPlayer = games[gameId].GetCurrentPlayer();
                await SendAllMessage(gameId, $"**Betting Round 1 has started.** It is {game2users[gameId][currentPlayer].Username}'s turn.");
            } else {
                await SendAllMessage(gameId, $"Game cannot be started.");
            }
        }

        [Command("poker5.terminate")]
        [Alias("p5.terminate", "poker.terminate", "p.terminate")]
        public async Task TerminatePokerGameAsync([Remainder]string s = null) {
            int gameId = user2game[Context.User].Item1;
            if (game2users[gameId][0].Id == Context.User.Id) {
                await SendAllMessage(gameId, $"Terminating game.");
                CleanGame(gameId);
            } else {
                await SendAllMessage(gameId, $"{Context.User.Username} just tried to end the game.");
            }
        }

        [Command("poker5.call")]
        [Alias("p5.call", "poker.call", "p.call", "p.ca")]
        public async Task PokerCallAsync([Remainder]string s = null) {
            int gameId = user2game[Context.User].Item1;
            int userId = user2game[Context.User].Item2;
            if (games[gameId].BettingAction(userId, PokerGame.PlayerAction.CALL, 0)) {
                await SendAllMessage(gameId, $"{Context.User.Username} called.\n{GetStandings(gameId)}");
                await ResolveBet(gameId);
            } else {
                await SendDM(Context.User, "Nope");
            }
        }

        [Command("poker5.fold")]
        [Alias("p5.fold", "poker.fold", "p.fold", "p.f")]
        public async Task PokerFoldAsync([Remainder]string s = null) {
            int gameId = user2game[Context.User].Item1;
            int userId = user2game[Context.User].Item2;
            if (games[gameId].BettingAction(userId, PokerGame.PlayerAction.FOLD, 0)) {
                await SendAllMessage(gameId, $"{Context.User.Username} folded.");
                await ResolveBet(gameId);
            } else {
                await SendDM(Context.User, "Nope");
            }
        }

        [Command("poker5.check")]
        [Alias("p5.check", "poker.check", "p.check", "p.ch")]
        public async Task PokerCheckAsync([Remainder]string s = null) {
            int gameId = user2game[Context.User].Item1;
            int userId = user2game[Context.User].Item2;
            if (games[gameId].BettingAction(userId, PokerGame.PlayerAction.CHECK, 0)) {
                await SendAllMessage(gameId, $"{Context.User.Username} checked.");
                await ResolveBet(gameId);
            } else {
                await SendDM(Context.User, "Nope");
            }
        }

        [Command("poker5.raise")]
        [Alias("p5.raise", "poker.rasie", "p.raise", "p.r")]
        public async Task PokerRaiseAsync([Remainder]string s = null) {
            int gameId = user2game[Context.User].Item1;
            int userId = user2game[Context.User].Item2;
            int raise;
            var args = Utilities.ParamSplit(s);
            if (args != null && args.Count > 0 && int.TryParse(args[0], out raise)) {
                if (games[gameId].BettingAction(userId, PokerGame.PlayerAction.RAISE, raise)) {
                    await SendAllMessage(gameId, $"{Context.User.Username} raised by {raise}.\n{GetStandings(gameId)}");
                    await ResolveBet(gameId);
                } else {
                    await SendDM(Context.User, "Nope");
                }
            } else {
                await SendDM(Context.User, "Invalid Raise");
            }
        }

        [Command("poker5.exchange")]
        [Alias("p5.exchange", "poker.exchange", "p.exchange", "p.e")]
        public async Task PokerExchangeAsync([Remainder]string s = null) {
            int gameId = user2game[Context.User].Item1;
            int userId = user2game[Context.User].Item2;
            List<int> indices = new List<int>();
            bool valid = false;
            foreach (string i in Utilities.ParamSplit(s)) {
                int index;
                if (int.TryParse(i, out index) && index >= 0 && index <= 5 && !indices.Contains(index - 1)) {
                    indices.Add(index - 1);
                    valid = true;
                } else {
                    await SendDM(Context.User, "Baka >.<");
                    valid = false;
                    break;
                }
            }
            if (valid) {
                if (games[gameId].ExchangeAction(userId, indices)) {
                    await SendAllMessage(gameId, $"{Context.User.Username} exchanged {indices.Count} card(s).");
                    await SendHand(Context.User);
                    await ResolveExchange(gameId);
                } else {
                    await SendDM(Context.User, "Nope");
                }
            } else if (s == null) {
                if (games[gameId].ExchangeAction(userId, indices)) {
                    await SendAllMessage(gameId, $"{Context.User.Username} hodled.");
                    await SendHand(Context.User);
                    await ResolveExchange(gameId);
                } else {
                    await SendDM(Context.User, "Nope");
                }
            }
        }

        [Command("poker5.hand")]
        [Alias("p5.hand", "poker.hand", "p.hand", "p.h")]
        public async Task PokerShowHandAsync([Remainder]string s = null) {
            int gameId = user2game[Context.User].Item1;
            int userId = user2game[Context.User].Item2;
            var hand = games[gameId].GetHand(userId).GetEnumeratedString();
            await SendDM(Context.User, hand);
        }

        public async Task AddPlayer(int gameId, Discord.WebSocket.SocketUser user, Discord.WebSocket.ISocketMessageChannel channel) {
            if (!user.IsBot) {
                if (!user2game.ContainsKey(user)) {
                    if (games[gameId].AddPlayer()) {
                        user2game.Add(user, Tuple.Create(gameId, game2users[gameId].Count));
                        game2users[gameId].Add(user);
                        await user.SendMessageAsync("You have successfully joined a game. Your hands will be sent here.");
                        await SendAllMessage(gameId, $"{user.Username} has successfully joined the game. {game2users[gameId].Count} player(s) ready.");
                    } else {
                        await Program.EnqueueMessage($"Game already in session.", channel);
                    }
                }
            } else {
                await Program.EnqueueMessage($"Sorry. No humans only, {user.Username}.", channel);
            }
        }

        public async Task SendAllMessage(int gameId, string message) {
            if (game2host[gameId] != null) {
                await Program.EnqueueMessage(message, (Discord.WebSocket.ISocketMessageChannel)game2host[gameId]);
            } else {
                foreach (var user in game2users[gameId]) {
                    await SendDM(user, message);
                }
            }
        }

        public async Task SendDM(Discord.WebSocket.SocketUser user, string message) {
            Discord.WebSocket.ISocketMessageChannel dm = (Discord.WebSocket.ISocketMessageChannel)(await user.GetOrCreateDMChannelAsync());
            await Program.EnqueueMessage(message, dm);
        }

        public string GetStandings(int gameId) {
            var standings = games[gameId].GetStandings();
            int pot = games[gameId].GetPot();
            string ret = "```\n";
            for (int i = 0; i < standings.Count; i++) {
                ret += $"{game2users[gameId][i].Username}: {standings[i]}\n";
            }
            ret += $"Current Pot: {pot}\n```";
            return ret;
        }

        public async Task SendHand(Discord.WebSocket.SocketUser user) {
            int gameId = user2game[user].Item1;
            int userId = user2game[user].Item2;
            var hand = games[gameId].GetHand(userId);
            await SendDM(user, hand.GetEnumeratedString());
        }

        public async Task ResolveBet(int gameId) {
            int currentPlayer = games[gameId].GetCurrentPlayer();
            int round = games[gameId].GetBettingRound();
            var hands = games[gameId].GetActiveHands();
            string results = "";
            switch (games[gameId].GetGameState()) {
                case PokerGame.GameState.BETTING:
                    await SendAllMessage(gameId, $"It is {game2users[gameId][currentPlayer].Username}'s turn.");
                    break;

                case PokerGame.GameState.EXCHANGE:
                    await SendAllMessage(gameId, $"**Beginning exchange round {round}.** It is {game2users[gameId][currentPlayer].Username}'s turn.");
                    break;

                case PokerGame.GameState.ROUND_END:
                    if (hands.Where(h => h != null).ToList().Count > 1) {
                        results = "```\n";
                        for (int p = 0; p < hands.Count; p++) {
                            if (hands[p] != null)
                                results += $"{game2users[gameId][p].Username}: {hands[p]} {hands[p].handRank}\n";
                        }
                        results += $"```\n{game2users[gameId][games[gameId].GetWinningPlayer()].Username} has won the pot.\n";
                    }
                    results += $"{GetStandings(gameId)}";
                    await SendAllMessage(gameId, results);

                    await SendAllMessage(gameId, $"\n\n\n**Beginning a new round.**");
                    games[gameId].InitRound();
                    round = games[gameId].GetBettingRound();
                    currentPlayer = games[gameId].GetCurrentPlayer();
                    foreach (var user in game2users[gameId]) {
                        await SendHand(user);
                    }
                    await SendAllMessage(gameId, $"**Beginning betting round {round}.**\n{GetStandings(gameId)}\n It is {game2users[gameId][currentPlayer].Username}'s turn.");
                    break;

                case PokerGame.GameState.GAME_END:
                    if (hands.Where(h => h != null).ToList().Count > 1) {
                        results = "```\n";
                        for (int p = 0; p < hands.Count; p++) {
                            if (hands[p] != null)
                                results += $"{game2users[gameId][p].Username}: {hands[p]} {hands[p].handRank}\n";
                        }
                        results += $"```\n{game2users[gameId][games[gameId].GetWinningPlayer()].Username} has won the pot.\n";
                    }
                    results += $"{GetStandings(gameId)}";
                    await SendAllMessage(gameId, results);

                    await SendAllMessage(gameId, $"{game2users[gameId][games[gameId].GetWinningPlayer()].Username} has won the game!\n");
                    CleanGame(gameId);
                    break;
            }
        }

        public void CleanGame(int gameId) {
            games[gameId] = null;
            game2host[gameId] = null;
            foreach (var user in game2users[gameId]) {
                user2game.Remove(user);
            }
            game2users[gameId] = new List<Discord.WebSocket.SocketUser>();
        }

        public async Task ResolveExchange(int gameId) {
            int currentPlayer = games[gameId].GetCurrentPlayer();
            int round = games[gameId].GetBettingRound();
            switch (games[gameId].GetGameState()) {
                case PokerGame.GameState.BETTING:
                    await SendAllMessage(gameId, $"**Beginning betting round {round}.**\n{GetStandings(gameId)}\n It is {game2users[gameId][currentPlayer].Username}'s turn.");
                    break;

                case PokerGame.GameState.EXCHANGE:
                    await SendAllMessage(gameId, $"It is {game2users[gameId][currentPlayer].Username}'s turn.");
                    break;
            }
        }
    }
}
