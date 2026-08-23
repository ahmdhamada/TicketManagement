using Microsoft.Extensions.Caching.Memory;
using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.Infrastructure.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool TryGet<T>(string key, out T? value) => _cache.TryGetValue(key, out value);

    public void Set<T>(string key, T value, TimeSpan absoluteExpiration) =>
        _cache.Set(key, value, absoluteExpiration);

    public void Remove(string key) => _cache.Remove(key);
}
