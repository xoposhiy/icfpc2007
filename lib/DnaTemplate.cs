using System.Collections.Generic;

namespace lib
{
    public class DnaTemplate
    {
        public DnaTemplate(params TBase[] items)
        {
            Items.AddRange(items);
        }

        public DnaTemplate()
        {
        }

        public List<TBase> Items = new List<TBase>();
        public override string ToString()
        {
            return string.Join("", Items);
        }

        public Dna Replace(Dna[] blocks)
        {
            var res = Dna.Empty;
            var word = new List<byte>();
            foreach (var item in Items)
            {
                if (item is TSym sym)
                    word.Add(sym.Sym);
                else
                {
                    if (word.Count > 0)
                    {
                        res = res.Append(new Dna(word.ToArray()));
                        word.Clear();
                    }
                    res = res.Append(item.Replace(blocks));
                }
            }
            if (word.Count > 0)
                res = res.Append(new Dna(word.ToArray()));
            return res;
        }
    }

    public abstract class TBase
    {
        public abstract Dna Replace(Dna[] blocks);
    }

    public class TSym : TBase
    {
        public TSym(byte sym)
        {
            Sym = sym;
        }

        public byte Sym;

        public override string ToString()
        {
            return "ICFP"[Sym].ToString();
        }

        public override Dna Replace(Dna[] blocks)
        {
            return new Dna(new[] { Sym });
        }
    }
    public class TRef : TBase
    {
        public TRef(int blockIndex, int protectionLevel)
        {
            BlockIndex = blockIndex;
            ProtectionLevel = protectionLevel;
        }

        public int BlockIndex;
        public int ProtectionLevel;

        public override string ToString()
        {
            return $"Ref({BlockIndex}, {ProtectionLevel})";
        }

        public override Dna Replace(Dna[] blocks)
        {
            var dna = BlockIndex >= blocks.Length ? Dna.Empty : blocks[BlockIndex];
            return dna.Protect(ProtectionLevel);
        }
    }
    public class TLen : TBase
    {
        public TLen(int blockIndex)
        {
            BlockIndex = blockIndex;
        }

        public int BlockIndex;
        public override string ToString()
        {
            return $"Len({BlockIndex})";
        }

        public override Dna Replace(Dna[] blocks)
        {
            var dna = BlockIndex >= blocks.Length ? Dna.Empty : blocks[BlockIndex];
            return dna.Len.AsDna();
        }
    }

}