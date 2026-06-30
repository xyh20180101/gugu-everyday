using Microsoft.Extensions.Caching.Distributed;

namespace GuguEveryday.Tests;

public class InMemoryDistributedCache : IDistributedCache
{
    private readonly Dictionary<string, byte[]> _store = new();

    public byte[]? Get(string key)
    {
        return _store.TryGetValue(key, out var value) ? value : null;
    }

    public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        return Task.FromResult(Get(key));
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        _store[key] = value;
    }

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        _store[key] = value;
        return Task.CompletedTask;
    }

    public void Refresh(string key) { }

    public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;

    public void Remove(string key) => _store.Remove(key);

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        _store.Remove(key);
        return Task.CompletedTask;
    }

    public void SetString(string key, string value, DistributedCacheEntryOptions options)
    {
        _store[key] = System.Text.Encoding.UTF8.GetBytes(value);
    }

    public Task SetStringAsync(string key, string value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        _store[key] = System.Text.Encoding.UTF8.GetBytes(value);
        return Task.CompletedTask;
    }

    public string? GetString(string key)
    {
        return _store.TryGetValue(key, out var value) ? System.Text.Encoding.UTF8.GetString(value) : null;
    }

    public Task<string?> GetStringAsync(string key, CancellationToken token = default)
    {
        return Task.FromResult(GetString(key));
    }
}
