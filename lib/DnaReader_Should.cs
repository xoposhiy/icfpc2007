using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;

namespace lib
{
    public class DnaReader_Should
    {
        [TestCase("P", 0)]
        [TestCase("IP", 0)]
        [TestCase("FP", 0)]
        [TestCase("FIFFIIP", 0)]
        [TestCase("CP", 1)]
        [TestCase("ICP", 2)]
        [TestCase("ICPII", 2)]
        [TestCase("ICFP", 2)]
        [TestCase("CCP", 3)]
        [TestCase("IICP", 4)]
        [TestCase("CICP", 5)]
        [TestCase("ICCP", 6)]
        [TestCase("CCCP", 7)]
        [TestCase("IIICP", 8)]
        public void ReadNat(string input, int expected)
        {
            new DnaReader(input).ReadNat(out var finish)
                .ShouldBe(expected);
            finish.ShouldBe(false);
        }
        [TestCase("I")]
        [TestCase("IC")]
        [TestCase("C")]
        [TestCase("FICCC")]
        [TestCase("FICFF")]
        [TestCase("IIIFC")]
        public void FinishReadingNat(string input)
        {
            new DnaReader(input).ReadNat(out var finish);
            finish.ShouldBe(true);
        }

        [Test]
        public void CutReadingNat()
        {
            var r = new DnaReader("PCPICP");
            r.ReadNat(out var finish).ShouldBe(0);
            r.ReadNat(out finish).ShouldBe(1);
            r.ReadNat(out finish).ShouldBe(2);
            r.Dispose();
            r.Dna.ToString().ShouldBe("");
        }


        [TestCase("IP", "")]
        [TestCase("", "")]
        [TestCase("IC", "P")]
        [TestCase("ICIP", "P")]
        [TestCase("CFPICIP", "ICFP")]
        [TestCase("ICPFCIF", "PFCI")]
        public void ReadConst(string input, string expected)
        {
            new Dna(new DnaReader((Dna)input).ReadConst().ToArray()).ToString()
                .ShouldBe(expected);
        }

        [TestCase("IPC", "C")]
        [TestCase("CIFIC", "IC")]
        [TestCase("ICPFCIF", "")]
        [TestCase("ICPFC", "")]
        public void CutDnaReadingConst(string input, string expected)
        {
            var r = new DnaReader(input);
            r.ReadConst();
            r.Dispose();
            r.Dna.ToString()
                .ShouldBe(expected);
        }

        [TestCase("IIC", "")]
        [TestCase("IIF", "")]
        [TestCase("CIIF", "I")]
        [TestCase("CIIC", "I")]
        [TestCase("IIPIPICPIICICIIF", "(![2])P")]
        [TestCase("IFFCFPICIIICCCCCCPIIC", "?[ICFP]", false, "CCCCCCP")]
        [TestCase("I", "", true, "")]
        [TestCase("II", "", true, "")]
        [TestCase("IIPIFFCPICFPPICIICCIICIPPPFIIC", "(?[IFPCFFP])I", false, "")]
        // IIP IFF CPICICIICPIIC IP PP ICIIC
        // (       IFP P )  F   Rf 00 P .
        [TestCase("IIPIFFCPICICIICPIICIPPPICIIC", "(?[IFPP])F", false, "")]
        [TestCase("IIPIFFFIICIIC", "(?[C])", false, "")]
        [TestCase("IIPCIICIIC", "(I)", false, "")]
        [TestCase("IFFFIIC", "?[C]", false, "")]
        public void ReadPattern(string input, string expected, bool expFinished = false, string expRna = "")
        {
            var rna = new List<Dna>();
            var pattenr = new DnaReader(input)
                .ReadPattern(out var finished, rna);
            finished
                .ShouldBe(expFinished);
            if (!expFinished)
            {
                pattenr.ToString()
                    .ShouldBe(expected);
                string.Join("", rna)
                    .ShouldBe(expRna);
            }
        }

        [TestCase("IIC", "")]
        [TestCase("CIIC", "I")]
        [TestCase("FIIC", "C")]
        [TestCase("PIIC", "F")]
        [TestCase("ICIIC", "P")]
        [TestCase("ICIIF", "P")]
        [TestCase("IFCPICPIIC", "Ref(2, 1)")]
        [TestCase("IPCPICPIIC", "Ref(2, 1)")]
        [TestCase("IIPICPIIC", "Len(2)")]
        [TestCase("IIPICPIPCPICPICIIF", "Len(2)Ref(2, 1)P")]
        [TestCase("I", "", true)]
        [TestCase("II", "", true)]
        [TestCase("IIICCCCCCIIIC", "", false, "CCCCCCI")]
        public void ReadTemplate(string input, string expected, bool expFinished = false, string expRna = "")
        {
            var rna = new List<Dna>();
            var template = new DnaReader(input)
                .ReadTemplate(out var finished, rna);
            finished
                .ShouldBe(expFinished);
            if (!expFinished)
            {
                template.ToString()
                    .ShouldBe(expected);
                string.Join("", rna)
                    .ShouldBe(expRna);
            }
        }
    }
}