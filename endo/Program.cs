using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lib;

namespace endo
{
    class Program
    {
        static void Main(string[] args)
        {
            var prefix = args.Length > 0 ? args[0] : "";
            if (prefix.StartsWith("p")) prefix = GetCatalogPagePrefix(int.Parse(prefix.Split('p')[1]));
            Console.WriteLine(prefix);
            var endoFile = args.Length > 1 ? args[1] : "endo.dna";
            vm = new Vm();
            Console.WriteLine("loading");
            vm.Load(prefix, endoFile);
            Console.WriteLine("loaded!");
            var rnaCount = 0;
            var updates = 0;
            var processor = new RnaProcessor();
            //processor.OnImageChange += SaveBitmap;
            var sw = Stopwatch.StartNew();
            foreach (var rna in vm.Execute())
            {
                //File.AppendAllLines("last.rna", new[]{rna.ToString()});
                rnaCount++;
                processor.Step(rna);
                if (vm.Iterations > updates + 10000)
                {
                    updates = vm.Iterations;
                    Console.WriteLine($"{sw.Elapsed} iterations {vm.Iterations}, speed = {vm.Iterations / sw.Elapsed.TotalSeconds:0} iterations/sec ");
                }
            }
            SaveBitmap(processor);
            //Console.WriteLine($"Elapsed {sw.Elapsed}");
        }

        private static string GetCatalogPagePrefix(int pageNo)
        {
            var sb = new StringBuilder();
            while (pageNo != 0)
            {
                if (pageNo % 2 == 1) sb.Append('F');
                else sb.Append('C');
                pageNo /= 2;
            }
            return $"IIPIFFCPICFPPICIIC{new string('C', sb.Length)}IICIPPP{sb}IIC";
        }

        private static int bmpNo = 0;
        private static Vm vm;

        private static void SaveBitmap(RnaProcessor processor)
        {
            var name = $"last.{vm.Iterations}.{bmpNo++}.bmp";
            processor.ToBitmap().Save(name);
            Console.WriteLine($"bmp saved to {name}");
        }
    }
}
