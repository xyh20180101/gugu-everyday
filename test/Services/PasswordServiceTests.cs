using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using GuguEveryday.Services;

namespace GuguEveryday.Tests;

[TestClass]
public class PasswordServiceTests
{
    private readonly PasswordService _sut = new();

    [TestMethod]
    public void GenerateSalt_Returns64CharHexString()
    {
        var salt = _sut.GenerateSalt();

        Assert.AreEqual(64, salt.Length);
        Assert.IsTrue(salt.All(c => "0123456789abcdef".Contains(c)));
    }

    [TestMethod]
    public void GenerateSalt_ReturnsUniqueValues()
    {
        var salts = Enumerable.Range(0, 100).Select(_ => _sut.GenerateSalt()).ToList();

        Assert.AreEqual(100, salts.Distinct().Count());
    }

    [TestMethod]
    public void HashPassword_DeterministicForSameInputs()
    {
        var salt = _sut.GenerateSalt();

        var hash1 = _sut.HashPassword("password123", salt);
        var hash2 = _sut.HashPassword("password123", salt);

        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void HashPassword_DifferentForDifferentPasswords()
    {
        var salt = _sut.GenerateSalt();

        var hash1 = _sut.HashPassword("password123", salt);
        var hash2 = _sut.HashPassword("different456", salt);

        Assert.AreNotEqual(hash1, hash2);
    }

    [TestMethod]
    public void HashPassword_DifferentForDifferentSalts()
    {
        var salt1 = _sut.GenerateSalt();
        var salt2 = _sut.GenerateSalt();

        var hash1 = _sut.HashPassword("password123", salt1);
        var hash2 = _sut.HashPassword("password123", salt2);

        Assert.AreNotEqual(hash1, hash2);
    }

    [TestMethod]
    public void HashPassword_Returns64CharHexString()
    {
        var salt = _sut.GenerateSalt();
        var hash = _sut.HashPassword("password123", salt);

        Assert.AreEqual(64, hash.Length);
        Assert.IsTrue(hash.All(c => "0123456789abcdef".Contains(c)));
    }

    [TestMethod]
    public void VerifyPassword_ReturnsTrueForCorrectPassword()
    {
        var salt = _sut.GenerateSalt();
        var hash = _sut.HashPassword("password123", salt);

        Assert.IsTrue(_sut.VerifyPassword("password123", salt, hash));
    }

    [TestMethod]
    public void VerifyPassword_ReturnsFalseForWrongPassword()
    {
        var salt = _sut.GenerateSalt();
        var hash = _sut.HashPassword("password123", salt);

        Assert.IsFalse(_sut.VerifyPassword("wrongpassword", salt, hash));
    }

    [TestMethod]
    public void VerifyPassword_ReturnsFalseForTamperedHash()
    {
        var salt = _sut.GenerateSalt();
        var hash = _sut.HashPassword("password123", salt);
        var tampered = hash[..^2] + "00";

        Assert.IsFalse(_sut.VerifyPassword("password123", salt, tampered));
    }

    [TestMethod]
    public void VerifyPassword_CaseSensitive()
    {
        var salt = _sut.GenerateSalt();
        var hash = _sut.HashPassword("Password", salt);

        Assert.IsFalse(_sut.VerifyPassword("password", salt, hash));
    }

    [TestMethod]
    public void HashPassword_HandlesEmptyString()
    {
        var salt = _sut.GenerateSalt();
        var hash = _sut.HashPassword("", salt);

        Assert.AreEqual(64, hash.Length);
        Assert.IsTrue(_sut.VerifyPassword("", salt, hash));
    }

    [TestMethod]
    public void HashPassword_HandlesUnicodeChars()
    {
        var salt = _sut.GenerateSalt();
        var hash = _sut.HashPassword("密码测试🔒", salt);

        Assert.IsTrue(_sut.VerifyPassword("密码测试🔒", salt, hash));
        Assert.IsFalse(_sut.VerifyPassword("密码测试", salt, hash));
    }
}
