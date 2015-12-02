using System;
using System.Collections.Generic;

namespace AnalyzerBug.Utils
{
    internal static class Guard
    {
        /// <summary>
        /// Ensures the value of the given <paramref name="parameter"/> is not null.
        /// Throws <see cref="ArgumentNullException"/> otherwise.
        /// </summary>
        /// <param name="parameter">objet to test for null</param>
        /// <param name="parameterName">parameter name</param>
        public static void NotNull(object parameter, string parameterName)
        {
            if (parameter == null)
                throw new ArgumentNullException(parameterName);
        }

        /// <summary>
        /// Ensures the value of the given <paramref name="parameter"/> is not null.
        /// Throws <see cref="ArgumentNullException"/> otherwise.
        /// </summary>
        /// <param name="parameter">generic object to test for default</param>
        /// <param name="parameterName">parameter name</param>
        public static void NotDefault<T>(T parameter, string parameterName)
        {
            if (EqualityComparer<T>.Default.Equals(parameter, default(T)))
                throw new ArgumentNullException(parameterName);
        }

        public static void NotNullOrEmpty<T>(T[] array, string parameterName)
        {
            if (array==null || array.Length == 0)
                throw new ArgumentNullException(parameterName);
        }

        /// <summary>
        /// Ensures the string value of the given <paramref name="parameter"/> is not null or empty.
        /// Throws <see cref="ArgumentNullException"/> in the first case, or 
        /// <see cref="ArgumentException"/> in the latter.
        /// </summary>
        /// <param name="parameter">string to test for null or empty</param>
        /// <param name="parameterName">parameter name</param>
        public static void NotNullOrEmpty(string parameter, string parameterName)
        {
            if (string.IsNullOrEmpty(parameter))
                throw new ArgumentNullException(parameterName);
        } 
    }
}
