using System;
using System.Collections.Generic;
using System.Linq;
using DelBot.Databases;

namespace DelBot.HauteAuction {
    class AuctionResources {
        public AuctionItemCollection house { get; private set; }
        public AuctionItemCollection bidArea { get; private set; }
        
        private AuctionCollection players;
        private AuctionCollection bots;

        private Dictionary<string, AuctionItemStruct> itemDict;

        public AuctionResources() {
            this.initialize();
        }

        private void initialize() {
            // Open database
            var db = JsonDatabase.Open(AuctionUtils.jsonFilename);
            var keys = db.ListKeys();

            // Construct item dictionary from database
            foreach (var id in keys) {
                string name = db.AccessString(new List<string> { id, "Name" });
                string klass = db.AccessString(new List<string> { id, "Class" });
                string rarity = db.AccessString(new List<string> { id, "Rarity" });
                string effectText = db.AccessString(new List<string> { id, "Effect Text" });
                string flavorText = db.AccessString(new List<string> { id, "Flavor Text" });
                string[] acquireEffect = db.AccessArray(new List<string> { id, "Effect", "acquire" });
                string[] fanfareEffect = db.AccessArray(new List<string> { id, "Effect", "fanfare" });
                string[] actionEffect = db.AccessArray(new List<string> { id, "Effect", "action" });
                string[] presenceEffect = db.AccessArray(new List<string> { id, "Effect", "presence" });
                string[] sellEffect = db.AccessArray(new List<string> { id, "Effect", "sell" });

                itemDict.Add(id, new AuctionItemStruct(id, name, klass, rarity, effectText, flavorText, 
                    acquireEffect, fanfareEffect, actionEffect, presenceEffect, sellEffect));
            }

            // Construct house deck
            foreach (var itemData in itemDict.Values) {
                if (itemData.klass != "Neutral") {
                    int freq;
                    switch (itemData.rarity) {
                    case "C":
                        freq = 8;
                        break;
                    case "R":
                        freq = 4;
                        break;
                    case "SR":
                        freq = 2;
                        break;
                    case "SSR":
                        freq = 1;
                        break;
                    default:
                        freq = 0;
                        break;
                    }
                    for (int i = 0; i < freq; i++) {
                        house.AddBottom(new AuctionItem(itemData.id));
                    }
                }
            }
            house.Shuffle();
        }

        public AuctionPlayer GetPlayer(string id) {
            foreach (var player in players) {
                if (player.id.ToString() == id) return player as AuctionPlayer;
            }
            return null;
        }

        public AuctionPlayer GetPlayerWithItem(AuctionItem item) {
            return this.GetPlayerWithItem(item.itemId.ToString());
        }

        public AuctionPlayer GetPlayerWithItem(string id) {
            foreach (var player in players) {
                if ((player as AuctionPlayer).HasItem(id)) return player as AuctionPlayer;
            }
            return null;
        }

        public AuctionBot GetBot(string id) {
            foreach (var bot in this.bots) {
                if (bot.id.ToString() == id) return bot as AuctionBot;
            }
            return null;
        }

        public AuctionItemStruct GetItemData(string id) {
            return itemDict[id];
        }

        public AuctionItem GetItem(string id) {
            foreach (var player in players) {
                var item = (player as AuctionPlayer).GetItem(id);
                if (item != null) return item;
            }
            return null;
        }

        public AuctionCollection GetCollection(string id) {
            if (this.house.id.ToString() == id) return this.house;
            if (this.bidArea.id.ToString() == id) return this.bidArea;
            foreach (var player in players) {
                var coll = (player as AuctionPlayer).GetCollection(id);
                if (coll != null) return coll;
            }
            return null;
        }
    }

    public abstract class AuctionIdentifiable {
        public int id { get; protected set; }
        private static int idCounter = 0;
        public char type { get; protected set; }

        public AuctionIdentifiable(char type) {
            this.id = idCounter++;
            this.type = type;
        }

        public override string ToString() {
            return "" + this.type + this.id;
        }
    }
}