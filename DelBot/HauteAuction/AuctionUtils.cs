using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DelBot.Databases;

namespace DelBot.HauteAuction {
    public class AuctionUtils {

        public const string tsvFilename = "AuctionItems.tsv";
        public const string jsonFilename = "AuctionItems.json";

        public static void ConstructJson(string tsvFilename, string jsonFilename) {

            using (StreamReader sr = new StreamReader(tsvFilename, Encoding.Default)) {
                var jsonDB = JsonDatabase.Open(jsonFilename);
                jsonDB.Clear();
                var name2ID = new Dictionary<string, string>();
                var id2Effect = new Dictionary<string, string>();
                foreach (var row in sr.ReadTSVLine()) {
                    string id = row["ID"];
                    string name = row["Name"];
                    string klass = row["Class"];
                    string rarity = row["Rarity"];
                    string effectText = row["Effect Text"];
                    string flavorText = row["Flavor Text"];
                    name2ID.Add(name, id);
                    id2Effect.Add(id, effectText);

                    jsonDB.WriteString(new List<string> { id, "Name" }, name);
                    jsonDB.WriteString(new List<string> { id, "Class" }, klass);
                    jsonDB.WriteString(new List<string> { id, "Rarity" }, rarity);
                    jsonDB.WriteString(new List<string> { id, "Effect Text" }, effectText);
                    jsonDB.WriteString(new List<string> { id, "Flavor Text" }, flavorText);
                }

                foreach (var item in id2Effect) {
                    string id = item.Key;
                    string effectText = item.Value;
                    string[] effects = effectText.Split('\n');
                    foreach (string effect in effects) {
                        string[] words = effect.Split();
                        string timing = words[0].Substring(0, words[0].Length - 1);
                        jsonDB.WriteString(new List<string> {id, "Effect", timing}, String.Join(' ', words.ToList().GetRange(1, words.Length - 1)).Trim());
                    }
                }

                jsonDB.Close();
            }
            
            return;
        }
    }
}