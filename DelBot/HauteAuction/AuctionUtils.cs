using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DelBot.Databases;

namespace DelBot.HauteAuction {
    public class AuctionUtils {

        public const string tsvFilename = "runtime_db/DelBot/AuctionItems.tsv";
        public const string jsonFilename = "runtime_db/DelBot/AuctionItems.json";

        public static void ConstructJson(string tsvFilename, string jsonFilename) {

            using (StreamReader sr = new StreamReader(tsvFilename, Encoding.Default)) {
                var jsonDB = JsonDatabase.Open(jsonFilename);
                jsonDB.Clear();
                var name2ID = new Dictionary<string, string>();
                var id2Effect = new Dictionary<string, string>();

                // Translate the basic information into the json file
                foreach (var row in sr.ReadTSVLine()) {
                    string id = row["ID"];
                    string name = row["Name"];
                    string klass = row["Class"];
                    string rarity = row["Rarity"];
                    string effectText = row["Effect Text"];
                    string flavorText = row["Flavor Text"];
                    List<string> codeText = row["Effect Code"].Split('\n').Select(s => s.Trim()).Where(s => s != "").ToList();
                    
                    jsonDB.WriteString(new List<string> { id, "Name" }, name);
                    jsonDB.WriteString(new List<string> { id, "Id" }, id);
                    jsonDB.WriteString(new List<string> { id, "Class" }, klass);
                    jsonDB.WriteString(new List<string> { id, "Rarity" }, rarity);
                    jsonDB.WriteString(new List<string> { id, "Effect Text" }, effectText);
                    jsonDB.WriteString(new List<string> { id, "Flavor Text" }, flavorText);
                    
                    string prevTiming = "";
                    List<string> lines = new List<string>();
                    foreach (var line in codeText) {
                        string timing = line.Split()[0];
                        string rest = line.Substring(timing.Length + 1).Trim();
                        if (timing != prevTiming && prevTiming != "") {
                            jsonDB.WriteArray(new List<string> { id, "Effect", prevTiming }, lines.ToArray());
                            lines.Clear();
                        }
                        lines.Add(rest);
                        prevTiming = timing;
                    }
                    jsonDB.WriteArray(new List<string> { id, "Effect", prevTiming }, lines.ToArray());
                }

                jsonDB.Close();
            }
            
            return;
        }
    }
}