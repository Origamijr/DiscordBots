using System;

namespace DelBot.HauteAuction {
    public enum AuctionPhase {
        DRAW = 0,
        MAIN = 1,
        ACTION = 2,
        END = 3,
        FINISH = 4
    }

    class AuctionGameflow {
        public AuctionPhase phase;
    }
}