using System;
using System.Collections.Generic;

namespace DelBot.HauteAuction {
    public struct AuctionItemStruct {
        public string id { get; private set; }
        public string name { get; private set; }
        public string klass { get; private set; }
        public string rarity { get; private set; }
        public string effectText { get; private set; }
        public string flavorText { get; private set; }
        public string[] acquireEffect { get; private set; }
        public string[] fanfareEffect { get; private set; }
        public string[] actionEffect { get; private set; }
        public string[] presenceEffect { get; private set; }
        public string[] sellEffect { get; private set; }

        public AuctionItemStruct(
            string id,
            string name,
            string klass,
            string rarity,
            string effectText,
            string flavorText,
            string[] acquireEffect,
            string[] fanfareEffect,
            string[] actionEffect,
            string[] presenceEffect,
            string[] sellEffect
        ) {
            this.id = id;
            this.name = name;
            this.klass = klass;
            this.rarity = rarity;
            this.effectText = effectText;
            this.flavorText = flavorText;
            this.acquireEffect = acquireEffect;
            this.fanfareEffect = fanfareEffect;
            this.actionEffect = actionEffect;
            this.presenceEffect = presenceEffect;
            this.sellEffect = sellEffect;
        }
    }

    class AuctionItem : AuctionIdentifiable, IEquatable<AuctionItem> {
        public string itemId { get; private set; }

        public Dictionary<string, string> locals;

        public AuctionItem(string id) : base('i') {
            this.itemId = id;
        }

        public bool Equals(AuctionItem other) {
            return other != null && other.id == this.id;
        }

        public override bool Equals(object obj) {
            return this.Equals(obj as AuctionItem);
        }

        public override int GetHashCode() {
            return id;
        }

        // Effect interface ====================================================

        public void SetLocal(string key, string value) {
            this.locals[key] = value;
        }

        public string GetLocal(string key) {
            return locals[key];
        }
    }
}