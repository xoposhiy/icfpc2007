using System;
using System.Collections.Generic;
using System.Linq;

namespace lib
{
    public class DnaReader : IDisposable
    {
        private int count = 0;
        private Dna dna;
        private readonly Action<Dna, Dna> finalize;
        private IEnumerator<byte> enumerator;
        private Stack<byte> backStack = new Stack<byte>();

        public DnaReader(Dna dna)
        {
            this.dna = dna;
            enumerator = dna.Enumerate().GetEnumerator();
        }
        public DnaReader(Dna dna, Action<Dna, Dna> finalize)
        {
            this.dna = dna;
            this.finalize = finalize;
            enumerator = dna.Enumerate().GetEnumerator();
        }

        public Dna Dna => dna;

        public void Dispose()
        {
            var parsedHead = dna.Substring(0, count);
            dna = dna.Substring(count);
            finalize?.Invoke(parsedHead, dna);
            enumerator.Dispose();
        }

        public List<byte> readBytes = new List<byte>();
        public byte? Read()
        {
            if (backStack.Any())
            {
                var v = backStack.Pop();
                readBytes.Add(v);
                return v;
            }
            if (!enumerator.MoveNext()) return null;
            count++;
            readBytes.Add(enumerator.Current);
            return enumerator.Current;
        }

        public void ClearPrev(int count)
        {
            readBytes.RemoveRange(readBytes.Count-count, count);
        }

        public void PushBack(byte v)
        {
            readBytes.RemoveAt(readBytes.Count - 1);
            backStack.Push(v);
        }
    }
}