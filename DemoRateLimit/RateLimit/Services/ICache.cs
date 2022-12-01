using System.Threading.Tasks;

namespace RateLimit.Services
{
    public interface ICache
    {
        Task<bool> SetAsync<T>(string key, T item);

        Task<T> GetAsync<T>(string key);

        bool Set<T>(string key, T item);

        T Get<T>(string key);
    }
}
