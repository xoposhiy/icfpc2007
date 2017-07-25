using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MoreLinq;

namespace lib
{
    public class Vm
    {
        private Dna dna;

        private bool firstLog = true;
        public int Iterations;
        public List<Dna> rna = new List<Dna>();
        public Dna Dna => dna;

        public void Load(string prefix, string filename)
        {
            dna = Dna.Load(prefix, filename);
        }

        public void LoadFromString(string input)
        {
            dna = input;
        }

        public IEnumerable<Dna> Execute()
        {
            var i = 0;
            while (true)
            {
                if (Step()) break;
                for (; i < rna.Count; i++)
                    yield return rna[i];
                Iterations++;
            }
            for (; i < rna.Count; i++)
                yield return rna[i];
        }

        public bool Step()
        {
            DnaPattern p;
            DnaTemplate t;
            var rnaStartCount = rna.Count;
            using (var reader = new DnaReader(
                dna, (head, d) =>
                {
                    dna = d;
                }))
            {
                p = reader.ReadPattern(out var finish, rna);
                if (finish) return true;
                t = reader.ReadTemplate(out finish, rna);
                if (finish) return true;
            }
            var rnasIssued = rna.Count - rnaStartCount;
            Log(
                new[]
                {
                    $"Iteration # {Iterations}, rnas issued: {rnasIssued}",
                    $"P: {p.Raw.Select(b => "ICFP"[b]).ToDelimitedString("")}",
                    p.ToString(),
                    "T: " + t
                });
            if (!MatchReplace(p, t))
                Log(new[] { "NOT REPLACED :(" });
            Log(new[] { $"Dna len = {dna.Len} height = {dna.root.Height} nodesCount = {dna.root.NodesCount}" });
            return false;
        }

        private void Log(string[] lines)
        {
            return;
            if (firstLog) File.Delete("dna.log");
            firstLog = false;
            File.AppendAllLines("dna.log", lines);
        }

        public bool MatchReplace(DnaPattern pattern, DnaTemplate template)
        {
            var i = 0;
            var blocks = new List<Dna>();
            var stack = new Stack<int>();
            foreach (var p in pattern.Items)
                switch (p)
                {
                    case PSym sym:
                        if (sym.Sym == PSym.OpenBracket)
                        {
                            stack.Push(i);
                            break;
                        }
                        else if (sym.Sym == PSym.CloseBracket)
                        {
                            var start = stack.Pop();
                            blocks.Add(dna.Substring(start, i));
                            break;
                        }
                        if (i <= dna.Len && dna[i] == sym.Sym) i++;
                        else return false;
                        break;
                    case PSkip skip:
                        i += skip.Count;
                        if (i > dna.Len) return false;
                        break;
                    case PSearch search:
                        var n = dna.FindEndIndex(i, search.Dna);
                        if (n < 0) return false;
                        i += n;
                        break;
                }
            dna = template.Replace(blocks.ToArray())
                .Append(dna.Substring(i));
            return true;
        }

        public void SaveRnas(string filename, List<Dna> rnas)
        {
            File.WriteAllLines(filename, rnas.Select(r => r.ToString()));
        }
    }
}