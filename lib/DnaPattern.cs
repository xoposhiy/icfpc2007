using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using NUnit.Framework;

namespace lib
{
    public class DnaPattern
    {
        public DnaPattern(params PBase[] items)
        {
            Items.AddRange(items);
        }

        public DnaPattern()
        {
        }

        public List<PBase> Items = new List<PBase>();
        public byte[] Raw;

        public string Encode()
        {
            return Items.Select(i => i.Encode()).ToDelimitedString("") + "IIC";
        }
        public override string ToString()
        {
            return string.Join("", Items);
        }

        public DnaPattern Scan(string symbol)
        {
            Items.AddRange(symbol.Select(c => new PSym(Dna.FromChar(c))));
            return this;
        }

        public DnaPattern GroupStart()
        {
            Items.Add(new PSym(PSym.OpenBracket));
            return this;
        }
        public DnaPattern GroupEnd()
        {
            Items.Add(new PSym(PSym.CloseBracket));
            return this;
        }
        public DnaPattern Search(string dna)
        {
            Items.Add(new PSearch(dna));
            return this;
        }
        public DnaPattern Skip(int count)
        {
            Items.Add(new PSkip(count));
            return this;
        }
    }

    public abstract class PBase
    {
        public abstract string Encode();
    }

    public class PSym : PBase
    {
        public byte Sym;

        public PSym(byte sym)
        {
            Sym = sym;
        }

        public const byte OpenBracket = 4;
        public const byte CloseBracket = 5;

        public override string ToString()
        {
            return "ICFP()"[Sym].ToString();
        }
        public override string Encode()
        {
            return new[]{"C", "F", "P", "IC", "IIP", "IIC"}[Sym];
        }
    }

    public class PSkip : PBase
    {
        public int Count;

        public PSkip(int count)
        {
            Count = count;
        }

        public override string ToString()
        {
            return $"![{Count}]";
        }
        public override string Encode()
        {
            return "IP" + Count.EncodeDna();
        }
    }

	public class PSearch : PBase
    {
        public List<byte> Dna;

        public PSearch(string dna)
            :this(lib.Dna.FromString(dna).ToList())
        {
        }
        public PSearch(List<byte> dna)
        {
            Dna = dna;
        }

        public override string ToString()
        {
            return $"?[{(Dna)Dna}]";
        }

        public override string Encode()
        {
            return "IFF" + new Dna(Dna.ToArray()).Quote();
        }
    }

    [TestFixture]
    public class DnaPattern_Should
    {
        [Test]
        public void Encode()
        {
            var green = "IFPICFPPCFFPP";
            var offset = 0x33963;
            var p = new DnaPattern()
                .GroupStart()
                .Search(green)
                .Skip(offset - green.Length)
                .GroupEnd()
                .Skip(1);
            var template = new DnaTemplate()
                .Ref(0, 0)
                .Text("P");
            Console.WriteLine(p.Encode() + template.Encode());
        }
    }
}