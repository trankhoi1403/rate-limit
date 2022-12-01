using System.Threading.Tasks;

namespace RateLimit.Core.Services
{
    public interface ICache
    {
        Task<bool> SetAsync<T>(string key, T item);

        Task<T> GetAsync<T>(string key);
    }
}
