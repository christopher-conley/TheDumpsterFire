using Microsoft.Extensions.Logging;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Interfaces
{
    public interface IRevenantFileLogger
    {
        IDisposable? BeginScope<TState>(TState state) where TState : notnull;
        bool IsEnabled(LogLevel logLevel);
        void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter);
    }
}
