using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace AnalyzerBug.Utils
{
    [ExcludeFromCodeCoverage]
    internal static class TaskConstants
    {
        public static readonly Task<bool> True = Task.FromResult(true);
        public static readonly Task<bool> False = Task.FromResult(false);
        public static readonly Task Empty = Task.FromResult<object>(null);

        public static readonly Task<int> Int32Zero = Task.FromResult(0);
        public static readonly Task<long> Int64Zero = Task.FromResult(0L);
    }
}
