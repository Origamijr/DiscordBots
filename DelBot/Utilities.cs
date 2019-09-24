using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DelBot {
    static class Utilities {
        private static Random random = new Random();

        // Generate random string
        public static string RandomString(int length) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static bool IsSelf(string userMention) {
            return GetId(userMention) == 388256006833963011;
        }

        // Get username given id as a string
        public static string GetUsername(string idString, SocketCommandContext context) {

            ulong id = GetId(idString);

            if (id != 0) {

                var user = context.Guild.GetUser(id);
                if (user != null) {
                    if (user.Nickname != null) {
                        return user.Nickname;
                    }
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
            int start = -1;
            int end = -2;

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

                if ((arg[i] == ' ' || arg[i] == '\n') && i == 0) {
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


        public static IEnumerable<Dictionary<string,string>> ReadTSVLine(this StreamReader sr) {
            string currEntry = "";
            List<string> header = new List<string>();
            bool readingHeader = true;
            Dictionary<string, string> row = new Dictionary<string, string>();
            bool inQuote = false;
            char prevChar = 'a';
            int currCol = 0;
            while (sr.Peek() >= 0) {
                char c = (char)sr.Read();
                switch (c) {
                    case '\t':
                        if (!inQuote) {
                            if (readingHeader) {
                                header.Add(currEntry.Trim());
                            } else {
                                row.Add(header[currCol++], currEntry.Trim());
                            }
                            currEntry = "";
                        } else {
                            currEntry += c;
                        }
                        break;
                    case '\n':
                        if (!inQuote) {
                            if (readingHeader) {
                                header.Add(currEntry.Trim());
                                readingHeader = false;
                            } else {
                                row.Add(header[currCol], currEntry.Trim());
                                yield return new Dictionary<string, string>(row);
                            }
                            currEntry = "";
                            currCol = 0;
                            row.Clear();
                        } else {
                            currEntry += c;
                        }
                        break;
                    case '"':
                        if (prevChar == '"') {
                            currEntry += c;
                            inQuote = !inQuote;
                        } else {
                            inQuote = !inQuote;
                        }
                        break;
                    default:
                        currEntry += c;
                        break;
                }
                prevChar = c;
            }
        }

        // === MATH ============================================================
        public static int OperatorPrecedence(char sgn) {
            switch (sgn) {
                case '+':
                case '-':
                    return -1;
                case '*':
                case '/':
                case '%':
                    return 1;
                default:
                    return 0;
            }
        }

        public static string ConvertToPostFix(string expr) {
            Stack<char> operatorStack = new Stack<char>();
            string postfix = "";

            // Iterate through string
            for (int i = 0; i < expr.Length; i++) {
                char curr = expr[i];

                // Check what character is
                if (char.IsDigit(curr)) {
                    // Append number if digit
                    int l = 1;
                    while (i + l < expr.Length && char.IsDigit(expr[i + l])) l++;
                    postfix += ((postfix == "") ? "" : " ") + expr.Substring(i, l);
                    i = i + l - 1;

                } else if (curr == '(') {
                    // push if left parenthesis
                    operatorStack.Push(curr);

                } else if (curr == ')') {
                    // pop and append until open parenthesis or empty
                    while (operatorStack.Count != 0 && operatorStack.Peek() != '(') {
                        postfix += ((postfix == "") ? "" : " ") + operatorStack.Pop();
                    }
                    if (operatorStack.Count == 0) {
                        return null;
                    } else {
                        operatorStack.Pop();
                    }

                } else if (OperatorPrecedence(curr) != 0) {
                    // if operator
                    if (operatorStack.Count != 0) {
                        // if not empty stack
                        int op1Prec = OperatorPrecedence(curr);
                        int op2Prec = OperatorPrecedence(operatorStack.Peek());
                        while (op2Prec != 0 && op2Prec >= op1Prec) {
                            // if op2 has precedence greater than or equal to op1
                            postfix += ((postfix == "") ? "" : " ") + operatorStack.Pop();

                            // Update operator, and break if empty
                            if (operatorStack.Count != 0) {
                                op2Prec = OperatorPrecedence(operatorStack.Peek());
                            } else {
                                break;
                            }
                        }
                    }
                    // push op1 into stack
                    operatorStack.Push(curr);
                } else if (curr != ' ') {
                    // return null if anything else besides whitespace
                    return null;
                }
            }

            // pop and append the remaining operators in the stack
            while (operatorStack.Count != 0) {
                char popped = operatorStack.Pop();
                if (popped == '(') return null;
                postfix += ((postfix == "") ? "" : " ") + popped;
            }

            return postfix;
        }

        public static int CalculatePostfixExpression(string postfixExpr) {
            Stack<int> operands = new Stack<int>();
            int result = 0;

            for (int i = 0; i < postfixExpr.Length; i++) {
                if (OperatorPrecedence(postfixExpr[i]) != 0) {
                    if (operands.Count < 2) {
                        throw new FormatException();
                    }
                    int int2 = operands.Pop();
                    int int1 = operands.Pop();

                    // Run calculations based on operand
                    switch (postfixExpr[i]) {
                        case '+':
                            result = int1 + int2;
                            break;
                        case '-':
                            result = int1 - int2;
                            break;
                        case '*':
                            result = int1 * int2;
                            break;
                        case '/':
                            result = int1 / int2;
                            break;
                        case '%':
                            result = int1 % int2;
                            break;
                        default:
                            break;
                    }

                    operands.Push(result);
                } else if (char.IsDigit(postfixExpr[i])) {
                    // push number into operand stack
                    int l = 1;
                    while (i + l < postfixExpr.Length && char.IsDigit(postfixExpr[i + l])) l++;
                    operands.Push(int.Parse(postfixExpr.Substring(i, l)));
                    i = i + l - 1;
                }
            }

            if (operands.Count > 1) throw new FormatException();
            return operands.Pop();
        }
    }
}
