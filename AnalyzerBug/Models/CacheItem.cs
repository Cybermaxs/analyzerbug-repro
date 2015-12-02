using AnalyzerBug.Extensions;
using System;
using System.Collections.Generic;

namespace AnalyzerBug.Models
{
    /// <summary>
    /// Base class for cache items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CacheItem<T>
    {
        /// <summary>
        /// A default and readonly empty value.
        /// </summary>
        public static CacheItem<T> Default = new CacheItem<T>(string.Empty, default(T), ItemPolicy.Default, DateTime.MinValue);
        /// <summary>
        /// Cache item Key.
        /// </summary>
        public string Key { get; private set; }
        /// <summary>
        /// Cache item value.
        /// </summary>
        public T Value { get; private set; }
        /// <summary>
        /// Cache item policy.
        /// </summary>
        public ItemPolicy Policy { get; private set; }
        /// <summary>
        /// Added date (used for RA).
        /// </summary>
        public DateTime Added { get; private set; }
        /// <summary>
        /// internal stale date.
        /// </summary>
        protected DateTime staleAt = DateTime.MaxValue;

        /// <summary>
        /// Cache item location (local/remote).
        /// </summary>
        public virtual CacheLocationType Location { get { return CacheLocationType.None; } }
        /// <summary>
        /// Indicates if this item is stale or not.
        /// </summary>
        public virtual bool IsStale { get { return staleAt < DateTime.UtcNow; } }
        /// <summary>
        /// Indicates if the inner value is Null or Default(generic)
        /// </summary>
        public bool IsNullValue
        {
            get { return Value.IsNullOrDefault(); }
        }

        /// <summary>
        /// Default ctor.
        /// </summary>
        /// <param name="key">Cache item key.</param>
        /// <param name="value">Cache item value.</param>
        /// <param name="policy">Cache item policy.</param>
        /// <param name="added">Added date (for RA)</param>
        protected CacheItem(string key, T value, ItemPolicy policy, DateTime added)
        {
            Key = key;
            Value = value;
            Policy = policy;
            Added = added;
        }

        #region overrides
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            var tobj = obj as CacheItem<T>;
            if (tobj == null)
                return false;

            return Key == tobj.Key
                && Location == tobj.Location
                && EqualityComparer<T>.Default.Equals(Value,tobj.Value)
                && Policy == tobj.Policy
                && Added == tobj.Added;
        }
        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            //http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
            unchecked // Overflow is fine, just wrap
            {
                var hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + Key.GetHashCode();
                hash = hash * 23 + Location.GetHashCode();
                hash = hash * 23 + Value.GetHashCode();
                hash = hash * 23 + Policy.GetHashCode();
                hash = hash * 23 + Added.GetHashCode();
                return hash;
            }
        }
        #endregion
    }
}
