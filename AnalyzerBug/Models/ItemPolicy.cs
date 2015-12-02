using System;

namespace AnalyzerBug.Models
{
    /// <summary>
    /// Represents cache item settings for each providers.
    /// </summary>
    public class ItemPolicy
    {
        /// <summary>
        /// The default ItemPolicy. Means the item is never cached.
        /// </summary>
        public static readonly ItemPolicy Default = new ItemPolicy();

        /// <summary>
        /// The local TTL value.
        /// </summary>
        public TimeSpan? LocalTtl { get; internal set; }
        /// <summary>
        /// Flag for local sliding expiration.
        /// </summary>
        public bool LocalIsSliding { get; internal set; }
        /// <summary>
        /// Local stale ratio.
        /// </summary>
        public double LocalStaleRatio { get; internal set; }
        /// <summary>
        /// The remote TTL value.
        /// </summary>
        public TimeSpan? RemoteTtl { get; internal set; }
        /// <summary>
        /// Flag for remote sliding expiration.
        /// </summary>
        public bool RemoteIsSliding { get; internal set; }
        /// <summary>
        /// Remote stale ratio.
        /// </summary>
        public double RemoteStaleRatio { get; internal set; }

        #region overrides
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            var tobj = obj as ItemPolicy;
            if (tobj == null)
                return false;

            return LocalTtl == tobj.LocalTtl
                && LocalIsSliding == tobj.LocalIsSliding
                && LocalStaleRatio == tobj.LocalStaleRatio
                && RemoteTtl == tobj.RemoteTtl
                && RemoteIsSliding == tobj.RemoteIsSliding
                && RemoteStaleRatio == tobj.RemoteStaleRatio;
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
                hash = hash * 23 + LocalTtl.GetHashCode();
                hash = hash * 23 + LocalIsSliding.GetHashCode();
                hash = hash * 23 + LocalStaleRatio.GetHashCode();
                hash = hash * 23 + RemoteTtl.GetHashCode();
                hash = hash * 23 + RemoteIsSliding.GetHashCode();
                hash = hash * 23 + RemoteStaleRatio.GetHashCode();
                return hash;
            }
        }
        #endregion
    }
}
