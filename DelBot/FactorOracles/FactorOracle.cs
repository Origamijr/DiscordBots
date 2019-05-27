using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DelBot.FactorOracles {
    class FactorOracle<T> {
        private int size;
        private List<T> sequence;
        private List<Dictionary<T, int>> forwardLinks;
        private List<int> suffixLinks;
        private List<List<int>> reverseSuffixLinks;

        public FactorOracle(List<T> s) {
            size = 0;
            int n = s.Count;
            sequence = new List<T>();
            forwardLinks = new List<Dictionary<T, int>>();
            suffixLinks = new List<int>();
            reverseSuffixLinks = new List<List<int>>();
            for (int i = 0; i <  n; i++) {
                suffixLinks.Add(-1);
                reverseSuffixLinks.Add(new List<int>());
                forwardLinks.Add(new Dictionary<T, int>());
            }

            foreach (var symbol in s) {
                AddFrame(symbol);
            }
            int curr = 0;
            foreach (var links in forwardLinks) {
                string str = "" + (curr++) + ": ";
                foreach (var entry in links) {
                    str += "" + entry.Key + "-" + entry.Value + " ";
                }
                Console.WriteLine(str);
            }
        }

        private void AddFrame(T symbol) {
            Console.WriteLine("adding " + symbol);
            sequence.Add(symbol);
            if (size != 0) {
                AddForwardLink(size - 1, size, symbol);
                int k = suffixLinks[size - 1];
                while (k > -1 && !LinkExistsTo(k, symbol)) {
                    AddForwardLink(k, size, symbol);
                    k = suffixLinks[k];
                }
                Console.WriteLine(k == -1 ? "no link" : "link found");
                if (k == -1) AddSuffixLink(size, 0);
                else AddSuffixLink(size, forwardLinks[k][symbol]);
            }
            size++;
        }

        private void AddForwardLink(int i, int j, T symbol) {
            Console.WriteLine("adding link from " + i + " to " + j + " by " + symbol);
            forwardLinks[i].Add(symbol, j);
        }

        private void AddSuffixLink(int i, int j) {
            Console.WriteLine("adding suffix link from " + i + " to " + j);
            suffixLinks[i] = j;
            reverseSuffixLinks[j].Add(i);
        }

        private bool LinkExistsTo(int i, T symbol) {
            Console.WriteLine("checking link from " + i + " by " + symbol);
            return forwardLinks[i].ContainsKey(symbol);
        }



        public bool Query(List<T> l) {
            int curr = 0;
            int cursor = 0;
            while (cursor < l.Count) {
                if (!LinkExistsTo(curr, l[cursor])) return false;
                curr = forwardLinks[curr][l[cursor++]];
            }

            return true;
        }
    }
}
