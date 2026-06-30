using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp;

namespace GuguEveryday.Services;

public class IpLimitService
{
    private readonly TimeSpan Duration = TimeSpan.FromMinutes(5);
    private readonly int LimitCount = 3;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDistributedCache _distributedCache;

    public IpLimitService(IHttpContextAccessor httpContextAccessor, IDistributedCache distributedCache)
    {
        _httpContextAccessor = httpContextAccessor;
        _distributedCache = distributedCache;
    }

    public async Task CheckAsync(string uniqueKey, string? errorMessage = null)
    {
        var ip = GetIpAddress(_httpContextAccessor);
        var key = $"ipLimit:{ip}:{uniqueKey}";
        var value = await _distributedCache.GetStringAsync(key);
        errorMessage = string.IsNullOrEmpty(errorMessage) ? "调用次数超过限制" : errorMessage;
        if (string.IsNullOrEmpty(value))
        {
            await _distributedCache.SetStringAsync(key, "1", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = Duration
            });
        }
        else
        {
            var count = int.Parse(value);
            count++;
            await _distributedCache.SetStringAsync(key, int.Min(count, LimitCount).ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = Duration
            });
            if (count > LimitCount)
                throw new StatusCodeException(errorMessage, 403);
        }        
    }

    public static string GetIpAddress(IHttpContextAccessor httpContextAccessor)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Request.Headers.TryGetValue("X-Real-IP", out var realIp) == true && !string.IsNullOrEmpty(realIp))
            return realIp.ToString().Split(',')[0].Trim();
        return httpContext?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }
}