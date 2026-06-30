using Microsoft.VisualStudio.TestTools.UnitTesting;
using GuguEveryday.Services;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Net;

namespace GuguEveryday.Tests.Services;

[TestClass]
public class IpLimitServiceTests
{
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    private readonly InMemoryDistributedCache _cache = new();
    private readonly IpLimitService _sut;

    public IpLimitServiceTests()
    {
        _sut = new IpLimitService(_httpContextAccessor, _cache);
        SetupIp("192.168.1.1");
    }

    private void SetupIp(string ip)
    {
        var connection = Substitute.For<ConnectionInfo>();
        connection.RemoteIpAddress.Returns(IPAddress.Parse(ip));
        var context = Substitute.For<HttpContext>();
        context.Connection.Returns(connection);
        _httpContextAccessor.HttpContext.Returns(context);
    }

    [TestMethod]
    public async Task CheckAsync_FirstCallSucceeds()
    {
        await _sut.CheckAsync("key1");
    }

    [TestMethod]
    public async Task CheckAsync_StoresCountInCache()
    {
        await _sut.CheckAsync("key1");

        var cached = await _cache.GetStringAsync($"ipLimit:192.168.1.1:key1");
        Assert.AreEqual("1", cached);
    }

    [TestMethod]
    public async Task CheckAsync_AllowsUptoLimit()
    {
        await _sut.CheckAsync("key1");
        await _sut.CheckAsync("key1");
        await _sut.CheckAsync("key1");
    }

    [TestMethod]
    public async Task CheckAsync_ThrowsWhenExceedingLimit()
    {
        await _sut.CheckAsync("key1");
        await _sut.CheckAsync("key1");
        await _sut.CheckAsync("key1");

        var ex = await Assert.ThrowsExceptionAsync<StatusCodeException>(
            () => _sut.CheckAsync("key1"));

        Assert.IsTrue(ex.Message.Contains("超过限制"));
    }

    [TestMethod]
    public async Task CheckAsync_UsesCustomErrorMessage()
    {
        await _sut.CheckAsync("key1");
        await _sut.CheckAsync("key1");
        await _sut.CheckAsync("key1");

        var ex = await Assert.ThrowsExceptionAsync<StatusCodeException>(
            () => _sut.CheckAsync("key1", "自定义错误"));

        Assert.AreEqual("自定义错误", ex.Message);
    }
}
