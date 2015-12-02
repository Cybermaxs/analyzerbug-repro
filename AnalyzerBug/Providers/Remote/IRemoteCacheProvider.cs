using AnalyzerBug.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnalyzerBug.Providers.Remote
{
    public interface IRemoteCacheProvider
    {
        bool Connect();
        bool IsConnected { get; }
        Task<bool> ContainsAsync(string key);
        Task<long> CountAsync();
        IEnumerable<string> Keys(string pattern);
        Task FlushAsync();
        Task SetAsync<T>(RemoteCacheItem<T> cacheItem);
        bool Set<T>(RemoteCacheItem<T> cacheItem);
        Task<RemoteCacheItem<T>> GetAsync<T>(string key);
        RemoteCacheItem<T> Get<T>(string key);
        Task<long> RemoveAsync(params string[] keys);
        long Remove(params string[] keys);
        bool LockTake(string key, string value, TimeSpan expiry);
        bool LockRelease(string key, string value);
        IConnectionMultiplexer Multiplexer { get; }
    }
}
