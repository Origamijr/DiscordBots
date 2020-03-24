using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DelBot.Poker {

    class PokerDeck {
        private List<PokerCard> cards;
        private Random rng = new Random();

        public PokerDeck() {
            cards = new List<PokerCard>();
            foreach (PokerSuit suit in Enum.GetValues(typeof(PokerSuit))) {
                foreach (PokerRank rank in Enum.GetValues(typeof(PokerRank))) {
                    cards.Add(new PokerCard(rank, suit));
                }
            }
            this.Shuffle();
        }

        public void Shuffle() {
            this.cards = this.cards.OrderBy(a => rng.Next()).ToList();
        }

        public PokerCard Draw() {
            PokerCard card = this.cards[0];
            this.cards.RemoveAt(0);
            return card;
        }

        public int Size() {
            return this.cards.Count;
        }
    }
}
