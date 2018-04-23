using DelBot.Databases;
using System;
using System.Collections.Generic;
using System.Text;

namespace DelBot.Modules.RPG {
    class RPGProfile {

        string rpgTag = "rpg";
        string nameTag = "name";
        string weaponTag = "weapon";
        string strengthTag = "str";
        string vitalityTag = "vit";
        string intelligenceTag = "int";
        string dexterityTag = "dex";
        string dbName = "Profiles.json";
        
        public string id, name = null;
        public int strength, vitality, intelligence, dexterity;

        public int hp, atk, def, agi, basehp = 20, basedmg = 1, basespd = 10;
        public float acc, eva, crt, cdm, evaCap = 0.75f;
        
        public RPGProfile(string user) {
            
            this.id = user;

            UserDatabase db = UserDatabase.Open(dbName);

            if (db.IsOpen()) {

                string retrievedStr = db.AccessString(new List<string> { id, rpgTag, nameTag });
                if (retrievedStr == null) {
                    name = "";
                    strength = 0;
                    vitality = 0;
                    intelligence = 0;
                    dexterity = 0;
                } else {
                    name = retrievedStr;
                    if (!int.TryParse(db.AccessString(new List<string> { id, rpgTag, strengthTag }), out strength)) strength = 0;
                    if (!int.TryParse(db.AccessString(new List<string> { id, rpgTag, vitalityTag }), out vitality)) vitality = 0;
                    if (!int.TryParse(db.AccessString(new List<string> { id, rpgTag, intelligenceTag }), out intelligence)) intelligence = 0;
                    if (!int.TryParse(db.AccessString(new List<string> { id, rpgTag, dexterityTag }), out dexterity)) dexterity = 0;
                }

                db.Close();
            }
        }

        public static RPGProfile Open(string user) {
            RPGProfile profile = new RPGProfile(user);
            if (profile.name == null) return null;
            return profile;
        }

        public bool Close() {
            UserDatabase db = UserDatabase.Open(dbName);

            if (db.IsOpen()) {

                if (!db.WriteString(new List<string> { id, rpgTag, nameTag }, name)) return false;
                if (!db.WriteString(new List<string> { id, rpgTag, strengthTag }, "" + strength)) return false;
                if (!db.WriteString(new List<string> { id, rpgTag, vitalityTag }, "" + vitality)) return false;
                if (!db.WriteString(new List<string> { id, rpgTag, intelligenceTag }, "" + intelligence)) return false;
                if (!db.WriteString(new List<string> { id, rpgTag, dexterityTag }, "" + dexterity)) return false;

                db.Close();

                return true;
            }

            return false;
        }

        public bool SetStats(int s, int v, int i, int d) {
            if (s < 0 || s > 10 || v < 0 || v > 10 || i < 0 || i > 10 || d < 0 || d > 10 || s + v + i + d < 0 || s + v + i + d > 20) return false;

            strength = s;
            vitality = v;
            intelligence = i;
            dexterity = d;

            return true;
        }


        public void SetBattleStats() {
            hp = 5 * vitality + 2 * strength + basehp;
            atk = 2 * strength + basedmg;
            def = strength / 2 + vitality / 2;
            agi = vitality / 2 + 2 * dexterity + basespd;
            acc = 0.33f + 0.66f / (1.0f + MathF.Pow(1.5f, -5.0f - (3 * intelligence + dexterity - 3 * strength)));
            eva = evaCap / (1.0f + MathF.Pow(1.2f, 15.0f - (2 * dexterity + intelligence)));
            crt = 1.0f / (1.0f + MathF.Pow(1.2f, 15.0f - (2 * intelligence + dexterity)));
            cdm = 1.5f + (2 * strength + dexterity) / 10.0f;
        }

        public string GetClass() {
            List<Tuple<int, string>> stats = new List<Tuple<int, string>>();

            stats.Add(Tuple.Create(strength, "str"));
            stats.Add(Tuple.Create(vitality, "vit"));
            stats.Add(Tuple.Create(intelligence, "int"));
            stats.Add(Tuple.Create(dexterity, "dex"));
            stats.Sort();
            stats.Reverse();
            
            if (stats[0].Item1 - stats[3].Item1 > 3) {
                
                if (stats[0].Item1 - stats[1].Item1 >= 3) {
                    
                    if (stats[0].Item2 == "str") return "Brawler";
                    if (stats[0].Item2 == "vit") return "Tank";
                    if (stats[0].Item2 == "int") return "Tactitian";
                    if (stats[0].Item2 == "dex") return "Ranger";
                } else {
                    if (stats[1].Item1 - stats[2].Item1 >= 1) {
                        if (stats[0].Item2 == "str") {
                            if (stats[1].Item2 == "vit") return "Brute";
                            if (stats[1].Item2 == "int") return "Martial Artist";
                            if (stats[1].Item2 == "dex") return "Killer";

                        } else if (stats[0].Item2 == "vit") {
                            if (stats[1].Item2 == "str") return "Juggernaut";
                            if (stats[1].Item2 == "int") return "Knight";
                            if (stats[1].Item2 == "dex") return "Boxer";

                        } else if (stats[0].Item2 == "int") {
                            if (stats[1].Item2 == "str") return "Monk";
                            if (stats[1].Item2 == "vit") return "Observer";
                            if (stats[1].Item2 == "dex") return "Scout";

                        } else if (stats[0].Item2 == "dex") {
                            if (stats[1].Item2 == "str") return "Assassin";
                            if (stats[1].Item2 == "vit") return "Ambusher";
                            if (stats[1].Item2 == "int") return "Ninja";

                        }
                    }
                }
            }

            return "Fighter";
        }

        public override string ToString() {
            SetBattleStats();

            return "```Name: " + name + "\n" +
                      "Title: " + GetClass() + "\n" +
                      "STR: " + strength + "\n" +
                      "VIT: " + vitality + "\n" +
                      "INT: " + intelligence + "\n" +
                      "DEX: " + dexterity + "\n\n" +
                      "HP: " + hp + "\n" +
                      "ATK: " + atk + "\n" +
                      "DEF: " + def + "\n" +
                      "AGI: " + agi + "\n" +
                      "ACC: " + acc + "\n" +
                      "EVA: " + eva + "\n" +
                      "CRT: " + crt + "\n" +
                      "CDM: " + cdm + "```";
        }

    }
}
