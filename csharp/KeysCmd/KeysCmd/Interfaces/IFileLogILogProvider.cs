using Microsoft.Extensions.Logging;

namespace RosettaTools.CLI.KeysCmd.Interfaces
{
    /// <summary>
    /// An interface for the FileLogILogProvider.
    /// </summary>
    public interface IFileLogILogProvider
    {
        /// <summary>
        /// Creates a logger with the specified category name.
        /// </summary>
        /// <param name="categoryName">The category name for the logger.</param>
        /// <returns>An instance of <see cref="ILogger"/>.</returns>
        ILogger CreateLogger(string categoryName);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void Dispose();
    }
}