using System;

namespace AnalyzerBug.Models
{
    /// <summary>
    /// A container class for remote cache item.
    /// </summary>
    public class RemoteCacheItem<T> : CacheItem<T>
    {
        /// <summary>
        /// A default readonly empty value.
        /// </summary>
        public static new RemoteCacheItem<T> Default = new RemoteCacheItem<T>(string.Empty, default(T), ItemPolicy.Default, DateTime.UtcNow);

        /// <summary>
        /// CacheItem location.
        /// </summary>
        public override CacheLocationType Location { get { return CacheLocationType.Remote; } }

        /// <summary>
        /// Default ctor.
        /// </summary>
        /// <param name="key">Cache item key.</param>
        /// <param name="value">Cache item value.</param>
        /// <param name="policy">Cache item policy.</param>
        /// <param name="added">Added date (for RA)</param>
        public RemoteCacheItem(string key, T value, ItemPolicy policy, DateTime added)
            : base(key, value, policy, added)
        {
            if (policy != null && !policy.RemoteIsSliding)
            {
                //stale is only supported for not sliding keys
                staleAt = Policy.RemoteTtl.HasValue && Policy.RemoteTtl!=TimeSpan.MaxValue ? Added.Add(Policy.RemoteTtl.Value) : DateTime.MaxValue;
            }
        }
    }
}
