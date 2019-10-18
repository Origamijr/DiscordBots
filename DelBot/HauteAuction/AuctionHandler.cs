using System;
using System.Collections.Generic;

namespace DelBot.HauteAuction {
    /* 
    Primary interface between user interface and inner classes
     */
    class AuctionHandler {
        private AuctionGameflow flow;
        private AuctionResources resources;
        private AuctionExecuter executer;

        public AuctionHandler() {
            this.flow = new AuctionGameflow();
            this.resources = new AuctionResources();
            this.executer = new AuctionExecuter(this.resources, this.flow);
        }

        public void AddPlayer(string name) {

        }
    }
}