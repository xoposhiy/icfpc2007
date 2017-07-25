using System.Drawing;
using NUnit.Framework;
using Shouldly;

namespace lib
{
    public class ColorsBucket
    {
        public int Alpha;

        private int alphaCount;
        public int B;
        private int colorsCount;
        public int G;
        public int R;
        private Color? lastColor;

        public void Clear()
        {
            R = 0;
            G = 0;
            B = 0;
            Alpha = 0;
            colorsCount = 0;
            alphaCount = 0;
            lastColor = null;
        }

        public ColorsBucket AddColor(Color color)
        {
            R += color.R;
            G += color.G;
            B += color.B;
            colorsCount++;
            lastColor = null;
            return this;
        }

        public ColorsBucket AddColor(int alpha)
        {
            Alpha += alpha;
            alphaCount++;
            lastColor = null;
            return this;
        }


        public Color CurrentPixel()
        {
            if (lastColor.HasValue)
                return lastColor.Value;
            var a = Avg(Alpha, alphaCount, 255);
            var value = Color.FromArgb(
                a,
                Avg(R, colorsCount, 0) * a / 255,
                Avg(G, colorsCount, 0) * a / 255,
                Avg(B, colorsCount, 0) * a / 255);
            lastColor = value;
            return value;
        }

        private static int Avg(int sum, int count, int defaultValue)
        {
            return count == 0 ? defaultValue : sum / count;
        }
    }

    [TestFixture]
    public class ColorsBucket_Should
    {
        [Test]
        public void AvgAlpha()
        {
            var bucket = new ColorsBucket();
            bucket.CurrentPixel();
            bucket.AddColor(0).AddColor(255).AddColor(255);
            bucket.CurrentPixel()
                .ShouldBe(Color.FromArgb(170, 0, 0, 0));
        }

        [Test]
        public void AvgColor()
        {
            var bucket = new ColorsBucket();
            bucket.AddColor(Color.Black).AddColor(Color.Yellow).AddColor(Color.Cyan);
            bucket.CurrentPixel()
                .ShouldBe(Color.FromArgb(255, 85, 170, 85));
        }

        [Test]
        public void AvgColorAndAlpha()
        {
            var bucket = new ColorsBucket();
            bucket.AddColor(Color.Yellow).AddColor(255).AddColor(0);
            bucket.CurrentPixel()
                .ShouldBe(Color.FromArgb(127, 127, 127, 0));
        }

        [Test]
        public void DefaultColor()
        {
            var bucket = new ColorsBucket();
            bucket.CurrentPixel()
                .ShouldBe(Color.FromArgb(255, 0, 0, 0));
        }
    }
}