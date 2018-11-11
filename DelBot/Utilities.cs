using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DelBot {
    class Utilities {
        private static Random random = new Random();

        // Generate random string
        public static string RandomString(int length) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Get username given id as a string
        public static string GetUsername(string idString, SocketCommandContext context) {

            ulong id = GetId(idString);

            if (id != 0) {

                var user = context.Guild.GetUser(id);

                if (user != null) {
                    return user.Username;
                }
            }

            return null;
        }

        // Parse ID from formatted ID
        public static ulong GetId(string idString) {
            ulong id;

            string modIdString = idString.Replace("<", "").Replace("@", "").Replace("!", "").Replace(">", "");

            if (ulong.TryParse(modIdString, out id)) {

                return id;
            }

            return 0;
        }

        // Trim spaces from beginning and end of string
        public static string TrimSpaces(string s) {
            int start = 0;
            int end = -1;

            for (int i = 0; i < s.Length; i++) {
                start = (start == -1 && s[i] != ' ') ? i : start;
                end = (s[i] != ' ') ? i : end;
            }

            return (start > end) ? "" : s.Substring(start, end - start + 1);
        }

        // Splits a param string into arguments. Arguments enclosed in quotes are considered a single argument.
        public static List<string> ParamSplit(string arg) {
            List<string> args = new List<string>();
            
            if (arg == null) {
                return args;
            }

            for (int i = 0; i < arg.Length; i++) {

                if (arg[i] == ' ' && i == 0) {
                    arg = arg.Substring(1);
                    i--;
                } else if (i == 0 && arg[i] == '"') {
                    int j;
                    for (j = i + 1; j < arg.Length - 1 && arg[j] != '"'; j++) ;
                    if (j - i > 1) {
                        args.Add(arg.Substring(i + 1, j - i - 1));
                    }
                    if (j == arg.Length - 1) break;
                    arg = arg.Substring(j + 1);
                    i = -1;
                } else if (arg[i] == ' ') {
                    args.Add(arg.Substring(0, i));
                    if (i < arg.Length - 1) {
                        arg = arg.Substring(i + 1);
                        i = -1;
                    } else {
                        break;
                    }
                } else if (i == arg.Length - 1) {
                    args.Add(arg);
                }
            }

            return args;
        }
    }
}
