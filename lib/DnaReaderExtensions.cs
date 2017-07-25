using System;
using System.Collections.Generic;

namespace lib
{
    public static class DnaReaderExtensions
    {
        public static List<byte> ReadConst(this DnaReader reader)
        {
            var res = new List<byte>();
            var gotI = false;
            while (true)
            {
                var readResult = reader.Read();
                if (readResult == null) break;
                var b = readResult.Value;

                if (!gotI)
                {
                    if (b == Dna.I) gotI = true;
                    else res.Add(b == Dna.C ? Dna.I : b == Dna.F ? Dna.C : Dna.F);
                }
                else
                {
                    if (b == Dna.C)
                    {
                        gotI = false;
                        res.Add(Dna.P);
                    }
                    else
                    {
                        reader.PushBack(b);
                        reader.PushBack(Dna.I);
                        break;
                    }
                }
            }
            return res;
        }

        public static int ReadNat(this DnaReader reader, out bool finish)
        {
            finish = false;
            var multiplier = 1;
            var res = 0;
            while (true)
            {
                var readResult = reader.Read();
                if (readResult == null)
                {
                    finish = true;
                    return res;
                }
                var b = readResult.Value;
                if (b == Dna.P) return res;
                else if (b == Dna.C) res = checked(res + multiplier);
                multiplier = checked(multiplier*2);
            }
        }
        public static DnaPattern ReadPattern(this DnaReader reader, out bool finish, List<Dna> rna)
        {
            finish = false;
            var level = 0;
            var res = new DnaPattern();
            var icount = 0;

            while (!finish)
            {
                var readResult = reader.Read();
                if (readResult == null)
                {
                    finish = true;
                    return res;
                }
                var b = readResult.Value;
                if (icount == 0)
                {
                    if (b == Dna.I) icount++;
                    else
                    {
                        icount = 0;
                        if (b == Dna.C) res.Items.Add(new PSym(Dna.I));
                        else if (b == Dna.F) res.Items.Add(new PSym(Dna.C));
                        else if (b == Dna.P) res.Items.Add(new PSym(Dna.F));
                    }
                }
                else if (icount == 1)
                {
                    if (b == Dna.I) icount++;
                    else
                    {
                        icount = 0;
                        if (b == Dna.C) res.Items.Add(new PSym(Dna.P));
                        else if (b == Dna.P) res.Items.Add(new PSkip(reader.ReadNat(out finish)));
                        else if (b == Dna.F)
                        {
                            reader.Read(); // special case
                            res.Items.Add(new PSearch(reader.ReadConst()));
                        }
                        else throw new FormatException(b.ToString());
                    }
                }
                else if (icount == 2)
                {
                    icount = 0;
                    if (b == Dna.C || b == Dna.F)
                    {
                        if (level == 0)
                        {
                            res.Raw = reader.readBytes.ToArray();
                            return res;
                        }
                        else
                        {
                            level--;
                            res.Items.Add(new PSym(PSym.CloseBracket));
                        }
                    }
                    else if (b == Dna.P)
                    {
                        level++;
                        res.Items.Add(new PSym(PSym.OpenBracket));
                    }
                    else if (b == Dna.I)
                    {
                        rna.Add(reader.Read(7));
                        reader.ClearPrev(10);
                    }
                }
            }
            finish = true;
            return res;
        }

        public static Dna Read(this DnaReader reader, int count)
        {
            var res = new List<byte>();
            for(int i=0; i<count; i++)
            {
                var rr = reader.Read();
                if (rr == null) break;
                res.Add(rr.Value);
            }
            return new Dna(res.ToArray());
        }

        public static DnaTemplate ReadTemplate(this DnaReader reader, out bool finish, List<Dna> rna)
        {
            finish = false;
            var res = new DnaTemplate();
            var icount = 0;

            while (!finish)
            {
                var readResult = reader.Read();
                if (readResult == null)
                {
                    finish = true;
                    return res;
                }
                var b = readResult.Value;
                if (icount == 0)
                {
                    if (b == Dna.I) icount++;
                    else
                    {
                        icount = 0;
                        if (b == Dna.C) res.Items.Add(new TSym(Dna.I));
                        else if (b == Dna.F) res.Items.Add(new TSym(Dna.C));
                        else if (b == Dna.P) res.Items.Add(new TSym(Dna.F));
                    }
                }
                else if (icount == 1)
                {
                    if (b == Dna.I) icount++;
                    else
                    {
                        icount = 0;
                        if (b == Dna.C) res.Items.Add(new TSym(Dna.P));
                        else if (b == Dna.P || b == Dna.F)
                        {
                            var level = reader.ReadNat(out finish);
                            if (finish) return res;
                            var index = reader.ReadNat(out finish);
                            if (finish) return res;
                            res.Items.Add(new TRef(index, level));
                        }
                        else throw new FormatException(b.ToString());
                    }
                }
                else if (icount == 2)
                {
                    icount = 0;
                    if (b == Dna.C || b == Dna.F)
                        return res;
                    else if (b == Dna.P)
                    {
                        res.Items.Add(new TLen(reader.ReadNat(out finish)));
                    }
                    else if (b == Dna.I)
                    {
                        rna.Add(reader.Read(7));
                    }
                }
            }
            finish = true;
            return res;
        }


    }
}