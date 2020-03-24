using System;
using System.Collections.Generic;
using System.Text;

namespace DelBot.Poker {

    public enum PokerSuit {
        CLUBS = 0,
        SPADES,
        DIAMONDS,
        HEARTS
    }

    public enum PokerRank {
        TWO = 0,
        THREE,
        FOUR,
        FIVE,
        SIX,
        SEVEN,
        EIGHT,
        NINE,
        TEN,
        JACK,
        QUEEN,
        KING,
        ACE
    }

    public struct PokerCard {
        public PokerRank Rank { get; }
        public PokerSuit Suit { get; }

        public PokerCard(PokerRank r, PokerSuit s) {
            Rank = r;
            Suit = s;
        }

        public int CompareTo(PokerCard other) {
            if (this.Rank == other.Rank) {
                return this.Suit - other.Suit;
            }
            return this.Rank - other.Rank;
        }

        public static bool operator >(PokerCard c1, PokerCard c2) {
            return c1.CompareTo(c2) > 0;
        }

        public static bool operator <(PokerCard c1, PokerCard c2) {
            return c1.CompareTo(c2) < 0;
        }

        public override string ToString() {
            string ret = "";
            switch (this.Rank) {
                case PokerRank.JACK:
                    ret += 'J';
                    break;
                case PokerRank.QUEEN:
                    ret += 'Q';
                    break;
                case PokerRank.KING:
                    ret += 'K';
                    break;
                case PokerRank.ACE:
                    ret += 'A';
                    break;
                case PokerRank.TEN:
                    ret += "10";
                    break;
                default:
                    ret += (char)('0' + (this.Rank + 2));
                    break;
            }
            switch (this.Suit) {
                case PokerSuit.SPADES:
                    ret += "\u2660";
                    //ret += "S";
                    break;
                case PokerSuit.HEARTS:
                    ret += "\u2665";
                    //ret += "H";
                    break;
                case PokerSuit.CLUBS:
                    ret += "\u2663";
                    //ret += "C";
                    break;
                case PokerSuit.DIAMONDS:
                    ret += "\u2666";
                    //ret += "D";
                    break;
            }
            return ret;
        }
    }
}
