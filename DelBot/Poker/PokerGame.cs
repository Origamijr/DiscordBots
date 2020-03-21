using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DelBot.Poker {
    class PokerGame {
        public enum GameState {
            GAME_INIT,
            BETTING,
            EXCHANGE,
            ROUND_END,
            GAME_END
        }

        public enum PlayerAction {
            CHECK,
            RAISE,
            CALL,
            FOLD
        }
        
        List<PokerPlayer> players;
        PokerDeck deck;
        
        private int startingCredits;
        private int ante;

        private GameState gameState;
        private int currentStarter;
        private int currentPlayer;
        private int totalExchangeRounds = 2;
        private int currentExchangeRound;

        private int pot;
        private int currentBet;

        public PokerGame(int credits = 1000, int ante = 10) {
            this.startingCredits = credits;
            this.ante = ante;
            this.gameState = GameState.GAME_INIT;
            this.currentStarter = 0;
            this.players = new List<PokerPlayer>();
        }

        public bool AddPlayer() {
            if (this.gameState != GameState.GAME_INIT || this.players.Count == 4) return false;
            players.Add(new PokerPlayer(this.players.Count, this.startingCredits));
            return true;
        }

        public void RemovePlayer(int player) {
            this.players[player].Leave();
            if (this.gameState == GameState.BETTING) {
                this.BettingAction(player, PlayerAction.FOLD, 0);
            } else if (this.gameState == GameState.EXCHANGE) {
                this.ExchangeAction(player, new List<int>());
            }
        }

        public PokerHand GetHand(int player) {
            return this.players[player].GetHand();
        }

        public List<int> GetStandings() {
            return this.players.Select(player => player.GetCredits()).ToList();
        }

        public int GetPot() {
            return this.pot;
        }

        public PokerPlayer.PlayerState GetPlayerState(int player) {
            return this.players[player].playerState;
        }

        public GameState GetGameState() {
            return this.gameState;
        }

        public int GetBettingRound() {
            return this.currentExchangeRound + 1;
        }

        public List<PokerHand> GetActiveHands() {
            return this.players.Select(p => {
                if (p.playerState == PokerPlayer.PlayerState.OUT) {
                    return null;
                }
                return p.GetHand();
            }).ToList();
        }

        public bool StartGame() {
            if (this.gameState != GameState.GAME_INIT || this.players.Count < 2) return false;
            this.InitRound();
            return true;
        }

        public void InitRound() {
            this.deck = new PokerDeck();

            this.gameState = GameState.BETTING;
            this.pot = 0;
            this.currentBet = 0;
            foreach (var player in this.players) {
                int credits = player.GetCredits();
                if (credits > 0) {
                    int payment = Math.Min(this.ante, credits);
                    player.BetCredits(payment);
                    this.pot += payment;
                    if (player.GetCredits() == 0) {
                        player.playerState = PokerPlayer.PlayerState.ALL_IN;
                    } else {
                        player.playerState = PokerPlayer.PlayerState.DECIDING;
                    }
                    player.Deal(deck);
                    player.ResetContribution();
                } else {
                    player.playerState = PokerPlayer.PlayerState.OUT;
                }
            }
            this.currentPlayer = this.currentStarter;
            this.currentExchangeRound = 0;
        }

        public int GetCurrentPlayer() {
            return this.currentPlayer;
        }

        public bool BettingAction(int player, PlayerAction action, int raise) {
            //Console.WriteLine("==========================================Betting");
            if (this.currentPlayer != player || this.gameState != GameState.BETTING) return false;

            int difference = this.currentBet - this.players[player].GetContribution();
            //Console.WriteLine(difference);
            int payment;
            switch (action) {
                case PlayerAction.CHECK:
                    if (difference > 0) return false;
                    if (this.players[player].playerState == PokerPlayer.PlayerState.DECIDING)
                        this.players[player].playerState = PokerPlayer.PlayerState.IN;
                    break;

                case PlayerAction.RAISE:
                    payment = difference + raise;
                    if (payment > this.players[player].GetCredits()) return false;
                    this.players[player].BetCredits(payment);
                    this.pot += payment;
                    this.currentBet += raise;
                    if (this.players[player].GetCredits() == 0) {
                        this.players[player].playerState = PokerPlayer.PlayerState.ALL_IN;
                    } else {
                        this.players[player].playerState = PokerPlayer.PlayerState.IN;
                    }
                    for (int i = 0; i < this.players.Count; i++) {
                        if (i != player && this.players[i].playerState == PokerPlayer.PlayerState.IN) {
                            this.players[i].playerState = PokerPlayer.PlayerState.DECIDING;
                        }
                    }
                    break;

                case PlayerAction.CALL:
                    if (difference == 0) return false;
                    payment = Math.Min(difference, this.players[player].GetCredits());
                    this.players[player].BetCredits(payment);
                    this.pot += payment;
                    if (this.players[player].GetCredits() == 0) {
                        this.players[player].playerState = PokerPlayer.PlayerState.ALL_IN;
                    } else {
                        this.players[player].playerState = PokerPlayer.PlayerState.IN;
                    }
                    break;

                case PlayerAction.FOLD:
                    this.players[player].playerState = PokerPlayer.PlayerState.OUT;
                    int remainingPlayers = 0;
                    foreach (var p in this.players) {
                        if (p.playerState != PokerPlayer.PlayerState.OUT)
                            remainingPlayers++;
                    }
                    if (remainingPlayers == 1) {
                        this.ConcludeRound();
                        return true;
                    }
                    break;
            }

            do {
                this.currentPlayer = (this.currentPlayer + 1) % this.players.Count;
            } while (this.currentPlayer != player && 
                    this.players[this.currentPlayer].playerState != PokerPlayer.PlayerState.DECIDING);
            
            if (this.currentPlayer == player) {
                if (this.currentExchangeRound == this.totalExchangeRounds) {
                    this.ConcludeRound();
                    return true;
                }

                int remainingPlayers = 0;
                foreach (var p in this.players) {
                    if (p.playerState != PokerPlayer.PlayerState.OUT)
                        remainingPlayers++;
                }
                if (remainingPlayers > 1) {
                    this.gameState = GameState.EXCHANGE;
                    this.currentPlayer = this.currentStarter;
                } else {
                    this.ConcludeRound();
                    return true;
                }
            }
            
            return true;
        }

        public bool ExchangeAction(int player, List<int> indices) {
            if (this.currentPlayer != player || this.gameState != GameState.EXCHANGE) return false;

            if (indices.Count > this.deck.Size()) return false;
            foreach (int index in indices) {
                this.players[player].Exchange(index, this.deck);
            }
            this.players[player].playerState = PokerPlayer.PlayerState.SET;

            do {
                this.currentPlayer = (this.currentPlayer + 1) % this.players.Count;
            } while (this.currentPlayer != player && 
                    (this.players[this.currentPlayer].playerState != PokerPlayer.PlayerState.IN &&
                    this.players[this.currentPlayer].playerState != PokerPlayer.PlayerState.ALL_IN));

            if (this.currentPlayer == player) {
                this.currentExchangeRound++;
                this.gameState = GameState.BETTING;
                foreach (var p in this.players) {
                    if (p.playerState == PokerPlayer.PlayerState.SET) {
                        if (p.GetCredits() == 0) {
                            p.playerState = PokerPlayer.PlayerState.ALL_IN;
                        } else {
                            p.playerState = PokerPlayer.PlayerState.DECIDING;
                        }
                    }
                }
                this.currentPlayer = this.currentStarter;
            }

            return true;
        }

        public int GetWinningPlayer() {
            var showdown = this.players.Where(p => p.playerState != PokerPlayer.PlayerState.OUT).OrderByDescending(p => p.GetHand()).ToList();
            return showdown[0].GetId();
        }

        public void ConcludeRound() {
            int winningPlayer = this.GetWinningPlayer();
            this.players[winningPlayer].AddCredits(this.pot);

            int remainingPlayers = 0;
            foreach (var player in this.players) {
                if (player.GetCredits() > 0) {
                    remainingPlayers++;
                }
            }
            if (remainingPlayers > 1) {
                this.currentStarter = winningPlayer;
                this.gameState = GameState.ROUND_END;
            } else {
                this.gameState = GameState.GAME_END;
            }
        }

        public static void Test() {
            Random rng = new Random();
            List<int> standings = new List<int>();
            PokerGame poker = new PokerGame();
            poker.AddPlayer();
            poker.AddPlayer();
            poker.AddPlayer();
            int round = 0;
            if (poker.StartGame()) {
                while (poker.GetGameState() != PokerGame.GameState.GAME_END) {
                    Console.WriteLine($"============================================== ROUND {round++}");
                    if (round > 1) poker.InitRound();
                    Console.WriteLine($"Player 0: {poker.GetHand(0)}");
                    Console.WriteLine($"Player 1: {poker.GetHand(1)}");
                    Console.WriteLine($"======= BETTING 1");
                    while (poker.GetGameState() == PokerGame.GameState.BETTING) {
                        int player = rng.Next(3);
                        PokerGame.PlayerAction action = (PokerGame.PlayerAction)rng.Next(4);
                        //PokerGame.PlayerAction action = PokerGame.PlayerAction.CHECK;
                        int raise = rng.Next(1000);
                        if (poker.BettingAction(player, action, raise)) {
                            if (action == PokerGame.PlayerAction.RAISE) {
                                Console.WriteLine($"Player {player}: {action} {raise}");
                            } else {
                                Console.WriteLine($"Player {player}: {action}");
                            }
                        }
                    }
                    if (poker.GetGameState() != PokerGame.GameState.EXCHANGE) {
                        standings = poker.GetStandings();
                        Console.WriteLine($"Player 0: {standings[0]}");
                        Console.WriteLine($"Player 1: {standings[1]}");
                        continue;
                    }
                    Console.WriteLine($"======= EXCHANGE 1");
                    while (poker.GetGameState() == PokerGame.GameState.EXCHANGE) {
                        int player = rng.Next(3);
                        List<int> indices = new List<int>();
                        for (int i = 0; i < 5; i++) {
                            if (rng.Next(2) == 1)
                                indices.Add(i);
                        }
                        if (poker.ExchangeAction(player, indices))
                            Console.WriteLine($"Player {player}: {String.Join(" ", indices)} {poker.GetHand(player)}");
                    }
                    Console.WriteLine($"======= BETTING 2");
                    while (poker.GetGameState() == PokerGame.GameState.BETTING) {
                        int player = rng.Next(3);
                        PokerGame.PlayerAction action = (PokerGame.PlayerAction)rng.Next(4);
                        //PokerGame.PlayerAction action = PokerGame.PlayerAction.CHECK;
                        int raise = rng.Next(1000);
                        if (poker.BettingAction(player, action, raise)) {
                            if (action == PokerGame.PlayerAction.RAISE) {
                                Console.WriteLine($"Player {player}: {action} {raise}");
                            } else {
                                Console.WriteLine($"Player {player}: {action}");
                            }
                        }
                    }
                    if (poker.GetGameState() != PokerGame.GameState.EXCHANGE) {
                        standings = poker.GetStandings();
                        Console.WriteLine($"Player 0: {standings[0]}");
                        Console.WriteLine($"Player 1: {standings[1]}");
                        continue;
                    }
                    Console.WriteLine($"======= EXCHANGE 2");
                    while (poker.GetGameState() == PokerGame.GameState.EXCHANGE) {
                        int player = rng.Next(3);
                        List<int> indices = new List<int>();
                        for (int i = 0; i < 5; i++) {
                            if (rng.Next(2) == 1)
                                indices.Add(i);
                        }
                        if (poker.ExchangeAction(player, indices))
                            Console.WriteLine($"Player {player}: {String.Join(" ", indices)} {poker.GetHand(player)}");
                    }

                    Console.WriteLine($"======= BETTING 3");
                    while (poker.GetGameState() == PokerGame.GameState.BETTING) {
                        int player = rng.Next(3);
                        PokerGame.PlayerAction action = (PokerGame.PlayerAction)rng.Next(4);
                        //PokerGame.PlayerAction action = PokerGame.PlayerAction.CHECK;
                        int raise = rng.Next(1000);
                        if (poker.BettingAction(player, action, raise)) {
                            if (action == PokerGame.PlayerAction.RAISE) {
                                Console.WriteLine($"Player {player}: {action} {raise}");
                            } else {
                                Console.WriteLine($"Player {player}: {action}");
                            }
                        }
                    }
                    standings = poker.GetStandings();
                    Console.WriteLine($"Player 0: {poker.GetHand(0)} {poker.GetHand(0).handRank} {standings[0]}");
                    Console.WriteLine($"Player 1: {poker.GetHand(1)} {poker.GetHand(1).handRank} {standings[1]}");
                }
                standings = poker.GetStandings();
                Console.WriteLine($"================= END");
                Console.WriteLine($"Player 0: {standings[0]}");
                Console.WriteLine($"Player 1: {standings[1]}");
            } else {
                Console.WriteLine("Failed to start game");
            }
        }
    }
}
