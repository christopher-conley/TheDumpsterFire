using Microsoft.Extensions.Logging;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Interfaces
{
    public interface IFileLogProvider
    {
        ILogger<FileLogProvider>? Logger { get; }
        IRevenantConfiguration LoggingConfig { get; }

        ILogger CreateLogger(string categoryName);
        void Dispose();
    }
}
