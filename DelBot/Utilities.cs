﻿using Discord.Commands;
using System;
using System.Collections;
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

        public static int EvaluateExpression(string expr) {


            return 0;
        }

        private int OperatorPrecedence(char sgn) {
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

        private string convertToPostFix(string expr) {
            Stack<char> operatorStack = new Stack<char>();
            string postfix = "";
            char curr;
            char popped;

            // Iterate through string
            for (int i = 0; i < expr.Length; i++) {
                curr = expr[i];

                // Check what character is
                if (curr - '0' >= 0 && curr - '0' <= 9) {
                    // Append if digit
                    // TODO
                    postfix += "";
                } else if (curr == '(') {
                    // push if left parenthesis
                    operatorStack.Push(curr);
                } else if (curr == ')') {
                    // pop and append until open parenthesis or empty
                    while (operatorStack.Count != 0) {
                        if (operatorStack.Peek() == '(') {
                            operatorStack.Pop();
                            break;
                        }
                        popped = operatorStack.Pop();
                        postfix += popped;
                    }
                } else {
                    // if operator
                    if (operatorStack.Count != 0) {
                        // if not empty stack
                        int op1Prec = OperatorPrecedence(curr);
                        int op2Prec = OperatorPrecedence(operatorStack.Peek());
                        while (op2Prec != 0 && op2Prec >= op1Prec) {
                            // if op2 has precedence greater than or equal to op1
                            popped = operatorStack.Pop();
                            postfix += popped;

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
                }
                i++;
            }

            // pop and append the remaining operators in the stack
            while (operatorStack.Count != 0) {
                popped = operatorStack.Pop();
                if (popped != '(') {
                    postfix += popped;
                }
            }

            return postfix;
        }

        int calculateExpression(string postfixExpr) {
            Stack<int> operands = new Stack<int>();
            int result = 0;

            for (int i = 0; i < postfixExpr.Length; i++) {
                if (OperatorPrecedence(postfixExpr[i]) != 0) {
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
                } else {
                    // TODO push number into operand stack
                }
            }

            return operands.Pop();
        }
    }
}
