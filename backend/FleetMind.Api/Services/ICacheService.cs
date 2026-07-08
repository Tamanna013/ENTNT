using System;
using System.Threading.Tasks;

namespace FleetMind.Api.Services
{
    public interface ICacheService
    {
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
        void Invalidate(string key);
    }
}
