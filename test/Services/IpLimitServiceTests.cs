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

    // --- CheckAsync ---

    [TestMethod]
    public async Task CheckAsync_FirstCallSucceeds()
    {
        await _sut.CheckAsync("login", "user@test.com");
    }

    [TestMethod]
    public async Task CheckAsync_ThrowsWhenLimitReached()
    {
        await _cache.SetStringAsync("ipLimit:192.168.1.1:login:user@test.com", "-1", new());

        var ex = await Assert.ThrowsExceptionAsync<StatusCodeException>(
            () => _sut.CheckAsync("login", "user@test.com"));

        Assert.AreEqual(429, ex.HttpStatusCode);
    }

    [TestMethod]
    public async Task CheckAsync_ThrowsWhenBlocked()
    {
        await _cache.SetStringAsync("ipBlock:192.168.1.1:login:user@test.com", "1", new());

        var ex = await Assert.ThrowsExceptionAsync<StatusCodeException>(
            () => _sut.CheckAsync("login", "user@test.com", null, TimeSpan.FromMinutes(10)));

        Assert.AreEqual(429, ex.HttpStatusCode);
    }

    [TestMethod]
    public async Task CheckAsync_ThrowsWhenBlockedEmptyUniqueKey()
    {
        await _cache.SetStringAsync("ipBlock:192.168.1.1:register", "1", new());

        var ex = await Assert.ThrowsExceptionAsync<StatusCodeException>(
            () => _sut.CheckAsync("register", "", null, TimeSpan.FromMinutes(10)));

        Assert.AreEqual(429, ex.HttpStatusCode);
    }

    [TestMethod]
    public async Task CheckAsync_ExtendsBlockWhenAlreadyBlocked()
    {
        await _cache.SetStringAsync("ipBlock:192.168.1.1:login:user@test.com", "1", new());

        try { await _sut.CheckAsync("login", "user@test.com", null, TimeSpan.FromMinutes(10)); } catch { }

        var blockValue = await _cache.GetStringAsync("ipBlock:192.168.1.1:login:user@test.com");
        Assert.AreEqual("1", blockValue);
    }

    [TestMethod]
    public async Task CheckAsync_UsesCustomErrorMessage()
    {
        await _cache.SetStringAsync("ipLimit:192.168.1.1:login:user@test.com", "-1", new());

        var ex = await Assert.ThrowsExceptionAsync<StatusCodeException>(
            () => _sut.CheckAsync("login", "user@test.com", "自定义错误"));

        Assert.AreEqual("自定义错误", ex.Message);
    }

    [TestMethod]
    public async Task CheckAsync_DoesNotWriteToCache()
    {
        await _sut.CheckAsync("login", "user@test.com");

        var cached = await _cache.GetStringAsync("ipLimit:192.168.1.1:login:user@test.com");
        Assert.IsNull(cached);
    }

    // --- RecordAsync ---

    [TestMethod]
    public async Task RecordAsync_FirstCallStoresOne()
    {
        await _sut.RecordAsync("login", "user@test.com", TimeSpan.FromMinutes(1), 20);

        var cached = await _cache.GetStringAsync("ipLimit:192.168.1.1:login:user@test.com");
        Assert.AreEqual("1", cached);
    }

    [TestMethod]
    public async Task RecordAsync_SecondCallStoresSameCount()
    {
        // lastCount++ 后置自增，currentCount 拿旧值，计数不变
        await _sut.RecordAsync("login", "user@test.com", TimeSpan.FromMinutes(1), 20);
        await _sut.RecordAsync("login", "user@test.com", TimeSpan.FromMinutes(1), 20);

        var cached = await _cache.GetStringAsync("ipLimit:192.168.1.1:login:user@test.com");
        Assert.AreEqual("1", cached);
    }

    [TestMethod]
    public async Task RecordAsync_SetsMinusOneWhenLimitReached()
    {
        // 首次存 "1"，第二次 currentCount=1 >= limitCount=1 → 存 -1
        await _sut.RecordAsync("register", "", TimeSpan.FromMinutes(1), 1);
        await _sut.RecordAsync("register", "", TimeSpan.FromMinutes(1), 1);

        var cached = await _cache.GetStringAsync("ipLimit:192.168.1.1:register");
        Assert.AreEqual("-1", cached);
    }

    [TestMethod]
    public async Task RecordAsync_CreatesBlockWhenLimitReachedWithBlockDuration()
    {
        await _sut.RecordAsync("login", "user@test.com", TimeSpan.FromMinutes(1), 1, TimeSpan.FromMinutes(10));
        await _sut.RecordAsync("login", "user@test.com", TimeSpan.FromMinutes(1), 1, TimeSpan.FromMinutes(10));

        var blockValue = await _cache.GetStringAsync("ipBlock:192.168.1.1:login:user@test.com");
        Assert.AreEqual("1", blockValue);
    }

    [TestMethod]
    public async Task RecordAsync_NoBlockWhenBlockDurationIsNull()
    {
        await _sut.RecordAsync("register", "", TimeSpan.FromMinutes(1), 1);

        var blockValue = await _cache.GetStringAsync("ipBlock:192.168.1.1:register");
        Assert.IsNull(blockValue);
    }

    [TestMethod]
    public async Task RecordAsync_MaintainsMinusOneWhenAlreadyAtLimit()
    {
        await _cache.SetStringAsync("ipLimit:192.168.1.1:login:user@test.com", "-1", new());

        await _sut.RecordAsync("login", "user@test.com", TimeSpan.FromMinutes(1), 20);

        var cached = await _cache.GetStringAsync("ipLimit:192.168.1.1:login:user@test.com");
        Assert.AreEqual("-1", cached);
    }

    // --- GetIpAddress ---

    [TestMethod]
    public void GetIpAddress_ReturnsRemoteIpAddress()
    {
        Assert.AreEqual("192.168.1.1", _sut.GetIpAddress());
    }

    [TestMethod]
    public void GetIpAddress_PrefersXRealIpHeader()
    {
        var headers = new HeaderDictionary { { "X-Real-IP", "10.0.0.1" } };
        var context = Substitute.For<HttpContext>();
        context.Request.Headers.Returns(headers);
        context.Connection.RemoteIpAddress.Returns(IPAddress.Parse("192.168.1.1"));
        _httpContextAccessor.HttpContext.Returns(context);

        Assert.AreEqual("10.0.0.1", _sut.GetIpAddress());
    }

    [TestMethod]
    public void GetIpAddress_XRealIpWithMultipleValuesTakesFirst()
    {
        var headers = new HeaderDictionary { { "X-Real-IP", "10.0.0.1, 10.0.0.2" } };
        var context = Substitute.For<HttpContext>();
        context.Request.Headers.Returns(headers);
        _httpContextAccessor.HttpContext.Returns(context);

        Assert.AreEqual("10.0.0.1", _sut.GetIpAddress());
    }
}
