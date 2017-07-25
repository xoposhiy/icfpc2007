using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using MoreLinq;
using NUnit.Framework;

namespace lib
{
    public class Pos
    {
        public int X, Y;

        public Pos(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"{X},{Y}";
        }
    }

    public class Bmp
    {
        public Color[,] Pixels = new Color[600, 600];

        public Bmp()
        {
            for (var x = 0; x < 600; x++)
                for (var y = 0; y < 600; y++)
                    Pixels[y, x] = Color.FromArgb(0, Color.Black);
        }
    }

    public enum Dir
    {
        N,
        E,
        S,
        W
    }

    public class RnaProcessor
    {
        private Stack<Bmp> bitmaps = new Stack<Bmp>();
        private ColorsBucket bucket = new ColorsBucket();
        private Dir dir = Dir.E;
        private Pos mark = new Pos(0, 0);
        private Pos pos = new Pos(0, 0);
        private int rnaIndex = 0;
        private Dictionary<string, int> counters = new Dictionary<string, int>();

        public RnaProcessor()
        {
            bitmaps.Push(new Bmp());
        }

        public event Action<RnaProcessor> OnImageChange;

        public Bitmap ToBitmap()
        {
            var res = new Bitmap(600, 600, PixelFormat.Format24bppRgb);
            for (var x = 0; x < 600; x++)
                for (var y = 0; y < 600; y++)
                    res.SetPixel(x, y, GetPixel(x, y));
            return res;
        }

        private bool firstLog = true;
        public void Log(string s)
        {
            return;
            if (firstLog) File.Delete("rna.log");
            firstLog = false;
            s += $" {pos} {mark} {bucket.CurrentPixel()} bmps: {bitmaps.Count} dir:{dir}";
            File.AppendAllLines("rna.log", new[] { s });
        }

        public void Step(Dna rna)
        {
            var r = rna.ToString();
            if (r == "PIPIIIC") AddColor(Color.Black);
            else if (r == "PIPIIIP") AddColor(Color.Red);
            else if (r == "PIPIICC") AddColor(Color.Green);
            else if (r == "PIPIICF") AddColor(Color.Yellow);
            else if (r == "PIPIICP") AddColor(Color.Blue);
            else if (r == "PIPIIFC") AddColor(Color.Magenta);
            else if (r == "PIPIIFF") AddColor(Color.Cyan);
            else if (r == "PIPIIPC") AddColor(Color.White);
            else if (r == "PIPIIPF") AddColor(0);
            else if (r == "PIPIIPP") AddColor(255);
            else if (r == "PIIPICP") ClearColors();
            else if (r == "PIIIIIP") pos = Move(pos, dir);
            else if (r == "PCCCCCP") dir = TurnCounterClockwise(dir);
            else if (r == "PFFFFFP") dir = TurnClockwise(dir);
            else if (r == "PCCIFFP") mark = pos;
            else if (r == "PFFICCP") Line(pos, mark);
            else if (r == "PIIPIIP") Tryfill();
            else if (r == "PCCPFFP") AddBitmap(new Bmp());
            else if (r == "PFFPCCP") Compose();
            else if (r == "PFFICCF") Clip();
            else IncCounter("bad");
        }

        private void ClearColors()
        {
            IncCounter("clear");
            Log("clear");
            bucket.Clear();
        }

        public string GetCountersData()
        {
            return counters.OrderBy(c => c.Key)
                .Select(kv => kv.Key + ":" + kv.Value)
                .ToDelimitedString(" ");
        }

        private void IncCounter(string name)
        {
            counters[name] = counters.GetOrDefault(name, 0) + 1;
        }

        private void AddColor(int alpha)
        {
            Log($"add {alpha}");
            IncCounter("addA");
            bucket.AddColor(alpha);
        }

        private void AddColor(Color color)
        {
            Log($"add {color}");
            IncCounter("addC");
            bucket.AddColor(color);
        }

        private void Clip()
        {
            Changed();
            Log($"clip");
            if (bitmaps.Count >= 2)
            {
                IncCounter("clip");
                var b0 = bitmaps.Pop();
                var b1 = bitmaps.Peek();
                for (var x = 0; x < 600; x++)
                    for (var y = 0; y < 600; y++)
                    {
                        var c1 = b1.Pixels[x, y];
                        var c0 = b0.Pixels[x, y];

                        b1.Pixels[x, y] = Color.FromArgb(
                            c1.A * c0.A / 255,
                            c1.R * c0.A / 255,
                            c1.G * c0.A / 255,
                            c1.B * c0.A / 255
                        );
                    }
            }
        }


        private void Compose()
        {
            Log("compose");
            Changed();
            if (bitmaps.Count >= 2)
            {
                IncCounter("compose");
                var b0 = bitmaps.Pop();
                var b1 = bitmaps.Peek();
                for (var x = 0; x < 600; x++)
                    for (var y = 0; y < 600; y++)
                    {
                        var c1 = b1.Pixels[x, y];
                        var c0 = b0.Pixels[x, y];
                        b1.Pixels[x, y] = Color.FromArgb(
                            c0.A + c1.A * (255 - c0.A) / 255,
                            c0.R + c1.R * (255 - c0.A) / 255,
                            c0.G + c1.G * (255 - c0.A) / 255,
                            c0.B + c1.B * (255 - c0.A) / 255
                        );
                    }
            }
        }

        private void Tryfill()
        {
            Log($"fill");
            var newColor = bucket.CurrentPixel();
            var oldColor = GetPixel(pos.X, pos.Y);
            if (newColor != oldColor) Fill(pos.X, pos.Y, oldColor);
        }

        private void Fill(int x, int y, Color initial)
        {
            Changed();
            IncCounter("fill");
            var q = new Queue<Tuple<int, int>>();
            q.Enqueue(Tuple.Create(x, y));
            SetPixel(x, y);
            while (q.Any())
            {
                var xy = q.Dequeue();
                x = xy.Item1;
                y = xy.Item2;
                if (x > 0 && GetPixel(x - 1, y) == initial)
                {
                    q.Enqueue(Tuple.Create(x - 1, y));
                    SetPixel(x - 1, y);
                }
                if (x < 599 && GetPixel(x + 1, y) == initial)
                {
                    q.Enqueue(Tuple.Create(x + 1, y));
                    SetPixel(x + 1, y);
                }
                if (y > 0 && GetPixel(x, y - 1) == initial)
                {
                    q.Enqueue(Tuple.Create(x, y - 1));
                    SetPixel(x, y - 1);
                }
                if (y < 599 && GetPixel(x, y + 1) == initial)
                {
                    q.Enqueue(Tuple.Create(x, y + 1));
                    SetPixel(x, y + 1);
                }
            }
            Changed();
        }

        private void Changed()
        {
            if (OnImageChange == null) return;
            var pixels = bitmaps.Peek().Pixels;
            for (var x = 0; x < 600; x++)
                for (var y = 0; y < 600; y++)
                {
                    var p = pixels[x,y];
                    if (p.R > 0 || p.G > 0 || p.B > 0)
                    {
                        OnImageChange(this);
                        return;
                    }
                }
        }

        private Color GetPixel(int x, int y)
        {
            return bitmaps.Peek().Pixels[x, y];
        }

        private void SetPixel(int x, int y)
        {
            bitmaps.Peek().Pixels[x, y] = bucket.CurrentPixel();
        }

        private void Line(Pos p0, Pos p1)
        {
            Log("line");
            IncCounter("line");
            //Log($"{p0.X},{p0.Y} - {p1.X},{p1.Y}");
            var dx = p1.X - p0.X;
            var dy = p1.Y - p0.Y;
            var d = Math.Max(Math.Abs(dx), Math.Abs(dy));
            var c = dx * dy <= 0 ? 1 : 0;
            var x = p0.X * d + (d - c) / 2;
            var y = p0.Y * d + (d - c) / 2;

            for (var i = 0; i < d; i++)
            {
                SetPixel(x / d, y / d);
                x += dx;
                y += dy;
            }
            SetPixel(p1.X, p1.Y);
        }

        private void AddBitmap(Bmp bmp)
        {
            Changed();
            Log("bmp");
            if (bitmaps.Count < 10)
            {
                IncCounter("addBmp");
                bitmaps.Push(bmp);
            }
        }

        private Dir TurnCounterClockwise(Dir d)
        {
            Log("ccw");
            IncCounter("ccw");
            switch (d)
            {
                case Dir.N: return Dir.W;
                case Dir.E: return Dir.N;
                case Dir.S: return Dir.E;
                case Dir.W: return Dir.S;
                default: throw new Exception(d.ToString());
            }
        }

        private Dir TurnClockwise(Dir d)
        {
            Log("cw");
            IncCounter("cw");
            switch (d)
            {
                case Dir.N: return Dir.E;
                case Dir.E: return Dir.S;
                case Dir.S: return Dir.W;
                case Dir.W: return Dir.N;
                default: throw new Exception(d.ToString());
            }
        }

        private Pos Move(Pos pos, Dir d)
        {
            Log("move");
            IncCounter("move");
            switch (d)
            {
                case Dir.N: return new Pos(pos.X, (pos.Y - 1 + 600) % 600);
                case Dir.E: return new Pos((pos.X + 1) % 600, pos.Y);
                case Dir.S: return new Pos(pos.X, (pos.Y + 1) % 600);
                case Dir.W: return new Pos((pos.X - 1 + 600) % 600, pos.Y);
                default: throw new Exception(d.ToString());
            }
        }
    }

    [TestFixture]
    public class RnaProcessor_Should
    {
        private string rnaFile() => TestContext.CurrentContext.TestDirectory + "\\rna.dna";

        [Test, Explicit]
        public void Process()
        {
            var vm = new Vm();
            var spec2Prefix = "IIPIFFCPICFPPICIICCIICIPPPFIIC";

            var specPrefix = "IIPIFFCPICICIICPIICIPPPICIIC";
            vm.Load(specPrefix, @"c:\work\contests\icfpc2017\lib\endo.dna");

            var proc = new RnaProcessor();
            var i = 0;
            foreach (var rna in vm.Execute())
            {
                proc.Step(rna);
                if (i % 100 == 0)
                    proc.ToBitmap().Save(rnaFile() + i + ".bmp");
                i++;
            }
            proc.ToBitmap().Save(rnaFile() + i + ".bmp");
            Console.WriteLine(vm.rna.Count);

            vm.SaveRnas(rnaFile(), vm.rna);
        }

        [Test]
        public void Visuzlize()
        {
            var rna = File.ReadLines(rnaFile()).Select(line => (Dna)line).ToList();
            var proc = new RnaProcessor();
            var i = 0;
            foreach (var r in rna)
            {
                proc.Step(r);
                if (i % 100 == 0)
                    proc.ToBitmap().Save(rnaFile() + i + ".bmp");
                i++;
            }
            proc.ToBitmap().Save(rnaFile() + i + ".bmp");
        }
    }
}