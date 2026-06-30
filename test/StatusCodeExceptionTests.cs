using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace GuguEveryday.Tests;

[TestClass]
public class StatusCodeExceptionTests
{
    [TestMethod]
    public void Constructor_SetsMessageAndDefaultStatusCode()
    {
        var ex = new StatusCodeException("出错了");

        Assert.AreEqual("出错了", ex.Message);
        Assert.AreEqual(400, ex.HttpStatusCode);
    }

    [TestMethod]
    public void Constructor_SetsCustomStatusCode()
    {
        var ex = new StatusCodeException("未找到", 404);

        Assert.AreEqual(404, ex.HttpStatusCode);
    }

    [TestMethod]
    public void ImplementsIHasHttpStatusCode()
    {
        var ex = new StatusCodeException("test");

        Assert.IsInstanceOfType<Volo.Abp.ExceptionHandling.IHasHttpStatusCode>(ex);
    }

    [TestMethod]
    public void IsUserFriendlyException()
    {
        var ex = new StatusCodeException("test");

        Assert.IsInstanceOfType<Volo.Abp.UserFriendlyException>(ex);
    }
}
