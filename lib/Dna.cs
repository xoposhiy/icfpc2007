using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace lib
{
    public class Dna
    {
        public const string AdapterMarker = "IFPICFPPCCC";
        public const string BlueZoneStartMarker = "IFPICFPPCFIPP";
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
}