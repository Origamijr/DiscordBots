using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DelBot.Databases {
    class UserDatabase {
        private static SortedSet<string> openFiles = new SortedSet<string>();

        private JObject profiles = null;
        private string filename;

        // Basic constructor
        public UserDatabase(string filename) {
            if (!(openFiles.Contains(filename))) {

                openFiles.Add(filename);
                this.filename = filename;

                try {
                    using (StreamReader sr = File.OpenText(filename)) {
                        profiles = (JObject)JToken.ReadFrom(new JsonTextReader(sr));
                    }
                } catch (IOException) {
                    profiles = new JObject();
                }

                Console.WriteLine("Successfully opened " + filename);
            }
        }

        // Open database
        public static UserDatabase Open(string filename) {
            return new UserDatabase(filename);
        }

        // List all the high level members in a file
        public static List<string> ListHigh(string filename) {
            List<string> l = new List<string>();

            JObject tempJ;

            try {
                using (StreamReader sr = File.OpenText(filename)) {
                    tempJ = (JObject)JToken.ReadFrom(new JsonTextReader(sr));
                }

                foreach (var u in tempJ) {
                    l.Add(u.Key);
                }

            } catch (IOException) {

            }

            return l;

        }

        // delete data in database
        public static bool PurgeFile(string filename) {

            if (openFiles.Contains(filename)) {
                return false;
            }

            JObject profiles = new JObject();

            System.IO.File.WriteAllText(filename, profiles.ToString());

            return true;
        }

        // delete single bucketg in database
        public static bool PurgeUser(string filename, string user) {
            if (user == null || filename == null) {
                return false;
            }

            if (openFiles == null) {
                openFiles = new SortedSet<string>();
            }

            JObject tempJ;

            if (!(openFiles.Contains(filename))) {

                openFiles.Add(filename);

                try {
                    using (StreamReader sr = File.OpenText(filename)) {
                        tempJ = (JObject)JToken.ReadFrom(new JsonTextReader(sr));
                    }
                } catch (IOException) {
                    tempJ = new JObject();
                }

                Console.WriteLine("Successfully opened " + filename);

                JObject step = tempJ;

                step.Remove(user);

                System.IO.File.WriteAllText(filename, tempJ.ToString());
                openFiles.Remove(filename);

                return true;
            }

            return false;
        }
        
        // Check if a database is open
        public bool IsOpen() {
            return profiles != null;
        }

        // Close database. Required to open database again
        public bool Close() {
            if (profiles != null) {
                System.IO.File.WriteAllText(filename, profiles.ToString());
                profiles = null;
                openFiles.Remove(filename);
                return true;
            }

            return false;
        }

        /*
         *  JToken
         *      JContainer
         *          JArray
         *          JObject
         *          JProperty
         *      JValue
         * Thanks Brian Rogers from stack overflow
         */


        // Access a string in the JObject
        public string AccessString(List<string> keys) {
            if (keys == null || profiles == null) {
                return null;
            }

            JObject step = profiles;
            for (int i = 0; i < keys.Count - 1; i++) {
                step = step[keys[i]] as JObject;

                if (step == null) {
                    return null;
                }
            }

            return (string)step[keys[keys.Count - 1]];
        }

        // Write a string to the JObject
        public bool WriteString(List<string> keys, string s) {
            if (keys == null || profiles == null) {
                return false;
            }

            JObject step = profiles;
            for (int i = 0; i < keys.Count - 1; i++) {

                if (step[keys[i]] == null) {
                    step[keys[i]] = new JObject();
                }

                step = step[keys[i]] as JObject;
            }

            step[keys[keys.Count - 1]] = s;
            return true;
        }
    }
}
