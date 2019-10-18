using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DelBot.HauteAuction {

    public enum CollectionPermission {
        None = 0,
        Interract = 1,
        View = 2
    }

    class AuctionCollection : AuctionIdentifiable, IEnumerable<AuctionIdentifiable> {
        protected List<AuctionIdentifiable> collection;

        public AuctionCollection() : base('c') {
            collection = new List<AuctionIdentifiable>();
        }

        public IEnumerator<AuctionIdentifiable> GetEnumerator() {
            foreach (var thing in collection) {
                yield return thing;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(AuctionIdentifiable thing) {
            this.collection.Add(thing);
        }

        public void Remove(AuctionIdentifiable thing) {
            this.collection.Remove(thing);
        }
    }

    class AuctionItemCollection : AuctionCollection {
        private Dictionary<AuctionPlayer, CollectionPermission> permissions;

        public AuctionItemCollection() : base() {}

        public void Shuffle() {
            Random r = new Random();
            collection = collection.Select(x => new Tuple<double, AuctionItem>(r.NextDouble(), x as AuctionItem))
                .OrderBy(x => x.Item1)
                .Select(x => x.Item2 as AuctionIdentifiable)
                .ToList();
        }

        public AuctionItem Draw() {
            var item = collection[0];
            collection.RemoveAt(0);
            return item as AuctionItem;
        }

        public void AddTop(AuctionItem item) {
            collection.Insert(0, item);
        }

        public void AddBottom(AuctionItem item) {
            collection.Add(item);
        }

        public bool Contains(AuctionIdentifiable obj) {
            return collection.Contains(obj);
        }

        public bool Contains(string id) {
            return collection.Select(x => x.id.ToString()).Contains(id);
        }

        public AuctionItem GetItem(string id) {
            return collection.Find(x => x.id.ToString() == id) as AuctionItem;
        }
    }
}