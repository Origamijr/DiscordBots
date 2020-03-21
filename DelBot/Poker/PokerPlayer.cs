using System;
using System.Collections.Generic;
using System.Text;

namespace DelBot.Poker {
    class PokerPlayer {
        public enum PlayerState {
            IN,
            OUT,
            DECIDING,
            ALL_IN,
            SET
        }

        private int id;
        private PokerHand hand;
        private int credits;
        private int contribution;
        public PlayerState playerState { get; set; }

        public PokerPlayer(int id, int credits) {
            this.id = id;
            this.credits = credits;
            this.contribution = 0;
        }

        public void Deal(PokerDeck deck) {
            hand = new PokerHand(deck);
        }

        public void Exchange(int index, PokerDeck deck) {
            hand.Exchange(index, deck);
        }

        public int GetId() {
            return this.id;
        }

        public PokerHand GetHand() {
            return hand;
        }

        public int GetCredits() {
            return this.credits;
        }

        public int GetContribution() {
            return this.contribution;
        }

        public void BetCredits(int c) {
            this.credits -= c;
            this.contribution += c;
        }

        public void AddCredits(int c) {
            this.credits += c;
        }

        public void Leave() {
            this.credits = 0;
            this.playerState = PlayerState.OUT;
        }

        public void ResetContribution() {
            this.contribution = 0;
        }
    }
}
