using System.Collections.Generic;
using System.Linq;

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

        public override string ToString()
        {
            return string.Join("", Items);
        }
    }

    public class PBase
    {

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
    }
}