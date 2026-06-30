using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace GuguEveryday.Tests;

[TestClass]
public class Snowflake16Tests
{
    [TestMethod]
    public void NextId_ReturnsPositiveValue()
    {
        var sut = new Snowflake16();
        var id = sut.NextId();

        Assert.IsTrue(id > 0);
    }

    [TestMethod]
    public void NextId_ReturnsUniqueValues()
    {
        var sut = new Snowflake16();
        var ids = Enumerable.Range(0, 1000).Select(_ => sut.NextId()).ToList();

        Assert.AreEqual(1000, ids.Distinct().Count());
    }

    [TestMethod]
    public void NextId_ReturnsMonotonicallyIncreasing()
    {
        var sut = new Snowflake16();
        var ids = Enumerable.Range(0, 100).Select(_ => sut.NextId()).ToList();

        for (int i = 1; i < ids.Count; i++)
        {
            Assert.IsTrue(ids[i] > ids[i - 1]);
        }
    }

    [TestMethod]
    public void NextId_ConcurrentCallsProduceUniqueIds()
    {
        var sut = new Snowflake16();
        var ids = new System.Collections.Concurrent.ConcurrentBag<long>();

        Parallel.For(0, 1000, _ => ids.Add(sut.NextId()));

        Assert.AreEqual(1000, ids.Distinct().Count());
    }

    [TestMethod]
    public void NextId_Returns15DigitId()
    {
        var sut = new Snowflake16();
        var id = sut.NextId();

        Assert.IsTrue(id.ToString().Length <= 15, $"ID length was {id.ToString().Length}");
    }
}
