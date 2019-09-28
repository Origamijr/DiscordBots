using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

                // Translate the basic information into the json file
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

                // Parse the effects for keywords
                foreach (var item in id2Effect) {
                    string id = item.Key;
                    string effectText = item.Value;
                    string[] effects = effectText.Split('\n');

                    // Iterate over the various effect timings
                    foreach (string effect in effects) {
                        string[] words = effect.Split();
                        string timing = words[0].Substring(0, words[0].Length - 1);
                        string effectBody = effect.Substring(timing.Length + 2).Trim();
                        List<string> effectParts = effectBody.Split('.').Select(eff => eff.Trim()).Where(s => s != "").ToList();
                        for (int i = 0; i < effectParts.Count; i++) {
                            var dependencyMatch = Regex.Match(effectParts[i], "^if (*), (*)$", RegexOptions.IgnoreCase);
                            if (dependencyMatch.Success) {
                                effectParts.Insert(i + 1, dependencyMatch.Groups[1].Value);
                                effectParts[i] = dependencyMatch.Groups[0].Value + " ? " + (i + 1);
                            }
                        }
                        jsonDB.WriteArray(new List<string> {id, "Effect", timing}, effectParts.ToArray());
                    }
                }

                jsonDB.Close();
            }
            
            return;
        }
    }
}