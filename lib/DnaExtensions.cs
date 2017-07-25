using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace lib
{
    public static class DnaExtensions
    {
        public static Dna Substring(this Dna dna, int start, int exclusiveEnd)
        {
            exclusiveEnd = Math.Min(dna.Len, exclusiveEnd);
            var length = Math.Max(0, exclusiveEnd - start);
            return dna.StrictSubstring(start, length);
        }
        public static Dna Substring(this Dna dna, int start)
        {
            return dna.Substring(start, dna.Len);
        }
        public static Dna Get(this Dna dna, int index)
        {
            return dna.Substring(index, index + 1);
        }
        private static readonly byte[][] quotation = { new[] { Dna.C }, new[] { Dna.F }, new[] { Dna.P }, new[] { Dna.I, Dna.C } };
        public static Dna Quote(this Dna dna)
        {
            var res = new List<byte>(dna.Len);
            foreach (var b in dna.Enumerate())
                res.AddRange(quotation[b]);
            return res;

        }
        public static Dna Protect(this Dna dna, int protectionLevel)
        {
            if (protectionLevel > 1000)
                Console.WriteLine($"protect dna.Len={dna.Len} level={protectionLevel}");
            if (protectionLevel == 0 || dna.Len == 0) return dna;
            return Protect(dna.Quote(), protectionLevel - 1);
        }
        public static Dna AsDna(this int number)
        {
            var res = new List<byte>();
            while (number != 0)
            {
                res.Add(number % 2 == 0 ? Dna.I : Dna.C);
                number /= 2;
            }
            res.Add(Dna.P);
            return new Dna(res.ToArray());
        }

        public static int[] GetPrefixFunction(this List<byte> s)
        {
            int n = s.Count;
            var pi = new int[n];
            for (int i = 1; i < n; ++i)
            {
                int j = pi[i - 1];
                while (j > 0 && s[i] != s[j])
                    j = pi[j - 1];
                if (s[i] == s[j]) ++j;
                pi[i] = j;
            }
            return pi;
        }
        public static int FindEndIndex(this Dna text, int start, List<byte> postfix)
        {
            var pi = postfix.GetPrefixFunction();
            var i = 0;
            var k = 0;
            foreach (var b in text.EnumerateFromIndex(start))
            {
                if (b == postfix[k])
                    k++;
                else
                {
                    while (k > 0 && b != postfix[k])
                        k = pi[k - 1];
                    if (b == postfix[k])
                        k++;
                }
                i++;
                if (k == postfix.Count) return i;
            }
            return -1;
        }

        public static bool Match(this Dna text, int start, Dna postfix)
        {
            var textEnumeration = text.EnumerateFromIndex(start);
            var len = postfix.Len;
            var sameLen = textEnumeration
                .Zip(postfix.Enumerate(), (a, b) => a == b)
                .TakeWhile(t => t)
                .Count();
            return sameLen == len;
        }

    }
}