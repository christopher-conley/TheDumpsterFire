using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;

namespace Serilog.Enrichers
{
    sealed class CallerNameEnricher : ILogEventEnricher
    {
        LogEventProperty? _callerProperty;
        const string CallerNamePropertyName = "CallerName";


        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            string callerName = "Unknown";

            StackFrame? firstUserFrame = new StackTrace(fNeedFileInfo: true)
                .GetFrames()
                .Where(f =>
                    null != f &&
                    null != f.GetMethod() &&
                    null != f.GetMethod().DeclaringType &&
                    !string.IsNullOrEmpty(f.GetMethod().DeclaringType.FullName) &&
                    !f.GetMethod().DeclaringType.FullName.Contains("System.") &&
                    !f.GetMethod().DeclaringType.FullName.Contains("Serilog.") &&
                    !f.GetMethod().DeclaringType.FullName.Contains("Microsoft.Extensions")
                )
                .ToList()
                .FirstOrDefault();

            if (firstUserFrame != null)
            {
                callerName = firstUserFrame.GetMethod()?.Name ?? "Unknown";

                if (callerName == ".ctor")
                {
                    callerName = firstUserFrame.GetMethod()?.DeclaringType?.Name ?? "Unknown";
                }
            }

            //callerName = "[darkgoldenrod]" + callerName + "[/][fuchsia]()[/]";

            _callerProperty = propertyFactory.CreateProperty(
                CallerNamePropertyName,
                callerName);

            logEvent.AddPropertyIfAbsent(_callerProperty);
            ;
            ;
        }
    }
}
