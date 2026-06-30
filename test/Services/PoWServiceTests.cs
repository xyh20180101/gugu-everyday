using Microsoft.VisualStudio.TestTools.UnitTesting;
using GuguEveryday.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GuguEveryday.Tests.Services;

[TestClass]
public class PoWServiceTests
{
    private readonly InMemoryDistributedCache _cache = new();
    private readonly ILogger<PoWService> _logger = Substitute.For<ILogger<PoWService>>();
    private readonly PoWService _sut;

    public PoWServiceTests()
    {
        _sut = new PoWService(_cache, _logger);
    }

    [TestMethod]
    public async Task GenerateLoginChallengeAsync_ReturnsValidChallenge()
    {
        var (challenge, nonce, expiresAt) = await _sut.GenerateLoginChallengeAsync("testuser");

        Assert.IsTrue(challenge == "114514" || challenge == "1919810");
        Assert.AreEqual(16, nonce.Length);
        Assert.IsTrue(expiresAt > DateTime.UtcNow);
        Assert.IsTrue(expiresAt <= DateTime.UtcNow.AddMinutes(2));
    }

    [TestMethod]
    public async Task GenerateLoginChallengeAsync_StoresInCache()
    {
        var (challenge, nonce, _) = await _sut.GenerateLoginChallengeAsync("testuser");

        var cached = await _cache.GetStringAsync($"PoW:testuser:{nonce}");
        Assert.AreEqual(challenge, cached);
    }

    [TestMethod]
    public async Task VerifyPoWChallengeAsync_ReturnsFalseWhenCacheMiss()
    {
        var result = await _sut.VerifyPoWChallengeAsync("testuser", "nonce123", "0");

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task VerifyPoWChallengeAsync_ReturnsTrueForValidProof()
    {
        var (challenge, nonce, _) = await _sut.GenerateLoginChallengeAsync("testuser");

        long foundCounter = -1;
        for (long c = 0; c < 10_000_000; c++)
        {
            var hash = System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(nonce + c));
            var hex = Convert.ToHexString(hash).ToLower();
            if (hex.Contains(challenge))
            {
                foundCounter = c;
                break;
            }
        }

        Assert.IsTrue(foundCounter >= 0, "Should find a valid counter within 10M iterations");

        var result = await _sut.VerifyPoWChallengeAsync("testuser", nonce, foundCounter.ToString());

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task VerifyPoWChallengeAsync_ReturnsFalseForInvalidProof()
    {
        await _sut.GenerateLoginChallengeAsync("testuser");

        var result = await _sut.VerifyPoWChallengeAsync("testuser", "nonce123", "999999");

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task VerifyPoWChallengeAsync_RemovesCacheEntryOnSuccess()
    {
        var (challenge, nonce, _) = await _sut.GenerateLoginChallengeAsync("testuser");

        long foundCounter = -1;
        for (long c = 0; c < 10_000_000; c++)
        {
            var hash = System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(nonce + c));
            var hex = Convert.ToHexString(hash).ToLower();
            if (hex.Contains(challenge))
            {
                foundCounter = c;
                break;
            }
        }

        await _sut.VerifyPoWChallengeAsync("testuser", nonce, foundCounter.ToString());

        var cached = await _cache.GetStringAsync($"PoW:testuser:{nonce}");
        Assert.IsNull(cached);
    }
}
