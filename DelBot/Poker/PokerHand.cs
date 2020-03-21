using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DelBot.Poker {
    public enum PokerHandRank {
        HIGH_CARD = 0,
        PAIR,
        TWO_PAIR,
        THREE_KIND,
        STRAIGHT,
        FLUSH,
        FULL_HOUSE,
        FOUR_KIND,
        STRAIGHT_FLUSH
    }

    class PokerHand : IComparable<PokerHand> {
        public List<PokerCard> cards;
        
        public const int HAND_SIZE = 5;

        public PokerHandRank handRank;
        private PokerCard rep1, rep2;

        public PokerHand(PokerDeck deck) {
            cards = new List<PokerCard>();
            for (int i = 0; i < HAND_SIZE; i++) {
                cards.Add(deck.Draw());
            }
            this.UpdateScore();
        }

        public void Exchange(int index, PokerDeck deck) {
            this.cards[index] = deck.Draw();
            this.UpdateScore();
        }

        public override string ToString() {
            return String.Join(", ", this.cards);
        }

        public string GetEnumeratedString() {
            string ret = "```\n";
            for (int i = 0; i < HAND_SIZE; i++) {
                ret += $"{i + 1}. {this.cards[i]}\n";
            }
            ret += "\n```";
            return ret;
        }

        public int CompareTo(PokerHand other) {
            if (this.handRank == other.handRank) {
                if (this.handRank == PokerHandRank.TWO_PAIR) {
                    if (this.rep1.Rank == other.rep1.Rank) {
                        if (this.rep2.Rank == other.rep2.Rank) {
                            return this.rep1.CompareTo(other.rep1);
                        }
                        return this.rep2.Rank - other.rep2.Rank;
                    }
                    return this.rep1.Rank - other.rep1.Rank;
                } else {
                    return this.rep1.CompareTo(other.rep1);
                }
            }
            return this.handRank - other.handRank;
        }

        public void UpdateScore() {
            var cards = this.cards.OrderByDescending(c => ((int)c.Rank << 2) + (int)c.Suit).ToList();

            this.rep1 = cards[0];
            this.rep2 = cards[0];

            bool flush = true;
            for (int i = 1; i < HAND_SIZE; i++) {
                if (cards[i].Suit != this.rep1.Suit) 
                    flush = false;
            }

            bool straight = true;
            for (int i = 1; i < HAND_SIZE; i++) {
                if (cards[i].Rank != this.rep1.Rank - i)
                    straight = false;
            }

            bool quad = false;
            for (int i = 0; i < HAND_SIZE - 3; i++) {
                if (cards[i].Rank == cards[i + 1].Rank && cards[i].Rank == cards[i + 2].Rank && cards[i].Rank == cards[i + 3].Rank) {
                    quad = true;
                    this.rep1 = cards[i];
                }
            }

            bool triple = false;
            if (!quad) {
                for (int i = 0; i < HAND_SIZE - 2; i++) {
                    if (cards[i].Rank == cards[i + 1].Rank && cards[i].Rank == cards[i + 2].Rank) {
                        triple = true;
                        this.rep1 = cards[i];
                    }
                }
            }

            bool pair = false;
            if (!quad && !triple) {
                for (int i = 0; i < HAND_SIZE - 1; i++) {
                    if (cards[i].Rank == cards[i + 1].Rank) {
                        pair = true;
                        this.rep1 = cards[i];
                        break;
                    }
                }
            }

            bool pair2 = false;
            if (triple || pair) {
                for (int i = 0; i < HAND_SIZE - 1; i++) {
                    if (cards[i].Rank == cards[i + 1].Rank && cards[i].Rank != rep1.Rank) {
                        pair2 = true;
                        this.rep2 = cards[i];
                        break;
                    }
                }
            }
            
            if (straight && flush) {
                this.handRank = PokerHandRank.STRAIGHT_FLUSH;
            } else if (quad) {
                this.handRank = PokerHandRank.FOUR_KIND;
            } else if (triple && pair2) {
                this.handRank = PokerHandRank.FULL_HOUSE;
            } else if (flush) {
                this.handRank = PokerHandRank.FLUSH;
            } else if (straight) {
                this.handRank = PokerHandRank.STRAIGHT;
            } else if (triple) {
                this.handRank = PokerHandRank.THREE_KIND;
            } else if (pair && pair2) {
                this.handRank = PokerHandRank.TWO_PAIR;
            } else if (pair) {
                this.handRank = PokerHandRank.PAIR;
            } else {
                this.handRank = PokerHandRank.HIGH_CARD;
            }
        }
    }
}
