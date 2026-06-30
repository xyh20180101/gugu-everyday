using Microsoft.VisualStudio.TestTools.UnitTesting;
using GuguEveryday.Models;
using GuguEveryday.Services;
using Microsoft.Extensions.Configuration;

namespace GuguEveryday.Tests.Services;

[TestClass]
public class JwtServiceTests
{
    private readonly InMemoryDistributedCache _cache = new();
    private readonly IConfiguration _configuration;
    private readonly JwtService _sut;

    private const string SecretKey = "Test_JWT_Secret_Key_For_Unit_Tests_32chars!";

    public JwtServiceTests()
    {
        var configDict = new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = SecretKey,
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience",
            ["Jwt:ExpirationMinutes"] = "30"
        };
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(configDict).Build();
        _sut = new JwtService(_configuration, _cache);
    }

    private static User CreateUser(long id = 1, string email = "test@example.com")
    {
        var user = new User { Email = email, IsActive = true };
        typeof(BaseModel).GetProperty(nameof(BaseModel.Id))!.SetValue(user, id);
        return user;
    }

    [TestMethod]
    public void GenerateToken_ReturnsNonEmptyString()
    {
        var token = _sut.GenerateToken(CreateUser());

        Assert.IsFalse(string.IsNullOrEmpty(token));
    }

    [TestMethod]
    public async Task GenerateToken_StoresTokenInCache()
    {
        var token = _sut.GenerateToken(CreateUser(email: "alice@example.com"));

        var cached = await _cache.GetStringAsync("jwt:alice@example.com:" + token);
        Assert.AreEqual("1", cached);
    }

    [TestMethod]
    public void ValidateToken_ReturnsUserIdForValidToken()
    {
        var user = CreateUser(id: 42, email: "bob@example.com");
        var token = _sut.GenerateToken(user);

        var userId = _sut.ValidateToken(token);

        Assert.AreEqual(42, userId);
    }

    [TestMethod]
    public void ValidateToken_ReturnsNullForInvalidToken()
    {
        var userId = _sut.ValidateToken("invalid.token.value");

        Assert.IsNull(userId);
    }

    [TestMethod]
    public void ValidateToken_ReturnsNullForTamperedToken()
    {
        var token = _sut.GenerateToken(CreateUser());
        var tampered = token[..^5] + "XXXXX";

        var userId = _sut.ValidateToken(tampered);

        Assert.IsNull(userId);
    }

    [TestMethod]
    public void ExpirationMinutes_ReturnsConfiguredValue()
    {
        Assert.AreEqual(30, _sut.ExpirationMinutes);
    }
}
