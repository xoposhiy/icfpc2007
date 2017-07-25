using NUnit.Framework;
using Shouldly;

namespace lib
{
    [TestFixture]
    public class DnaTemplate_Should
    {
        [Test]
        public void ReplaceSymbols()
        {
            var template = new DnaTemplate();
            template.Items.Add(new TSym(Dna.I));
            template.Items.Add(new TSym(Dna.C));
            var dna = template.Replace(new Dna[0]);
            dna.ToString()
                .ShouldBe("IC");
        }
        [Test]
        public void ReplaceRef()
        {
            var template = new DnaTemplate();
            template.Items.Add(new TRef(0, 1));
            var dna = template.Replace(new Dna[] { "ICFP" });
            dna.ToString()
                .ShouldBe("CFPIC");
        }
        [Test]
        public void ReplaceLen()
        {
            var template = new DnaTemplate();
            template.Items.Add(new TLen(0));
            var dna = template.Replace(new Dna[] { "ICFP" });
            dna.ToString()
                .ShouldBe("IICP");
        }
        [Test]
        public void ReplaceAll()
        {
            var template = new DnaTemplate();
            template.Items.Add(new TSym(Dna.I));
            template.Items.Add(new TRef(1, 1));
            template.Items.Add(new TLen(2));
            var dna = template.Replace(new Dna[] { "F", "ICFP", "CP" });
            dna.ToString()
                .ShouldBe("I CFPIC ICP".Replace(" ", ""));
        }
    }
}