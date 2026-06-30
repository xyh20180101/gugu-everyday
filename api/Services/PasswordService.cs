using System.Security.Cryptography;
using System.Text;

namespace GuguEveryday.Services;

public class PasswordService
{
    private const int SaltSize = 32; // 256 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 100000; // PBKDF2 迭代次数
    
    public string GenerateSalt()
    {
        using var rng = RandomNumberGenerator.Create();
        var saltBytes = new byte[SaltSize];
        rng.GetBytes(saltBytes);
        return Convert.ToHexString(saltBytes).ToLowerInvariant();
    }
    
    public string HashPassword(string password, string salt)
    {
        var saltBytes = Convert.FromHexString(salt);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        
        // 使用新的静态方法替代已过时的构造函数
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(passwordBytes, saltBytes, Iterations, HashAlgorithmName.SHA256, HashSize);
        
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
    
    public bool VerifyPassword(string password, string salt, string hash)
    {
        var computedHash = HashPassword(password, salt);
        return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(computedHash), Convert.FromHexString(hash));
    }
}