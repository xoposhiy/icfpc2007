using System.Data;
using NUnit.Framework;
using Shouldly;

namespace lib
{
    public class Vm_Should
    {
        private Vm vm;

        [SetUp]
        public void SetUp()
        {
            vm = new Vm();
        }

        [Test]
        public void MatchReplaceNothing()
        {
            vm.LoadFromString("ICFP");
            vm.MatchReplace(new DnaPattern(new PSym(Dna.P)), new DnaTemplate(new TSym(Dna.F)));
            vm.Dna.ToString()
                .ShouldBe("ICFP");
        }
        [Test]
        public void MatchReplaceOneSymbol()
        {
            vm.LoadFromString("ICFP");
            vm.MatchReplace(new DnaPattern(new PSym(Dna.I)), new DnaTemplate(new TSym(Dna.F)));
            vm.Dna.ToString()
                .ShouldBe("FCFP");
        }
        [Test]
        public void MatchReplaceManySymbols()
        {
            vm.LoadFromString("ICFP");
            vm.MatchReplace(
                new DnaPattern(new PSym(Dna.I), new PSym(Dna.C), new PSym(Dna.F)),
                new DnaTemplate(new TSym(Dna.F), new TSym(Dna.F), new TSym(Dna.C)));
            vm.Dna.ToString()
                .ShouldBe("FFCP");
        }
        [Test]
        public void MatchReplaceBySkip()
        {
            vm.LoadFromString("ICFP");
            vm.MatchReplace(
                new DnaPattern(new PSkip(2), new PSym(PSym.OpenBracket), new PSkip(2), new PSym(PSym.CloseBracket)),
                new DnaTemplate(new TSym(Dna.F), new TLen(0), new TSym(Dna.C)));
            vm.Dna.ToString()
                .ShouldBe("FICPC");
        }
        [Test]
        public void MatchReplaceBySearch()
        {
            vm.LoadFromString("ICFP");
            vm.MatchReplace(
                new DnaPattern(new PSym(PSym.OpenBracket), new PSearch("CF"), new PSym(PSym.CloseBracket)),
                new DnaTemplate(new TRef(0, 1)));
            vm.Dna.ToString()
                .ShouldBe("CFPP");
        }

        //IIP IP ICP IIC IC IIF ICC IF PP IIC CFPC
        //(![2])P PI Ref(0,0)
        [TestCase("IIPIPICPIICICIIFICCIFPPIICCFPC", "PICFC")]
        [TestCase("IIPIPICPIICICIIFICCIFCCCPPIICCFPC", "PIICCFCFFPC")]
        [TestCase("IIPIPIICPIICIICCIICFCFC", "I")]
        public void Execute(string input, string output)
        {
            vm.LoadFromString(input);
            vm.Step();
            vm.Dna.ToString()
                .ShouldBe(output);
        }
    }
}