using Microsoft.Extensions.Caching.Memory;
using IntelliPM.Application.Interfaces;

namespace IntelliPM.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly HashSet<string> _cacheKeys = new();
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var cachedValue = _cache.Get<T>(key);
        return Task.FromResult(cachedValue);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5),
            Priority = CacheItemPriority.Normal
        };

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            _cache.Set(key, value, options);
            _cacheKeys.Add(key);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        _cacheKeys.Remove(key);
        return Task.CompletedTask;
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var keysToRemove = _cacheKeys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                _cacheKeys.Remove(key);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

