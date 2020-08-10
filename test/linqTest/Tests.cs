namespace linqTest
{
    using System.Linq;
    using IotHub.linq;
    using NUnit.Framework;

    public class Tests
    {
        [Test]
        public void SkipTest()
        {
            var arr = new[] {0, 1, 2, 3, 4};
            Assert.AreEqual(arr.Skip(1).ToArray(), LinqEx.Skip(arr, 1));
        }
        [Test]
        public void FirstTest()
        {
            var arr = new[] {0, 1, 2, 3, 4};
            Assert.AreEqual(arr.First(), LinqEx.First(arr));
        }
        [Test]
        public void AllTest()
        {
            var arr = new[] {0, 1, 2, 3, 4};
            Assert.AreEqual(arr.All(x => x > -1), LinqEx.All(arr, x => x > -1));
            Assert.AreEqual(arr.All(x => x < -1), LinqEx.All(arr, x => x < -1));
        }
    }
}