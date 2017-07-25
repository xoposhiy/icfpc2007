using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace lib
{
    [TestFixture]
    public class Dna_Should
    {
        [Test]
        public void LargeSubstring()
        {
            new Dna(Dna.FromString("IIIIIIIIIICCCCCCCCCCFFFFFFFFFFPPPPPPPPPP"))
                .Substring(9, 31).ToString()
                .ShouldBe("ICCCCCCCCCCFFFFFFFFFFP");
        }

        [Test]
        public void Create()
        {
            new Dna(Dna.FromString("ICFPICFP")).ToString()
                .ShouldBe("ICFPICFP");
        }

        [TestCase("ICFP", 0, 2, "IC")]
        [TestCase("ICFP", 2, 0, "")]
        [TestCase("ICFP", 2, 2, "")]
        [TestCase("ICFP", 2, 3, "F")]
        [TestCase("ICFP", 2, 6, "FP")]
        public void Substring(string s, int start, int end, string expected)
        {
            new Dna(Dna.FromString(s)).Substring(start, end).ToString()
                .ShouldBe(expected);
        }

        [TestCase("ICFP", 0, "I")]
        [TestCase("ICFP", 1, "C")]
        [TestCase("ICFP", 2, "F")]
        [TestCase("ICFP", 3, "P")]
        [TestCase("ICFP", 6, "")]
        public void BeIndexed(string s, int index, string expected)
        {
            new Dna(Dna.FromString(s)).Get(index).ToString()
                .ShouldBe(expected);
        }

        [Test]
        public void SubstringWithSingleArg()
        {
            new Dna(Dna.FromString("ICFP")).Substring(2).ToString()
                .ShouldBe("FP");
        }

        [TestCase(0, "P")]
        [TestCase(1, "CP")]
        [TestCase(2, "ICP")]
        [TestCase(3, "CCP")]
        [TestCase(4, "IICP")]
        [TestCase(5, "CICP")]
        public void ConvertIntToDna(int n, string expected)
        {
            n.AsDna().ToString()
                .ShouldBe(expected);
        }

        [TestCase(0, "ICFPC", "ICFPC")]
        [TestCase(0, "", "")]
        [TestCase(1, "ICFP", "CFPIC")]
        [TestCase(2, "ICFP", "FPICCF")]
        public void Protect(int level, string input, string expected)
        {
            ((Dna) input).Protect(level).ToString()
                .ShouldBe(expected);
        }

        [TestCase("ICFP", "CFP", 1, true)]
        [TestCase("ICFP", "CFP", 0, false)]
        [TestCase("ICFP", "CFP", 2, false)]
        [TestCase("ICFP", "CF", 1, true)]
        [TestCase("ICFP", "CF", 0, false)]
        [TestCase("ICFP", "CF", 2, false)]
        [TestCase("ICFP", "IC", 1, false)]
        [TestCase("ICFP", "IC", 2, false)]
        [TestCase("ICFP", "IC", 0, true)]
        public void Match(string text, string search, int start, bool isMatch)
        {
            Dna dna = text;
            dna.Match(start, search)
                .ShouldBe(isMatch);
        }

        [TestCase("ICFP", "C", 2)]
        [TestCase("IIIC", "IC", 4)]
        [TestCase("ICICPIC", "ICP", 5)]
        [TestCase("IIICIIICIIII", "IIII", 12)]
        [TestCase("CCICCICCCCFFCCCCCCCCCCCCCCCCCCICCCFCCICCICCCCFFCCCCCCCCCCCCCCCCCCICCCFCCFCCFFFCFFCFPPFFFFPFFPFFFFFFFFFFFFFCFFFPFCFFFFPFFPFFFFFFFFFFFFFFFFCFFCFFFFPPFFFFFFFFFFFFFFFFFFCFFFCFFCFCICICCCFICFFPFCFFFFPPFFFFFFFFFFFFFFFFFFCFFFCFFCFCICICCFICFFPFCFFFFPPFFFFFFFFFFFFFFFFFFCFFFCFFCFCICICFFICFFPFFPFCFCFCFCICFICCCFICFCFCFPCFFCFCFFPCFCICFICFICFCFCFPPCFPPPPPPICICPPPPPPPPPPPPPPPFPIPICPPIPICPCPIPICPICPIPICPCCPIPICPIICPIPICPCICPPPPPPPPPPPPPPPPPPPPPPPPFPPPPPICICPPPPPPPPPPPPPPPPPFPFFPCICICFCFICCCFIICCCCCCIIIICICICCCCCCCICCPCIIIICIIICICCIIICCCICICPCCCICCCCICICIIIIIIIIIIIPICICIICICICCCICCCCCCICCPICIIIICICCIIIIIIIIIIIIIPICCCCIICCIICCICCCCCCICCPIIPIIPIPIIICCIIIIIIIIIIIIIIIIIIPIICIICIICCCICCICCCCFFCCCCCCCCCCCCCCCCCCICCCFCCICCICIPPPCCFCCICCICCCCFFCCCCCCCCCCCCCCCCCCICCCFCCFFFCFFFCFFCFFFFPPFFFFFFFFFFFFFFFFFFCFFFPFFPFFPPPFPPFPPPPICICPPPPPPPPPPPPPPPPPPFPPFPFCFCFCFPPFPPFPPPPICICPPPPPPPPPPPPPPPPPPFPPPICPPICPPICICICPICICPICICICICCFCFICICICICICICICICICICICICICICICICICICPICICICCFICICPICICPICFCFPCFCFICICCFICICPICICPICICICICCFCFICICICICICICICICICICICICICICICICICICPICICICCFICICPICICPICPFPFPFPICICCFICICCFICPICPICCFCFPICCICFFICICICPICPICCFPICICPICPICICCFPICICPICPICCFCFPICICICCFPFPFPICFPPPICFCFCFPCFFFPCICICCFICCICICFICCCFIPPCPIICCICICICIIIIIIIIIIIIIIIIPIIICCIIIIIIIIIIIIIIIIIIPICCCIICCCIIICCIIIIIICCCPCICCIIIIIIIIIIIIIIIIIIIPIFPICFPPCFIPPIFPICFPPCCIIFPICFPPCIFFPIFPICFPPCIFP", "IFPICFPPCIFF", 1337)]
        public void FindEndingIndex(string text, string search, int expected)
        {
            Dna dna = text;
            dna.FindEndIndex(0, Dna.FromString(search).ToList())
                .ShouldBe(expected);
        }
    }
}