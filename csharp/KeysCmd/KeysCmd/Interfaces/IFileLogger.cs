using Microsoft.Extensions.Logging;
using RosettaTools.CLI.KeysCmd.Logging;


namespace RosettaTools.CLI.KeysCmd.Interfaces
{
    /// <summary>
    /// Interface for the <see cref="FileLogger"/> class, a custom
    /// <br><see cref="ILogger"/> implementation that logs to a file since</br>
    /// <br>that functionality is not provided by the default <see cref="ILogger"/></br>
    /// </summary>
    public interface IFileLogger
    {
        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <typeparam name="TState">The type of the state to associate with the scope.</typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>An <see cref="IDisposable"/> that ends the logical operation scope on dispose.</returns>
        IDisposable? BeginScope<TState>(TState state) where TState : notnull;

        /// <summary>
        /// Checks if the given <see cref="Microsoft.Extensions.Logging.LogLevel"/> is enabled.
        /// </summary>
        /// <param name="logLevel">Level to be checked.</param>
        /// <returns><c>true</c> if enabled; otherwise, <c>false</c>.</returns>
        bool IsEnabled(LogLevel logLevel);

        /// <summary>
        /// Writes a log entry.
        /// </summary>
        /// <typeparam name="TState">The type of the object to be written.</typeparam>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">Id of the event.</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a <see cref="string"/> message of the state and exception.</param>
        void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter);
    }
}