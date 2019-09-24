using System;

namespace DelBot.HauteAuction {
    class AuctionItem {
        public int id { get; private set; }
        public int uid { get; private set; }
        public string name { get; private set; }
        public string klass { get; private set; }
        public string rarity { get; private set; }
        public string effectText { get; private set; }
        public string flavorText { get; private set; }

        private static int uidCounter = 0;

        public AuctionItem(int id) {
            this.id = id;
            this.uid = uidCounter++;
        }
    }
}