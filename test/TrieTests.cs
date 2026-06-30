using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace GuguEveryday.Tests;

[TestClass]
public class TrieTests
{
    [TestMethod]
    public void Find_MatchesSingleWord()
    {
        var trie = new Trie();
        trie.Add("敏感词");
        trie.Build();

        var results = trie.Find("这是一段包含敏感词的文本").ToList();

        Assert.AreEqual(1, (results).Count());
        Assert.AreEqual("敏感词", results[0]);
    }

    [TestMethod]
    public void Find_MatchesMultipleWords()
    {
        var trie = new Trie();
        trie.Add("苹果");
        trie.Add("香蕉");
        trie.Build();

        var results = trie.Find("我喜欢苹果和香蕉").ToList();

        Assert.AreEqual(2, results.Count);
        Assert.IsTrue((results).Contains("苹果"));
        Assert.IsTrue((results).Contains("香蕉"));
    }

    [TestMethod]
    public void Find_NoMatch_ReturnsEmpty()
    {
        var trie = new Trie();
        trie.Add("敏感词");
        trie.Build();

        var results = trie.Find("这是一段正常的文本").ToList();

        Assert.AreEqual(0, (results).Count());
    }

    [TestMethod]
    public void Find_MatchesOverlappingWords()
    {
        var trie = new Trie();
        trie.Add("abc");
        trie.Add("bcd");
        trie.Build();

        var results = trie.Find("abcde").ToList();

        Assert.IsTrue((results).Contains("abc"));
        Assert.IsTrue((results).Contains("bcd"));
    }

    [TestMethod]
    public void Find_MatchesAtStart()
    {
        var trie = new Trie();
        trie.Add("开头");
        trie.Build();

        var results = trie.Find("开头就在最前面").ToList();

        Assert.AreEqual(1, (results).Count());
        Assert.AreEqual("开头", results[0]);
    }

    [TestMethod]
    public void Find_MatchesAtEnd()
    {
        var trie = new Trie();
        trie.Add("结尾");
        trie.Build();

        var results = trie.Find("最后面是结尾").ToList();

        Assert.AreEqual(1, (results).Count());
        Assert.AreEqual("结尾", results[0]);
    }

    [TestMethod]
    public void Find_MatchesEntireString()
    {
        var trie = new Trie();
        trie.Add("完全匹配");
        trie.Build();

        var results = trie.Find("完全匹配").ToList();

        Assert.AreEqual(1, (results).Count());
        Assert.AreEqual("完全匹配", results[0]);
    }

    [TestMethod]
    public void Find_MatchesRepeatedWord()
    {
        var trie = new Trie();
        trie.Add("重复");
        trie.Build();

        var results = trie.Find("重复重复重复").ToList();

        Assert.AreEqual(3, results.Count);
    }

    [TestMethod]
    public void Add_MultipleStringsAtOnce()
    {
        var trie = new Trie();
        trie.Add(new[] { "词1", "词2", "词3" });
        trie.Build();

        var results = trie.Find("包含词1和词2还有词3").ToList();

        Assert.AreEqual(3, results.Count);
    }

    [TestMethod]
    public void Find_EmptyText_ReturnsEmpty()
    {
        var trie = new Trie();
        trie.Add("test");
        trie.Build();

        var results = trie.Find("").ToList();

        Assert.AreEqual(0, (results).Count());
    }

    [TestMethod]
    public void Build_CalledTwice_Works()
    {
        var trie = new Trie();
        trie.Add("first");
        trie.Build();
        trie.Add("second");
        trie.Build();

        var results = trie.Find("first and second").ToList();

        Assert.IsTrue((results).Contains("first"));
        Assert.IsTrue((results).Contains("second"));
    }
}
