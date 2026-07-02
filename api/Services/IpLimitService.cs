using Microsoft.Extensions.Caching.Distributed;

namespace GuguEveryday.Services;

public class IpLimitService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDistributedCache _distributedCache;

    public IpLimitService(IHttpContextAccessor httpContextAccessor, IDistributedCache distributedCache)
    {
        _httpContextAccessor = httpContextAccessor;
        _distributedCache = distributedCache;
    }

    public async Task CheckAsync(string action, string uniqueKey, string? errorMessage = null, TimeSpan? blockDuration = null)
    {
        var ip = GetIpAddress();
        errorMessage = string.IsNullOrEmpty(errorMessage) ? "操作过于频繁，请稍后再试" : errorMessage;

        var key = string.IsNullOrEmpty(uniqueKey) ? $"ipLimit:{ip}:{action}" : $"ipLimit:{ip}:{action}:{uniqueKey}";
        var blockKey = string.IsNullOrEmpty(uniqueKey) ? $"ipBlock:{ip}:{action}" : $"ipBlock:{ip}:{action}:{uniqueKey}";

        if (blockDuration is not null && blockDuration != TimeSpan.Zero)
        {
            var blockValue = await _distributedCache.GetStringAsync(blockKey);
            if (!string.IsNullOrEmpty(blockValue))
            {
                //拉黑了还试, 继续拉黑
                await _distributedCache.SetStringAsync(blockKey, "1", new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = blockDuration
                });
                throw new StatusCodeException(errorMessage, 429);
            }
        }

        var value = await _distributedCache.GetStringAsync(key);
        //到达上限后, 继续调用不延续时长
        if (!string.IsNullOrEmpty(value) && int.Parse(value) == -1)
            throw new StatusCodeException(errorMessage, 429);
    }

    public async Task RecordAsync(string action, string uniqueKey, TimeSpan duration, int limitCount, TimeSpan? blockDuration = null)
    {
        var ip = GetIpAddress();

        var key = string.IsNullOrEmpty(uniqueKey) ? $"ipLimit:{ip}:{action}" : $"ipLimit:{ip}:{action}:{uniqueKey}";
        var blockKey = string.IsNullOrEmpty(uniqueKey) ? $"ipBlock:{ip}:{action}" : $"ipBlock:{ip}:{action}:{uniqueKey}";

        var value = await _distributedCache.GetStringAsync(key);
        if (string.IsNullOrEmpty(value))
        {
            await _distributedCache.SetStringAsync(key, "1", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duration
            });
        }
        else
        {
            var lastCount = int.Parse(value);

            //上次已达上限
            if (lastCount == -1)
            {
                await _distributedCache.SetStringAsync(key, "-1", new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = duration
                });
                return;
            }

            var currentCount = lastCount++;

            //上次未达上限,本次已达上限
            if (blockDuration is not null && blockDuration != TimeSpan.Zero && currentCount >= limitCount)
            {
                await _distributedCache.SetStringAsync(blockKey, "1", new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = blockDuration
                });
            }

            await _distributedCache.SetStringAsync(key, (currentCount >= limitCount ? -1 : currentCount).ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duration
            });
        }
    }

    public string GetIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request.Headers.TryGetValue("X-Real-IP", out var realIp) == true && !string.IsNullOrEmpty(realIp))
            return realIp.ToString().Split(',')[0].Trim();
        return httpContext?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }
}