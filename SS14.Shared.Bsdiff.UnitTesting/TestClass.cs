using NUnit.Framework;
using System.Linq;

namespace SS14.Shared.Bsdiff.UnitTesting
{
    [TestFixture]
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
    }
}
