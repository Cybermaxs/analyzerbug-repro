using System.Diagnostics;
using Xunit;

namespace AnalyzerBug.Tests.Attributes
{
    public sealed class RunnableInDebugOnlyTheoryAttribute : TheoryAttribute
    {
        public RunnableInDebugOnlyTheoryAttribute()
        {
            if (!Debugger.IsAttached)
            {
                Skip = "Only running in interactive mode.";
            }
        }
    }
}
