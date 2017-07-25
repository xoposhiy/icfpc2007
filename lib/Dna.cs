using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace lib
{
    public class Dna
    {
        public const byte I = 0;
        public const byte C = 1;
        public const byte F = 2;
        public const byte P = 3;
        public static Dna Empty = new Dna(new byte[0]);
        public R<byte> root;

        public Dna(byte[] data)
        {
            root = new R<byte>(data);
        }

        public Dna(R<byte> root)
        {
            this.root = root;
        }

        public byte this[int index] => root[index];

        public int Len => root.Length;

        public static implicit operator Dna(string s)
        {
            return new Dna(FromString(s));
        }

        public static implicit operator Dna(byte[] dna)
        {
            return new Dna(dna);
        }

        public static implicit operator Dna(List<byte> dna)
        {
            return new Dna(dna.ToArray());
        }

        public static byte[] FromString(string s)
        {
            return s.Select(FromChar).ToArray();
        }

        public static byte FromChar(char c)
        {
            switch (c)
            {
                case 'I': return 0;
                case 'C': return 1;
                case 'F': return 2;
                case 'P': return 3;
                default: throw new FormatException(c.ToString());
            }
        }

        public Dna StrictSubstring(int start, int length)
        {
            if (length == 0) return Empty;
            return new Dna(root.Substring(start, length));
        }

        public Dna Append(Dna other)
        {
            return new Dna(root.Concat(other.root));
        }

        public IEnumerable<byte> Enumerate()
        {
            return root.Enumerate();
        }

        public override string ToString()
        {
            return new string(root.Enumerate().Select(b => "ICFP"[b]).ToArray());
        }

        public static Dna Load(string prefix, string filename)
        {
            var dnaString = File.ReadAllText(filename);
            var body = new Dna(Dna.FromString(dnaString));
            return new Dna(Dna.FromString(prefix)).Append(body);
        }

        public IEnumerable<byte> EnumerateFromIndex(int start)
        {
            return root.EnumerateFrom(start);
        }
    }
    

    public class Treap
    {
        public Treap Left;
        public Treap Right;
        public int Size = 1;
        public int Height = 1;
        public byte Sym;
        private int y;

        public Treap(int y, byte sym, Treap left = null, Treap right = null)
        {
            this.y = y;
            Sym = sym;
            Left = left;
            Right = right;
        }

        public byte this[int index]
        {
            get
            {
                var cur = this;
                while (cur != null)
                {
                    var sizeLeft = SizeOf(cur.Left);

                    if (sizeLeft == index)
                        return cur.Sym;

                    cur = sizeLeft > index ? cur.Left : cur.Right;
                    if (sizeLeft < index)
                        index -= sizeLeft + 1;
                }
                throw new IndexOutOfRangeException();
            }
        }

        public static int SizeOf(Treap treap)
        {
            return treap == null ? 0 : treap.Size;
        }
        public static int HeightOf(Treap treap)
        {
            return treap == null ? 0 : treap.Height;
        }

        public void Recalc()
        {
            Size = 1 + SizeOf(Left) + SizeOf(Right);
            Height = 1 + Math.Max(HeightOf(Left), HeightOf(Right));
        }

        public void Split(int x, out Treap left, out Treap right)
        {
            Treap newTree = null;
            var curIndex = SizeOf(Left) + 1;

            if (curIndex <= x)
            {
                if (Right == null)
                    right = null;
                else
                    Right.Split(x - curIndex, out newTree, out right);
                left = new Treap(y, Sym, Left, newTree);
                left.Recalc();
            }
            else
            {
                if (Left == null)
                    left = null;
                else
                    Left.Split(x, out left, out newTree);
                right = new Treap(y, Sym, newTree, Right);
                right.Recalc();
            }
        }

        public static Treap Merge(Treap left, Treap right)
        {
            if (left == null) return right;
            if (right == null) return left;
            Treap ans;
            if (left.y > right.y)
            {
                var newR = Merge(left.Right, right);
                ans = new Treap(left.y, left.Sym, left.Left, newR);
            }
            else
            {
                var newL = Merge(left, right.Left);
                ans = new Treap(right.y, right.Sym, newL, right.Right);
            }
            ans.Recalc();
            return ans;
        }
    }

    public static class TreapExtensions
    {
        private static Random rand = new Random();

        public static IEnumerable<byte> Enumerate(this Treap treap)
        {
            if (treap == null) yield break;
            foreach (var b in treap.Left.Enumerate())
                yield return b;
            yield return treap.Sym;
            foreach (var b in treap.Right.Enumerate())
                yield return b;
        }

        public static int GetHeight(this Treap treap)
        {
            return treap == null ? 0 : treap.Height;
        }

        public static Treap Append(this Treap treap, byte value)
        {
            var m = new Treap(rand.Next(), value);
            return Treap.Merge(treap, m);
        }
    }

    [TestFixture]
    public class Treap_Should
    {
        [Test]
        public void LoadAndEnumerate()
        {
            var sw = Stopwatch.StartNew();
            var bytes = Dna.FromString(File.ReadAllText(@"c:\work\contests\icfpc2017\endo\bin\Debug\endo.dna"));
            Console.WriteLine("loaded " + sw.Elapsed);
            sw.Restart();
            var dna = new Dna(bytes);
            Console.WriteLine("treap built " + sw.Elapsed);
            sw.Restart();
            var c = dna.Enumerate().Count();
            Console.WriteLine("enumerated " + sw.Elapsed);
            Console.WriteLine(c);
        }
    }
}