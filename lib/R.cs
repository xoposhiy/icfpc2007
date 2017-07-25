using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace lib
{
    public class R<T>
    {
        private readonly RopeNode<T> root;

        public static implicit operator R<T>(T[] data) => new R<T>(data);

        public R(IEnumerable<T> data)
            : this(new BulkNode<T>(data.ToArray()))
        {
        }

        public R(T[] data)
            : this(new BulkNode<T>(data))
        {
        }

        private R(RopeNode<T> root)
        {
            this.root = root;
        }

        public int Length => root.Length;
        public int Height => root.Height;
        public int NodesCount => root.NodesCount;

        public IEnumerable<T> Enumerate()
        {
            return root.Enumerate();
        }

        public R<T> Substring(int startIndex, int length)
        {
            var newRoot = root.Substring(startIndex, length);
            return new R<T>(newRoot);
        }

        public T this[int index] => root.Get(index);

        public T[] ToArray()
        {
            var res = new T[root.Length];
            root.CopyTo(res, 0);
            return res;
        }

        public R<T> Concat(R<T> other)
        {
            var left = root;
            var right = other.root;
            return new R<T>(Concat(left, right));
        }

        public static RopeNode<T> Concat(RopeNode<T> left, RopeNode<T> right)
        {
            if (left.Length == 0) return right;
            if (right.Length == 0) return left;
            RopeNode<T> concatNode = new ConcatNode<T>(left, right);
            var totalLen = left.Length + right.Length;
            if (totalLen >= JoinThreshold) return AutoRebalance(concatNode);
            var res = new T[totalLen];
            concatNode.CopyTo(res, 0);
            return new BulkNode<T>(res);
        }

        private static RopeNode<T> AutoRebalance(RopeNode<T> node)
        {
            if (node.Height > RebalanceHeightThreshold)
            {
                return Rebalance(node);
            }
            else
            {
                return node;
            }
        }

        private static int RebalanceHeightThreshold = 80;

        public static RopeNode<T> Rebalance(RopeNode<T> node)
        {
            //Console.WriteLine("rebalance node: " + node.Height + " " + node.NodesCount);
            var leafs = new List<RopeNode<T>>();
            var toExamine = new Stack<RopeNode<T>>();
            toExamine.Push(node);
            while (toExamine.Count > 0)
            {
                var x = toExamine.Pop();
                if (x is ConcatNode<T> concatX)
                {
                    toExamine.Push(concatX.Right);
                    toExamine.Push(concatX.Left);
                }
                else
                {
                    leafs.Add(x);
                }
            }
            var res = Merge(leafs, 0, leafs.Count);
            //Console.WriteLine(res.Height + " " + res.NodesCount);
            return res;
        }

        private static RopeNode<T> Merge(List<RopeNode<T>> leafNodes, int start, int end)
        {
            int range = end - start;
            switch (range)
            {
                case 1:
                    return leafNodes[start];
                case 2:
                    return Concat(leafNodes[start], leafNodes[start + 1]);
                default:
                    int middle = start + (range / 2);
                    return Concat(Merge(leafNodes, start, middle), Merge(leafNodes, middle, end));
            }
        }

        public static int JoinThreshold = 17;

        public IEnumerable<T> EnumerateFrom(int start)
        {
            return root.EnumerateFrom(start);
        }
    }

    public class ConcatNode<T> : RopeNode<T>
    {
        public RopeNode<T> Left { get; }
        public RopeNode<T> Right { get; }

        public ConcatNode(RopeNode<T> left, RopeNode<T> right)
        {
            Left = left;
            Right = right;
            Length = Left.Length + Right.Length;
            Height = Math.Max(Left.Height, Right.Height) + 1;
            NodesCount = 1 + Left.NodesCount + Right.NodesCount;
        }

        public override int Length { get; }
        public override int Height { get; }
        public override int NodesCount { get; }

        public override void CopyTo(T[] destination, int destinationStart)
        {
            Left.CopyTo(destination, destinationStart);
            Right.CopyTo(destination, destinationStart + Left.Length);
        }

        public override RopeNode<T> Substring(int startIndex, int length)
        {
            if (length == 0) return Empty;
            if (startIndex == 0 && length == Length) return this;
            if (startIndex >= Left.Length)
                return Right.Substring(startIndex - Left.Length, length);
            if (startIndex + length <= Left.Length)
                return Left.Substring(startIndex, length);
            var leftCount = Left.Length - startIndex;
            var rightCount = length - leftCount;
            return R<T>.Concat(
                Left.Substring(startIndex, leftCount),
                Right.Substring(0, rightCount));
        }

        public override T Get(int index)
        {
            if (index < Left.Length) return Left.Get(index);
            return Right.Get(index - Left.Length);
        }

        public override IEnumerable<T> Enumerate()
        {
            foreach (var b in Left.Enumerate())
                yield return b;
            foreach (var b in Right.Enumerate())
                yield return b;
        }

        public override IEnumerable<T> EnumerateFrom(int start)
        {
            if (start < Left.Length)
                return Left.EnumerateFrom(start).Concat(Right.Enumerate());
            else return Right.EnumerateFrom(start - Left.Length);
        }
    }

    public abstract class RopeNode<T>
    {
        public abstract int Length { get; }
        public abstract int Height { get; }
        public abstract int NodesCount { get; }
        public static RopeNode<T> Empty = new BulkNode<T>(new T[0]);

        public abstract void CopyTo(T[] destination, int destinationStart);
        public abstract RopeNode<T> Substring(int startIndex, int length);
        public abstract T Get(int index);

        public abstract IEnumerable<T> Enumerate();

        public abstract IEnumerable<T> EnumerateFrom(int start);
    }

    public class BulkNode<T> : RopeNode<T>
    {
        public readonly int StartIndex;
        public T[] Bulk { get; }

        public BulkNode(T[] bulk)
            : this(bulk, 0, bulk.Length)
        {
        }

        private BulkNode(T[] bulk, int startIndex, int length)
        {
            if (startIndex + length > bulk.Length)
                throw new ArgumentException($"{startIndex} + {length} > {bulk.Length}");
            Bulk = bulk;
            StartIndex = startIndex;
            Length = length;
        }

        public override int Length { get; }
        public override int Height => 1;
        public override int NodesCount => 1;

        public override void CopyTo(T[] destination, int destinationStart)
        {
            Array.Copy(Bulk, StartIndex, destination, destinationStart, Length);
        }

        public override RopeNode<T> Substring(int startIndex, int length)
        {
            if (length == 0) return Empty;
            if (StartIndex + startIndex + length > Bulk.Length)
                throw new ArgumentException();
            return new BulkNode<T>(Bulk, StartIndex + startIndex, length);
        }

        public override T Get(int index)
        {
            return Bulk[StartIndex + index];
        }

        public override IEnumerable<T> Enumerate()
        {
            for (int i = StartIndex; i < StartIndex + Length; i++)
                yield return Bulk[i];
        }

        public override IEnumerable<T> EnumerateFrom(int start)
        {
            for (int i = StartIndex + start; i < StartIndex + Length; i++)
                yield return Bulk[i];
        }
    }

    [TestFixture]
    public class Rope2_Tests
    {
        [Test]
        public void CreateEmptyRope()
        {
            var rope = new R<byte>(new byte[0]);
            var res = rope.ToArray();
            res.ShouldBe(new byte[0]);
        }

        [TestCase(new byte[] { 1 })]
        [TestCase(new byte[] { 1, 2, 3 })]
        public void CreateBulkRope(byte[] bulk)
        {
            var rope = new R<byte>(bulk);
            var res = rope.ToArray();
            res.ShouldBe(bulk);
        }

        [TestCase("ab", 0, 2, "ab")]
        [TestCase("abcde", 1, 2, "bc")]
        [TestCase("abcde", 0, 2, "ab")]
        [TestCase("abcde", 4, 1, "e")]
        [TestCase("abcde", 4, 0, "")]
        public void BulkSubstring(string input, int start, int len, string expected)
        {
            var rope = new R<char>(input.ToCharArray());
            var res = rope.Substring(start, len);
            new string(res.ToArray())
                .ShouldBe(expected);
        }

        [TestCase("abcde", 1, 3, 1, 1, "c")]
        [TestCase("abcde", 0, 5, 0, 5, "abcde")]
        public void SubstringSubstring(string input, int start1, int len1, int start2, int len2, string expected)
        {
            var rope = new R<char>(input.ToCharArray());
            var res = rope.Substring(start1, len1)
                .Substring(start2, len2);
            new string(res.ToArray())
                .ShouldBe(expected);
        }

        [TestCase("abc", "def")]
        [TestCase("", "def")]
        [TestCase("abc", "")]
        [TestCase("z", "a")]
        [TestCase("", "")]
        public void BulkConcat(string left, string right)
        {
            var a = new R<char>(left);
            var b = new R<char>(right);
            var res = a.Concat(b);
            new string(res.ToArray())
                .ShouldBe(left + right);
        }

        [TestCase("abc", "def", 0, 1, "a")]
        [TestCase("abc", "def", 0, 0, "")]
        [TestCase("abc", "def", 0, 3, "abc")]
        [TestCase("abc", "def", 0, 4, "abcd")]
        [TestCase("abc", "def", 0, 6, "abcdef")]
        [TestCase("abc", "def", 1, 5, "bcdef")]
        [TestCase("abc", "def", 2, 2, "cd")]
        [TestCase("abc", "def", 3, 1, "d")]
        [TestCase("abc", "def", 3, 3, "def")]
        public void SubstringOfConcat(string left, string right, int start, int len, string expected)
        {
            var a = new R<char>(left);
            var b = new R<char>(right);
            var res = a.Concat(b).Substring(start, len);
            new string(res.ToArray())
                .ShouldBe(expected);
        }

        [TestCase("a", "b", 1)]
        public void SmallConcat(string left, string right, int expectedCount)
        {
            var a = new R<char>(left);
            var b = new R<char>(right);
            var res = a.Concat(b);
            res.NodesCount
                .ShouldBe(expectedCount);
        }
    }
}