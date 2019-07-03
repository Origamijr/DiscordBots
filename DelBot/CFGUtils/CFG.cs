using System;
using System.Collections.Generic;
using System.Text;

namespace DelBot.CFGUtils {
    class CFG {
        private List<string> variables;
        private List<string> terminals;
        private Dictionary<string, List<string>> vRules;
        private Dictionary<string, List<string>> tRules;
        private string start;
        private Dictionary<string, Dictionary<string, float>> pMap;

        public CFG(List<string> variables,
            List<string> terminals,
            Dictionary<string, List<string>> vRules,
            Dictionary<string, List<string>> tRules,
            string start,
            Dictionary<string, Dictionary<string, float>> pMap) {

            this.variables = variables;
            this.terminals = terminals;
            this.vRules = vRules;
            this.tRules = tRules;
            this.start = start;
            this.pMap = pMap;
        }

        public CFG(List<string> variables,
            List<string> terminals,
            Dictionary<string, List<string>> vRules,
            Dictionary<string, List<string>> tRules,
            string start) {

            this.variables = variables;
            this.terminals = terminals;
            this.vRules = vRules;
            this.tRules = tRules;
            this.start = start;
            this.FillPMap();
        }



        public static CFG MakeCFG(List<string> variables, List<string> terminals, string[] rules, string start) {
            Console.Write("Parsing grammar...");
            bool validVariables = true;
            foreach (string v in variables) {
                if (terminals.Contains(v)) {
                    validVariables = false;
                    break;
                }
            }

            if (!validVariables || terminals.Contains("ε")) {
                return null;
            }

            Console.Write("Reading rules...");
            bool validRules = true;
            var vRules = new Dictionary<string, List<string>>();
            var pMap = new Dictionary<string, Dictionary<string, float>>();
            foreach (string a in variables) {
                vRules.Add(a, new List<string>());
                pMap.Add(a, new Dictionary<string, float>());
            }
            foreach (string rule in rules) {
                int split = rule.IndexOf("->");

                if (split == -1) {
                    Console.WriteLine("No transition found: " + rule);
                    validRules = false;
                    break;
                }

                string[] alpha = rule.Substring(0, split).Split(' ');
                string[] beta = rule.Substring(split + 2).Split(' ');

                bool validAlpha = true;
                string alphaRule = "";
                float probability = -1;
                foreach (string a in alpha) {
                    if (a != "") {
                        if (alphaRule == "" && variables.Contains(a)) {
                            alphaRule = a;
                        } else if (probability == -1 && float.TryParse(a, out float p)) {
                            probability = p;
                        } else {
                            Console.WriteLine("Invalid left side: ");
                            validAlpha = false;
                            break;
                        }
                    }
                }

                bool validBeta = true;
                List<string> rewrite = new List<string>();
                foreach (string b in beta) {
                    if (b != "") {
                        if (variables.Contains(b) || terminals.Contains(b)) {
                            rewrite.Add(b);
                        } else {
                            Console.WriteLine("Invalid right side: " + rule.Substring(split + 2));
                            validBeta = false;
                            break;
                        }
                    }
                }

                if (!validBeta || !validAlpha) {
                    validRules = false;
                    break;
                }

                string betaRule = "";
                for (int i = 0; i < rewrite.Count; i++) {
                    if (i > 0) {
                        betaRule += " ";
                    }
                    betaRule += rewrite[i];
                }

                vRules[alphaRule].Add(betaRule);
                pMap[alphaRule][betaRule] = probability;
            }

            foreach (var ruleSet in vRules) {
                if (ruleSet.Value.Count == 0) {
                    Console.WriteLine("Function not well defined");
                    validRules = false;
                    break;
                }
            }

            if (!validRules) {
                return null;
            }
            CFG grammar = new CFG(variables, terminals, vRules, null, start, pMap);
            grammar.FillPMap();
            Console.WriteLine("Grammar complete!");
            return grammar;
        }



        private void FillPMap() {
            Console.Write("Filling probabilities...");
            if (pMap == null) {
                pMap = new Dictionary<string, Dictionary<string, float>>();
            }

            foreach (var alpha in variables) {
                var rules = pMap[alpha];

                float sum = 0.0f;
                int undefinedCount = 0;
                foreach (var rulePPair in rules) {
                    if (rulePPair.Value < 0) {
                        undefinedCount++;
                    } else {
                        sum += rulePPair.Value;
                    }
                }

                if (sum > 1.0f) undefinedCount = rules.Count;

                float evenSplit = (1.0f - sum) / undefinedCount;
                foreach (var beta in vRules[alpha]) {
                    if (rules[beta] == -1 || undefinedCount == rules.Count) {
                        pMap[alpha][beta] = evenSplit;
                    }
                }
                if (tRules != null) {
                    foreach (var beta in tRules[alpha]) {
                        if (rules[beta] == -1 || undefinedCount == rules.Count) {
                            pMap[alpha][beta] = evenSplit;
                        }
                    }
                }
            }
        }



        public List<string> Simulate(int steps) {
            string lastStep = start;
            string currStep = "";
            List<string> stepList = new List<string> { start };

            Random rand = new Random();
            for (int i = 0; i < steps; i++) {
                var letters = lastStep.Split(' ');
                bool variableFound = false;
                bool added = true;
                for (int j = 0; j < letters.Length; j++) {
                    if (i > 0 && added) currStep += " ";
                    if (!variables.Contains(letters[j])) {
                        currStep += letters[j];
                    } else {
                        variableFound = true;
                        float r = (float)rand.NextDouble();
                        string rewrite = "";
                        foreach (var rule in pMap[letters[j]]) {
                            r -= rule.Value;
                            if (r <= 0) {
                                rewrite = rule.Key;
                                break;
                            }
                        }
                        if (rewrite == "") {
                            added = false;
                        } else {
                            added = true;
                            currStep += rewrite;
                        }
                    }
                }
                if (!variableFound) break;
                stepList.Add(currStep);
                lastStep = currStep;
                currStep = "";
            }
            return stepList;
        }



        public void ToCNF() {
            if (tRules != null) return;

            // Verify start variable
            // We want to check if the start variable is a production of a rule
            // If so, then we create a new variable that goes to the start variable and set that as the new start variable
            foreach (var rule in vRules) {
                foreach (var beta in rule.Value) {
                    foreach (var production in beta.Split(' ')) {
                        if (production == start) {
                            int id = 1;
                            while (variables.Contains(start + id) || terminals.Contains(start + id)) id++;
                            variables.Add(start + id);
                            vRules.Add(start + id, new List<string> { start });
                            this.start = start + id;
                            break;
                        }
                    }
                }
            }

            // Eliminate empty productions
            List<string> nullable = new List<string>();
            do {
                List<string> nulled = new List<string>();
                nullable.Clear();

                // find nullable variables
                foreach (var variable in variables) {
                    for (int i = 0; i < vRules[variable].Count; i++) {
                        if (vRules[variable][i] == "") {
                            if (vRules[variable].Count == 1) nulled.Add(variable);
                            else nullable.Add(variable);
                        }
                    }
                }
                if (nullable.Count == 0 || (nullable.Count == 1 && nullable.Contains(start))) break;

                // remove nullable variables from right sides of rules
                foreach (var variable in variables) {
                    List<string> newBeta = new List<string>(vRules[variable]);
                    foreach (var nil in nullable) {
                        for (int i = 0; i < vRules[variable].Count; i++) {
                            string[] beta = vRules[variable][i].Split(' ');

                        }
                    }
                }
            } while (!(nullable.Count == 0 || (nullable.Count == 1 && nullable.Contains(start))));

            // Elliminate Variable unit production

            // Replace Long productions by shorter ones

            // Move terminals to unit production
        }



        public string[] GetVariables() {
            return variables.ToArray();
        }



        public string[] GetTerminals() {
            return terminals.ToArray();
        }



        public string[] GetRules() {
            List<string> rules = new List<string>();
            foreach (var ruleSet in vRules) {
                for (int i = 0; i < ruleSet.Value.Count; i++) {
                    string s = ruleSet.Key + " ";
                    if (ruleSet.Value[i] == "") {
                        s += pMap[ruleSet.Key][ruleSet.Value[i]] + "->";
                    } else {
                        s += pMap[ruleSet.Key][ruleSet.Value[i]] + "-> " + ruleSet.Value[i];
                    }
                    rules.Add(s);
                }
            }
            if (tRules != null) {
                foreach (var ruleSet in tRules) {
                    for (int i = 0; i < ruleSet.Value.Count; i++) {
                        string s = ruleSet.Key + " ";
                        if (ruleSet.Value[i] == "") {
                            s += pMap[ruleSet.Key][ruleSet.Value[i]] + "-> ε";
                        } else {
                            s += pMap[ruleSet.Key][ruleSet.Value[i]] + "-> " + ruleSet.Value[i];
                        }
                        rules.Add(s);
                    }
                }
            }
            return rules.ToArray();
        }



        public string GetStart() {
            return start;
        }



        public override string ToString() {
            string s = "";

            s += "V = { ";
            for (int i = 0; i < variables.Count; i++) {
                if (i > 0) s += ", ";
                s += variables[i];
            }

            s += " }\nΣ = { ";
            for (int i = 0; i < terminals.Count; i++) {
                if (i > 0) s += ", ";
                s += terminals[i];
            }

            s += " }\nR = { ";
            foreach (var ruleSet in vRules) {

                for (int i = 0; i < ruleSet.Value.Count; i++) {
                    s += "\n\t" + ruleSet.Key + " -> ";
                    if (ruleSet.Value[i] == "") {
                        s += "ε";
                    } else {
                        s += ruleSet.Value[i] + "\t|\t" + pMap[ruleSet.Key][ruleSet.Value[i]];
                    }
                }
            }
            if (tRules != null) {
                foreach (var ruleSet in tRules) {

                    for (int i = 0; i < ruleSet.Value.Count; i++) {
                        s += "\n\t" + ruleSet.Key + " -> ";
                        if (ruleSet.Value[i] == "") {
                            s += "ε";
                        } else {
                            s += ruleSet.Value[i] + "\t|\t" + pMap[ruleSet.Key][ruleSet.Value[i]];
                        }
                    }
                }
            }

            s += "\n}\nS = " + start;
            return s;
        }
    }
}
