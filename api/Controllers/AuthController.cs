using GuguEveryday.Models;
using GuguEveryday.Models.Dtos;
using GuguEveryday.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using StackExchange.Redis;

namespace GuguEveryday.Controllers;

public class AuthController : BaseController
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<UserInfo> _userInfoRepository;
    private readonly PasswordService _passwordService;
    private readonly JwtService _jwtService;
    private readonly PoWService _powService;
    private readonly EmailService _emailService;
    private readonly IpLimitService _ipLimitService;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IRepository<User> userRepository,
        IRepository<UserInfo> userInfoRepository,
        PasswordService passwordService,
        JwtService jwtService,
        PoWService powService,
        EmailService emailService,
        IpLimitService ipLimitService,
        IConnectionMultiplexer connectionMultiplexer,
        IDistributedCache cache,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _userInfoRepository = userInfoRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _powService = powService;
        _emailService = emailService;
        _ipLimitService = ipLimitService;
        _connectionMultiplexer = connectionMultiplexer;
        _cache = cache;
        _logger = logger;
    }

    public class RegisterRequest
    {
        [Required(ErrorMessage = "邮箱不能为空")]
        [StringLength(100, ErrorMessage = "邮箱长度不能超过100个字符")]
        [EmailAddress(ErrorMessage = "邮箱格式错误")]
        public string Email { get; set; } = string.Empty;

        [DisableAuditing]
        [Required(ErrorMessage = "密码不能为空")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须在6-100个字符之间")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "nonce不能为空")]
        public string Nonce { get; set; } = string.Empty;

        [Required(ErrorMessage = "counter不能为空")]
        public string Counter { get; set; } = string.Empty;
    }

    [HttpPost("register")]
    public async Task Register([FromBody] RegisterRequest request)
    {
        var powValid = await _powService.VerifyPoWChallengeAsync(request.Email, request.Nonce, request.Counter);
        if (!powValid)
            throw new StatusCodeException("PoW验证失败");

        await _ipLimitService.CheckAsync("register", string.Empty);
        await _ipLimitService.RecordAsync("register", string.Empty, TimeSpan.FromMinutes(1), 1);

        if (await _userRepository.AnyAsync(u => u.Email == request.Email))
            throw new StatusCodeException("邮箱已注册", 409);

        var salt = _passwordService.GenerateSalt();
        var passwordHash = _passwordService.HashPassword(request.Password, salt);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            Salt = salt,
            IsActive = false,
            LastLoginAt = DateTime.UtcNow
        };

        await _userRepository.InsertAsync(user, true);

        user.IdSalt = _passwordService.GenerateSalt();
        user.IdHash = _passwordService.HashPassword(user.Id.ToString(), user.IdSalt);

        var userInfo = new UserInfo
        {
            UserId = user.Id,
            UserName = request.Email.Split('@')[0],
            Bulletin = string.Empty,
            IsShowPageEnabled = false,
            ShowPageTitle = "项目进度",
            IsAllowReminder = false
        };

        await _userInfoRepository.InsertAsync(userInfo);

        await _emailService.SendActivationEmailAsync(user.Id, request.Email);
    }

    public class ActivateRequest
    {
        [DisableAuditing]
        [Required(ErrorMessage = "token不能为空")]
        public string Token { get; set; } = string.Empty;
    }

    [HttpPost("activate")]
    public async Task Activate([FromBody] ActivateRequest request)
    {
        var userId = await _emailService.GetActivateUserIdAsync(request.Token);
        if (userId is null)
            throw new StatusCodeException("链接无效或已过期", 403);

        var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId.Value);
        if (user is null)
            throw new StatusCodeException("用户不存在", 404);

        if (user.IsActive)
        {
            await _emailService.RemoveActivateInfoAsync(request.Token);
            return;
        }

        user.IsActive = true;
        await _userRepository.UpdateAsync(user, true);
        await _emailService.RemoveActivateInfoAsync(request.Token);

        _logger.LogInformation("用户 {Email} 邮箱验证通过", user.Email);
    }

    public class LoginChallengeRequest
    {
        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "邮箱格式错误")]
        public string Email { get; set; } = string.Empty;
    }

    [HttpGet("login-challenge")]
    public async Task<object> GetLoginChallenge([FromQuery] LoginChallengeRequest request)
    {
        var (challenge, nonce, expiresAt) = await _powService.GenerateLoginChallengeAsync(request.Email);

        return new
        {
            Challenge = challenge,
            Nonce = nonce,
            ExpiresAt = expiresAt
        };
    }

    [HttpGet("update-password-challenge")]
    [Authorize]
    public async Task<object> GetUpdatePasswordChallenge()
    {
        var (challenge, nonce, expiresAt) = await _powService.GenerateLoginChallengeAsync(CurrentUserId!.Value.ToString());

        return new
        {
            Challenge = challenge,
            Nonce = nonce,
            ExpiresAt = expiresAt
        };
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "邮箱格式错误")]
        public string Email { get; set; } = string.Empty;

        [DisableAuditing]
        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "nonce不能为空")]
        public string Nonce { get; set; } = string.Empty;

        [Required(ErrorMessage = "counter不能为空")]
        public string Counter { get; set; } = string.Empty;
    }

    [HttpPost("login")]
    public async Task<object> Login([FromBody] LoginRequest request)
    {
        var powValid = await _powService.VerifyPoWChallengeAsync(request.Email, request.Nonce, request.Counter);
        if (!powValid)
            throw new StatusCodeException("PoW验证失败");

        await _ipLimitService.CheckAsync("login", request.Email, null, TimeSpan.FromMinutes(10));

        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
        if (user is null || !_passwordService.VerifyPassword(request.Password, user.Salt, user.PasswordHash))
        {
            await _ipLimitService.RecordAsync("login", request.Email, TimeSpan.FromMinutes(1), 20, TimeSpan.FromMinutes(10));
            throw new StatusCodeException("邮箱或密码错误", 403);
        }

        user.LastLoginAt = DateTime.UtcNow;
        var token = _jwtService.GenerateToken(user);

        _logger.LogInformation("用户 {Email} 登录成功", user.Email);

        return new
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtService.ExpirationMinutes)
        };
    }

    public class ResetPasswordSendEmailRequest
    {
        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "邮箱格式错误")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "nonce不能为空")]
        public string Nonce { get; set; } = string.Empty;

        [Required(ErrorMessage = "counter不能为空")]
        public string Counter { get; set; } = string.Empty;
    }

    [HttpPost("reset-password-send-email")]
    public async Task ResetPasswordSendEmail([FromBody] ResetPasswordSendEmailRequest request)
    {
        var powValid = await _powService.VerifyPoWChallengeAsync(request.Email, request.Nonce, request.Counter);
        if (!powValid)
            throw new StatusCodeException("PoW验证失败");

        await _ipLimitService.CheckAsync("reset-password", request.Email);
        await _ipLimitService.RecordAsync("reset-password", request.Email, TimeSpan.FromMinutes(1), 1);

        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user is null)
            throw new StatusCodeException("邮箱不存在", 403);

        await _emailService.SendResetPasswordEmailAsync(user.Email);
    }

    public class ResetPasswordValidRequest
    {
        [DisableAuditing]
        public string Token { get; set; } = string.Empty;
    }

    [HttpPost("reset-password-valid")]
    public async Task ResetPasswordValid([FromBody] ResetPasswordValidRequest request)
    {
        var info = await _emailService.GetResetPasswordInfoAsync(request.Token);
        if (info is null)
            throw new StatusCodeException("token无效", 403);
    }

    public class ResetPasswordRequest
    {
        [DisableAuditing]
        public string Token { get; set; } = string.Empty;

        [DisableAuditing]
        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; } = string.Empty;
    }

    [HttpPost("reset-password")]
    public async Task ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var info = await _emailService.GetResetPasswordInfoAsync(request.Token);
        if (info is null)
            throw new StatusCodeException("token无效", 403);

        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == info.Email);
        if (user is null)
            throw new StatusCodeException("token无效", 403);

        user.Salt = _passwordService.GenerateSalt();
        user.PasswordHash = _passwordService.HashPassword(request.Password, user.Salt);
        await _userRepository.UpdateAsync(user, true);

        await _emailService.RemoveResetPasswordInfoAsync(request.Token);
        await RemoveAllJwtAsync(user.Email);
    }

    public class UpdatePasswordRequest
    {
        [DisableAuditing]
        [Required(ErrorMessage = "旧密码不能为空")]
        public string OldPassword { get; set; } = string.Empty;

        [DisableAuditing]
        [Required(ErrorMessage = "新密码不能为空")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "nonce不能为空")]
        public string Nonce { get; set; } = string.Empty;

        [Required(ErrorMessage = "counter不能为空")]
        public string Counter { get; set; } = string.Empty;
    }

    [HttpPost("update-password")]
    [Authorize]
    public async Task UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        var user = (await _userRepository.FirstOrDefaultAsync(u => u.Id == CurrentUserId!.Value))!;

        var powValid = await _powService.VerifyPoWChallengeAsync(user.Id.ToString(), request.Nonce, request.Counter);
        if (!powValid)
            throw new StatusCodeException("PoW验证失败");

        if (!_passwordService.VerifyPassword(request.OldPassword, user.Salt, user.PasswordHash))
            throw new StatusCodeException("旧密码错误", 403);

        user.Salt = _passwordService.GenerateSalt();
        user.PasswordHash = _passwordService.HashPassword(request.NewPassword, user.Salt);
        await _userRepository.UpdateAsync(user, true);

        await RemoveAllJwtAsync(user.Email);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<UserInfoDto?> GetCurrentUserInfo()
    {
        var userInfo = await _userInfoRepository.FirstOrDefaultAsync(u => u.UserId == CurrentUserId!.Value);
        if (userInfo is null)
            throw new EntityNotFoundException("用户信息不存在");

        return userInfo.ToDto();
    }

    [HttpGet("userIdHash")]
    [Authorize]
    public async Task<GetUserIdHashResponse> GetUserIdHash()
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == CurrentUserId!.Value);
        if (user is null)
            throw new EntityNotFoundException("用户不存在");

        return new GetUserIdHashResponse { UserIdHash = user.IdHash };
    }

    public class GetUserIdHashResponse
    {
        public string UserIdHash { get; set; } = string.Empty;
    }

    private async Task RemoveAllJwtAsync(string email)
    {
        var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
        foreach (var key in server.Keys(pattern: $"GuguEveryday:jwt:{email}:*"))
        {
            await _connectionMultiplexer.GetDatabase().KeyDeleteAsync(key);
        }
    }
}
