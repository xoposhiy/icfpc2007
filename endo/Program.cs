using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lib;
using MoreLinq;

namespace endo
{
	class Program
	{
		static void Main(string[] args)
		{
			var prefix = args.Length > 0 ? args[0] : "";
			if (prefix.StartsWith("p")) prefix = GetCatalogPagePrefix(int.Parse(prefix.Substring(1)));
			else if (prefix.StartsWith("g")) prefix = GetGenesPagePrefix(int.Parse(prefix.Substring(1)));
			else if (prefix.StartsWith("a")) prefix = ActivateGenePrefix(prefix.Substring(1));
			Console.WriteLine(prefix);
			var endoFile = args.Length > 1 ? args[1] : "endo.dna";
			vm = new Vm();
			Console.WriteLine("loading");
			vm.Load(prefix, endoFile);
			Console.WriteLine("loaded!");
			var rnaCount = 0;
			var updates = 0;
			var processor = new RnaProcessor();
			//processor.OnImageChange += OnImageChanged;
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

		private static string ActivateGenePrefix(string arg)
		{
			var ps = arg.Split(',');
			var off = int.Parse(ps[0], NumberStyles.HexNumber);
			var len = int.Parse(ps[1], NumberStyles.HexNumber);
			var skip = new DnaPattern().Search("IFPICFPPCIFP").Encode();
			return "IIPIFFCPICCFPICICFFFIIPIFFCPICCFPICICFFFIICIICIICIPPP"
				+off.EncodeDna(0, quoted:true)
				+len.EncodeDna(0, quoted: true)
				+"IPPCPIIC" + skip + "IIC";
		}

		private static string GetGenesPagePrefix(int pageNo)
		{
			var green = "IFPICFPPCFFPP";
			var pageNoEncoded = pageNo.EncodeDna();
			var p = new DnaPattern()
				.GroupStart()
				.Search(green)
				.Skip(0x510 - green.Length)
				.GroupEnd()
				.Skip(pageNoEncoded.Length-1);
			var t = new DnaTemplate()
				.Ref(0, 0)
				.Text(pageNoEncoded.Substring(0, pageNoEncoded.Length-1));
			return p.Encode() + t.Encode() + GetCatalogPagePrefix(42);
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
		private static int i = 0;
		private static void SaveBitmap(RnaProcessor processor)
		{
			var name = $"last.{vm.Iterations}.{bmpNo++}.bmp";
			processor.ToBitmap().Save(name);
			Console.WriteLine($"bmp saved to {name}");
		}
		private static void OnImageChanged(RnaProcessor processor, string rna)
		{
			//if (i++ % 100 == 0)
			//{
			//	var outp = processor.undocumentedRnas.OrderByDescending(kv => kv.Value)
			//		.Take(30)
			//		.Select(kv => kv.Value + "\t" + kv.Key)
			//		.ToDelimitedString("\n");
			//	Console.WriteLine(outp);
			//}
			var name = $"{rna}.{vm.Iterations}.{bmpNo++}.bmp";
			processor.ToBitmap().Save(name);
			Console.WriteLine($"bmp saved to {name}");

		}
	}
}
