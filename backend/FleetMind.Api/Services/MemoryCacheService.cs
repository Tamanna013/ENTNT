using System;
using System.Threading.Tasks;
using FleetMind.Api.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FleetMind.Api.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly CacheOptions _options;

        public MemoryCacheService(IMemoryCache cache, IOptions<CacheOptions> options)
        {
            _cache = cache;
            _options = options.Value;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (!_cache.TryGetValue(key, out object? cachedValueObj))
            {
                var cachedValue = await factory();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(expiration ?? TimeSpan.FromMinutes(_options.DefaultAbsoluteExpirationMinutes));

                _cache.Set(key, cachedValue, cacheEntryOptions);
                return cachedValue;
            }

            return (T)cachedValueObj!;
        }

        public void Invalidate(string key)
        {
            _cache.Remove(key);
        }
    }
}
