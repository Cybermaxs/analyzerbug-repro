using System.Diagnostics;
using Xunit;

namespace AnalyzerBug.Tests.Attributes
{
    public class RunnableInDebugOnlyFactAttribute : FactAttribute
    {
        public RunnableInDebugOnlyFactAttribute()
        {
            if (!Debugger.IsAttached)
            {
                Skip = "Only running in interactive mode.";
            }
        }
    }
}
