using System;
using System.Collections.Generic;

namespace DelBot.HauteAuction {
    class AuctionBot : AuctionIdentifiable {
        public string name { get; private set; }
        public AuctionPlayer backing;

        public AuctionBot(string name) : base('b') {
            this.name = name;
        }
        
        public void SetBacking(AuctionPlayer player) {
            this.backing = player;
        }

        public AuctionPlayer GetBacking() {
            return this.backing;
        }
    }
}