using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestBot.Modules {
    class ChatScrape {

        private string dumpFile = "";
        private string strBuffer = "";
        private string speaker = "";
        DateTimeOffset timstamp = new DateTimeOffset();

        /*
         * Time Buckets
         * __1__
         * __5__
         * __15__
         * __30__
         * __60__
         */

        public ChatScrape() {
            this.dumpFile = "" + DateTime.UtcNow.ToString() + "-dumpfile";
        }

        public void storeMessage(SocketUserMessage message) {
            if (speaker == message.Author.Username) {
                
            }
        }
    }
}
