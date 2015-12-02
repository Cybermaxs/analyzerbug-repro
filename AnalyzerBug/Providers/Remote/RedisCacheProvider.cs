using AnalyzerBug.Models;
using AnalyzerBug.Serialization;
using AnalyzerBug.Utils;
using AnalyzerBug.Extensions;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StackExchange.Redis.KeyspaceIsolation;
using System.Collections.Generic;
using AnalyzerBug.Client;
using System.Reflection;

namespace AnalyzerBug.Providers.Remote
{
    internal class RedisCacheProvider : IRemoteCacheProvider
    {
        internal IConnectionMultiplexer mux;
        internal IDatabase db;
        private readonly ConfigurationOptions redisOptions;
        private readonly int redisDb;
        private readonly IMessageSerializer messageSerializer;
        private readonly string ns;

        //script
        private readonly LuaScript RefreshScript;
        private readonly LuaScript SetScript;

        public RedisCacheProvider(Guid clientId, ClientSettings settings)
        {
            Guard.NotNull(settings, "settings");

            redisOptions = ParseRedisConfig(clientId, settings.RedisConnectionInfo);
            redisDb = settings.DefaultRedisDb;
            messageSerializer = settings.MessageSerializer;
            ns = settings.Namespace;

            RefreshScript = LuaScript.Prepare(Scripts.Refresh);
            SetScript = LuaScript.Prepare(Scripts.Set);
        }

        public bool Connect()
        {
            if (!IsConnected)
            {
                try
                {
                    mux = ConnectionMultiplexer.Connect(redisOptions);
                    db = mux.GetDatabase(redisDb).WithKeyPrefix(ns ?? string.Empty);

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }

        public bool IsConnected
        {
            get { return mux != null && mux.IsConnected; }
        }

        public IConnectionMultiplexer Multiplexer
        {
            get { return mux; }
        }

        public IEnumerable<string> Keys(string pattern)
        {
            if (!IsConnected)
            {
                return Enumerable.Empty<string>();
            }

            var server = mux.GetServer();
            var p = string.IsNullOrEmpty(pattern) ? "*" : pattern;
            var keys = server.Keys(redisDb, p, int.MaxValue, CommandFlags.PreferMaster).ToList();

            return keys.Select(k => (string)k);
        }

        public Task<long> RemoveAsync(params string[] keys)
        {
            return SafeInvokeAsync(() =>
            {
                return db.KeyDeleteAsync(keys.Select(k => (RedisKey)k).ToArray());
                // TODO : notify subscribers ???

            }, 0L);
        }

        public long Remove(params string[] keys)
        {
            return SafeInvoke(() =>
            {
                return db.KeyDelete(keys.Select(k => (RedisKey)k).ToArray());
                // TODO : notify subscribers ???

            }, 0L);
        }

        public Task FlushAsync()
        {
            if (!IsConnected)
                return TaskConstants.Empty;

            var server = mux.GetServer();
            return server.FlushDatabaseAsync(db.Database);
        }

        public Task<bool> ContainsAsync(string key)
        {
            return SafeInvokeAsync(() =>
            {
                return db.KeyExistsAsync(key);
            }, false);
        }

        public Task<long> CountAsync()
        {
            return SafeInvokeAsync(async () =>
            {
                // note : we use INFO command to get the list of keys
                var server = mux.GetServer();
                var raw = await server.InfoRawAsync("Keyspace").ConfigureAwait(false);

                var match = Regex.Match(raw, @"keys=(\d*)");
                if (match.Success)
                {
                    long total;
                    long.TryParse(match.Value.Remove(0, 5), out total);
                    return total;
                }

                return 0L;
            }, 0L);
        }

        public Task SetAsync<T>(RemoteCacheItem<T> cacheItem)
        {
            return SafeInvokeAsync<bool>(async () =>
            {
                byte[] data;
                if (!messageSerializer.TrySerialize(cacheItem.Value, out data))
                {
                    return false;
                }

                var parameters = new ScriptParameters(ns+cacheItem.Key, cacheItem.Policy, cacheItem.Added, data);
                var result = await SetScript.EvaluateAsync(db, parameters).ConfigureAwait(false);
                return !result.IsNull;
            }, false);
        }

        public bool Set<T>(RemoteCacheItem<T> cacheItem)
        {
            return SafeInvoke(() =>
            {
                byte[] data;
                if (!messageSerializer.TrySerialize(cacheItem.Value, out data))
                {
                    return false;
                }

                var parameters = new ScriptParameters(ns + cacheItem.Key, cacheItem.Policy, cacheItem.Added, data);
                var result = SetScript.Evaluate(db, parameters);
                return !result.IsNull;
            }, false);
        }

        public Task<RemoteCacheItem<T>> GetAsync<T>(string key)
        {
            return SafeInvokeAsync(async () =>
            {
                RemoteCacheItem<T> cacheItem;
                var fields = await db.HashGetAllAsync(key).ConfigureAwait(false);

                if (!TryMapProperties(key, fields, out cacheItem))
                {
                    return cacheItem;
                }


                if (cacheItem.Policy.RemoteIsSliding)
                {
                    //refresh remote if remote sliding
                    var parameters = new ScriptParameters(ns + cacheItem.Key, cacheItem.Policy);

                    await RefreshScript.EvaluateAsync(db, parameters).ConfigureAwait(false);
                }

                return cacheItem;
            }, RemoteCacheItem<T>.Default);
        }

        public RemoteCacheItem<T> Get<T>(string key)
        {
            return SafeInvoke(() =>
            {
                RemoteCacheItem<T> cacheItem;
                var fields = db.HashGetAll(key);

                if (!TryMapProperties(key, fields, out cacheItem))
                {
                    return cacheItem;
                }


                if (cacheItem.Policy.RemoteIsSliding)
                {
                    //refresh remote if remote sliding
                    var parameters = new ScriptParameters(ns + cacheItem.Key, cacheItem.Policy);

                    RefreshScript.Evaluate(db, parameters);
                }

                return cacheItem;
            }, RemoteCacheItem<T>.Default);
        }

        public bool LockTake(string key, string value, TimeSpan expiry)
        {
            Guard.NotDefault(value, "value");

            return SafeInvoke(() =>
            {
                return db.LockTake(key, value, expiry);
            }, false);
        }

        public bool LockRelease(string key, string value)
        {
            Guard.NotDefault(value, "value");

            return SafeInvoke(() =>
            {
                return db.LockRelease(key, value);
            }, false);
        }

        #region Private
        internal bool TryMapProperties<T>(string key, HashEntry[] fields, out RemoteCacheItem<T> result)
        {
            result = RemoteCacheItem<T>.Default;
            if (fields == null || fields.Length == 0)
            {
                return false;
            }

            try
            {
                var policy = new ItemPolicy();
                var added = DateTime.UtcNow;
                var lastRetrieved = DateTime.UtcNow;
                var val = default(T);

                foreach (var field in fields)
                {
                    switch (field.Name)
                    {
                        case Constants.Redis.LocalTtlKey:
                            var localTicks = (long?)field.Value;
                            if (localTicks.HasValue && localTicks.Value != Constants.Redis.NotPresent)
                            {
                                policy.LocalTtl = TimeSpan.FromSeconds(localTicks.Value);
                            }
                            break;

                        case Constants.Redis.LocalStaleRatioKey:
                            policy.LocalStaleRatio = (double)field.Value;
                            break;

                        case Constants.Redis.LocalSlidingKey:
                            policy.LocalIsSliding = (bool)field.Value;
                            break;

                        case Constants.Redis.RemoteTtlKey:
                            var remoteTicks = (long?)field.Value;
                            if (remoteTicks.HasValue && remoteTicks.Value != Constants.Redis.NotPresent)
                            {
                                policy.RemoteTtl = TimeSpan.FromSeconds(remoteTicks.Value);
                            }
                            break;

                        case Constants.Redis.RemoteSlidingKey:
                            policy.RemoteIsSliding = (bool)field.Value;
                            break;
                        case Constants.Redis.RemoteStaleRatioKey:
                            policy.RemoteStaleRatio = (double)field.Value;
                            break;
                        case Constants.Redis.AddedKey:
                            var addedTicks = (long?)field.Value;
                            if (addedTicks.HasValue && addedTicks.Value != Constants.Redis.NotPresent)
                            {
                                added = new DateTime(addedTicks.Value, DateTimeKind.Utc);
                            }
                            break;
                        case Constants.Redis.DataKey:
                            var data = (byte[])field.Value;
                            if (data != null)
                            {
                                if (data.GetType() == typeof(T))
                                    val = (T)Convert.ChangeType(data, typeof(T));
                                else
                                    messageSerializer.TryDeserialize(data, out val);
                            }
                            break;
                    }
                }

                result = new RemoteCacheItem<T>(key, val, policy, added);
                return true;
            }
            catch (Exception ex)
            {
                result = RemoteCacheItem<T>.Default;
                return false;
            }
        }

        private ConfigurationOptions ParseRedisConfig(Guid clientId, string options)
        {
            var redisOptions = ConfigurationOptions.Parse(options);
            redisOptions.ConfigCheckSeconds = 300;
            redisOptions.KeepAlive = 60;
            redisOptions.ClientName = string.Format("CacheClient.{0}.{1}.{2}", Environment.MachineName, Assembly.GetExecutingAssembly().GetName().Name, clientId.ToString());
            return redisOptions;
        }

        private T SafeInvoke<T>(Func<T> func, T returnDefaultValue = default(T))
        {
            if (!IsConnected)
                return returnDefaultValue;

            try
            {
                return func();
            }
            catch (Exception ex)
            {
                //removed
            }
            return returnDefaultValue;
        }

        private async Task<T> SafeInvokeAsync<T>(Func<Task<T>> func, T returnDefaultValue = default(T))
        {
            if (!IsConnected)
                return returnDefaultValue;

            try
            {
                return await func().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                //removed
            }
            return returnDefaultValue;
        }
        #endregion

    }
}
