using NUnit.Framework;
using System.Linq;
using System;

namespace SS14.Shared.Bsdiff.UnitTesting
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    [TestOf(typeof(Bsdiff))]
    public class TestClass
    {
        [Test]
        public void TestMethod()
        {
            var bytes = new byte[] { 0, 1, 2, 3, 5 };
            var bytes2 = new byte[] { 0, 1, 2, 3, 10 };

            var patch = Bsdiff.GenerateBzip2Diff(bytes, bytes2);
            var result = Bsdiff.ApplyBzip2Patch(bytes, patch);

            bool equals = result.OrderBy(a => a).SequenceEqual(bytes2.OrderBy(a => a));
            Assert.True(equals);
        }

        [Test]
        [Repeat(10)]
        public void TestNoise()
        {
            var random = new Random();
            var bytes = new byte[1024];
            random.NextBytes(bytes);
            var bytes2 = new byte[1024];
            bytes.CopyTo(bytes2, 0);

            for (var i = 0; i < bytes2.Length; i++)
            {
                if (random.NextDouble() > 0.5)
                {
                    bytes2[i] = (byte)random.Next();
                }
            }

            var patch = Bsdiff.GenerateBzip2Diff(bytes, bytes2);
            var result = Bsdiff.ApplyBzip2Patch(bytes, patch);

            bool equals = result.OrderBy(a => a).SequenceEqual(bytes2.OrderBy(a => a));
            Assert.True(equals);
        }
    }
}
