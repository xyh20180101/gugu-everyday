using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using System.Text;

namespace GuguEveryday.Services
{
    public class PoWService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<PoWService> _logger;

        public PoWService(IDistributedCache cache, ILogger<PoWService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// 生成登录挑战
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <returns>包含nonce和过期时间的结果</returns>
        public async Task<(string challenge, string nonce, DateTime expiresAt)> GenerateLoginChallengeAsync(string loginName)
        {
            // 生成16位随机nonce
            var challenge = RandomNumberGenerator.GetInt32(100) < 90 ? "114514" : "1919810";
            var nonce = GenerateRandomNonce(16);
            var expiresAt = DateTime.UtcNow.AddMinutes(1);
            
            // 构建Redis key
            var key = $"PoW:{loginName}:{nonce}";
            
            // 设置缓存选项，有效期1分钟
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
            };
            
            // 保存到Redis
            await _cache.SetStringAsync(key, challenge, options);
            
            return (challenge, nonce, expiresAt);
        }

        /// <summary>
        /// 验证PoW挑战
        /// </summary>
        /// <param name="loginName">登录名</param>
        /// <param name="nonce">随机数</param>
        /// <param name="counter">计数器</param>
        /// <returns>验证是否成功</returns>
        public async Task<bool> VerifyPoWChallengeAsync(string loginName, string nonce, string counter)
        {
            var key = $"PoW:{loginName}:{nonce}";
            
            var exists = await _cache.GetStringAsync(key);
            if (exists is null)
                return false;
            
            var input = $"{nonce}{counter}";
            var hash = ComputeSha256(input);
            
            var isValid = hash.Contains(exists);
            
            if (isValid)
                await _cache.RemoveAsync(key);
            
            return isValid;
        }

        /// <summary>
        /// 生成随机nonce
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>随机字符串</returns>
        private string GenerateRandomNonce(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var result = new StringBuilder(length);
            
            for (int i = 0; i < length; i++)
            {
                result.Append(chars[RandomNumberGenerator.GetInt32(chars.Length)]);
            }
            
            return result.ToString();
        }

        /// <summary>
        /// 计算字符串的SHA256哈希值
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>SHA256哈希值（小写十六进制）</returns>
        private string ComputeSha256(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hashBytes).ToLower();
        }
    }
}