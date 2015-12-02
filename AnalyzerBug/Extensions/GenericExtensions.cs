using System.Collections.Generic;

namespace AnalyzerBug.Extensions
{
    /// <summary>
    /// Extension methods for genertic types.
    /// </summary>
    public static class GenericExtensions
    {
        /// <summary>
        /// Tests if a generic instance is equal to default(T)
        /// </summary>
        /// <typeparam name="T">Type of item to test</typeparam>
        /// <param name="item">item to test</param>
        /// <returns>true is value equals to default(T)</returns>
        public static bool IsNullOrDefault<T>(this T item)
        {
            return EqualityComparer<T>.Default.Equals(item, default(T));
        }
    }
}
