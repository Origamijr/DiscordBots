using System;
using System.Collections.Generic;

namespace DelBot.HauteAuction {
    class AuctionPlayer : AuctionIdentifiable, IEquatable<AuctionPlayer> {
        public string name { get; private set; }

        public AuctionItemCollection deck;
        public AuctionItemCollection discard;
        public AuctionItemCollection hand;
        public AuctionItemCollection revealed;

        public int cookies;
        public Dictionary<AuctionBot, int> favor;

        public AuctionPlayer(string name) : base('p') {
            this.name = name;
            this.favor = new Dictionary<AuctionBot, int>();
        }

        public bool Equals(AuctionPlayer other) {
            return other != null && other.id == this.id;
        }

        public override bool Equals(object obj) {
            return this.Equals(obj as AuctionPlayer);
        }
        
        public override int GetHashCode() {
            return id.GetHashCode();
        }

        public bool HasItem(AuctionItem item) {
            return deck.Contains(item) || discard.Contains(item) || hand.Contains(item);
        }

        public bool HasItem(string id) {
            return deck.Contains(id) || discard.Contains(id) || hand.Contains(id);
        }

        public AuctionItem GetItem(string id) {
            if (deck.Contains(id)) return deck.GetItem(id);
            if (discard.Contains(id)) return discard.GetItem(id);
            if (hand.Contains(id)) return hand.GetItem(id);
            return null;
        }

        public AuctionCollection GetCollection(string id) {
            if (this.deck.id.ToString() == id) return this.deck;
            if (this.discard.id.ToString() == id) return this.discard;
            if (this.hand.id.ToString() == id) return this.hand;
            return null;
        }

        public void Discard(AuctionItem item) {
            if (deck.Contains(item)) {
                deck.Remove(item);
                discard.Add(item);
            }
            if (hand.Contains(item)) {
                discard.Remove(item);
                discard.Add(item);
            }
        }

        public int CountOf(string itemId) {
            int count = 0;
            foreach (AuctionItem item in this.deck) {
                if (item.itemId.ToString() == itemId) count++;
            }
            foreach (AuctionItem item in this.hand) {
                if (item.itemId.ToString() == itemId) count++;
            }
            foreach (AuctionItem item in this.discard) {
                if (item.itemId.ToString() == itemId) count++;
            }

            return count;
        }

        public int GetCookies() {
            return cookies;
        }

        public void SetCookies(int cookies) {
            this.cookies = cookies;
        }

        public void AddCookies(int cookies) {
            this.cookies += cookies;
        }

        public int GetFavor(AuctionBot bot) {
            return this.favor.ContainsKey(bot) ? this.favor[bot] : 0;
        }

        public void SetFavor(AuctionBot bot, int favor) {
            this.favor[bot] = favor;
        }

        public void AddFavor(AuctionBot bot, int favor) {
            if (this.favor.ContainsKey(bot)) {
                this.SetFavor(bot, favor);
            } else {
                this.favor[bot] += favor;
            }
        }
    }
}