using MailKit.Net.Smtp;
using Microsoft.Extensions.Caching.Distributed;
using MimeKit;
using System.Security.Cryptography;
using System.Text.Json;

namespace GuguEveryday.Services;

public class EmailService
{
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;

    public EmailService(IDistributedCache cache, IConfiguration configuration)
    {
        _cache = cache;
        _configuration = configuration;
    }

    public async Task SendResetPasswordEmailAsync(string email)
    {
        var token = GenerateToken();
        await _cache.SetStringAsync($"ResetPassword:{token}", JsonSerializer.Serialize(new TokenInfo { Email = email }), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        });

        await SendEmailAsync(email, "Gugu Everyday 重置密码",
            $"请在15分钟内通过以下链接重置你的密码：\nhttps://gugu-everyday.cn/admin/reset-password?token={token}\n如非本人操作，请忽略该邮件。");
    }

    public async Task SendActivationEmailAsync(long userId, string email)
    {
        var token = GenerateToken();
        await _cache.SetStringAsync($"Activate:{token}", userId.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        });

        await SendEmailAsync(email, "Gugu Everyday 邮箱验证",
            $"请在30分钟内通过以下链接完成邮箱验证：\nhttps://gugu-everyday.cn/admin/activate?token={token}\n如非本人操作，请忽略该邮件。");
    }

    public async Task<TokenInfo?> GetResetPasswordInfoAsync(string token)
    {
        var json = await _cache.GetStringAsync($"ResetPassword:{token}");
        return json == null ? null : JsonSerializer.Deserialize<TokenInfo>(json);
    }

    public async Task RemoveResetPasswordInfoAsync(string token)
    {
        await _cache.RemoveAsync($"ResetPassword:{token}");
    }

    public async Task<long?> GetActivateUserIdAsync(string token)
    {
        var value = await _cache.GetStringAsync($"Activate:{token}");
        return value == null ? null : long.Parse(value);
    }

    public async Task RemoveActivateInfoAsync(string token)
    {
        await _cache.RemoveAsync($"Activate:{token}");
    }

    public class TokenInfo
    {
        public string Email { get; set; } = string.Empty;
    }

    private static string GenerateToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpSection = _configuration.GetSection("Smtp");
        var fromName = smtpSection["FromName"] ?? "Gugu Everyday";
        var fromAddress = smtpSection["FromAddress"] ?? throw new InvalidOperationException("Smtp:FromAddress not configured");
        var host = smtpSection["Host"] ?? throw new InvalidOperationException("Smtp:Host not configured");
        var port = int.Parse(smtpSection["Port"] ?? "465");
        var username = smtpSection["Username"] ?? throw new InvalidOperationException("Smtp:Username not configured");
        var password = smtpSection["Password"] ?? throw new InvalidOperationException("Smtp:Password not configured");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, true);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
