using System;

namespace AnalyzerBug.Models
{
    /// <summary>
    /// Cache location type.
    /// </summary>
    [Flags]
    public enum CacheLocationType
    {
        None = 0,
        Local=1,
        Remote=2,
        Both= Local | Remote
    }

    internal static class CacheLocationTypeExtensions
    {
        /// <summary>
        /// Fast HasFlag check.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool Has(this CacheLocationType e, CacheLocationType other)
        {
            return ((e & other) == other);
        }
    }
}
