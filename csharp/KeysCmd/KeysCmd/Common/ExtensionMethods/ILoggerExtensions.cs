using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace RosettaTools.CLI.KeysCmd.Common.ExtensionMethods
{

    /// <summary>
    /// Class containing extension methods for <see cref="ILogger"/>. Calling one of the
    /// <br>methods in this class will auto-populate the log message with the calling method's</br>
    /// <br>name, as well as applying some light markup to make log items more easily-distinguishable.</br>
    /// <br>Otherwise identical to the stock <c>ILogger</c> methods.</br>
    /// <br></br>
    /// <br>Contains the following methods:</br>
    /// <list type="bullet">
    /// <item><see cref="CLogDebug" /></item>
    /// <item><see cref="CLogTrace" /></item>
    /// <item><see cref="CLogInformation" /></item>
    /// <item><see cref="CLogWarning" /></item>
    /// <item><see cref="CLogError" /></item>
    /// <item><see cref="CLogCritical" /></item>
    /// <item><see cref="CLog" /></item>
    /// </list>
    /// </summary>

    public static class ILoggerExtensions
    {
        /// <summary>
        /// Logs a message at the <see cref="Microsoft.Extensions.Logging.LogLevel.Debug"/> level with the calling method's name prepended to the message.
        /// </summary>
        /// <param name="logger">The current <see cref="ILogger"/> instance that this method is being called on. Auto-populated.</param>
        /// <param name="message">A <see cref="string"/>? representing the message to be logged.</param>
        /// <param name="caller">A <see cref="string"/>? containing the name of the method that called this log method. Auto-populated.</param>
        /// <param name="args">An <see cref="object"/>?[] array containing any remaining arguments for the <see cref="ILogger"/> instance, such as
        /// <br>variables that will be interpolated and substituted for placeholders in the <c>message</c> string being logged.</br></param>
        public static void CLogDebug(this ILogger logger, string? message, [CallerMemberName] string? caller = "Unknown", params object?[] args)
        {
            message = $"{caller}[fuchsia]()[/]: {message}";
            logger.Log(LogLevel.Debug, message, args);
        }

        /// <summary>
        /// Logs a message at the <see cref="Microsoft.Extensions.Logging.LogLevel.Trace"/> level with the calling method's name prepended to the message.
        /// </summary>
        /// <param name="logger">The current <see cref="ILogger"/> instance that this method is being called on. Auto-populated.</param>
        /// <param name="message">A <see cref="string"/>? representing the message to be logged.</param>
        /// <param name="caller">A <see cref="string"/>? containing the name of the method that called this log method. Auto-populated.</param>
        /// <param name="args">An <see cref="object"/>?[] array containing any remaining arguments for the <see cref="ILogger"/> instance, such as
        /// <br>variables that will be interpolated and substituted for placeholders in the <c>message</c> string being logged.</br></param>
        public static void CLogTrace(this ILogger logger, string? message, [CallerMemberName] string? caller = "Unknown", params object?[] args)
        {
            message = $"{caller}[fuchsia]()[/]: {message}";
            logger.Log(LogLevel.Trace, message, args);
        }

        /// <summary>
        /// Logs a message at the <see cref="Microsoft.Extensions.Logging.LogLevel.Information"/> level with the calling method's name prepended to the message.
        /// </summary>
        /// <param name="logger">The current <see cref="ILogger"/> instance that this method is being called on. Auto-populated.</param>
        /// <param name="message">A <see cref="string"/>? representing the message to be logged.</param>
        /// <param name="caller">A <see cref="string"/>? containing the name of the method that called this log method. Auto-populated.</param>
        /// <param name="args">An <see cref="object"/>?[] array containing any remaining arguments for the <see cref="ILogger"/> instance, such as
        /// <br>variables that will be interpolated and substituted for placeholders in the <c>message</c> string being logged.</br></param>
        public static void CLogInformation(this ILogger logger, string? message, [CallerMemberName] string? caller = "Unknown", params object?[] args)
        {
            message = $"{caller}[fuchsia]()[/]: {message}";
            logger.Log(LogLevel.Information, message, args);
        }

        /// <summary>
        /// Logs a message at the <see cref="Microsoft.Extensions.Logging.LogLevel.Warning"/> level with the calling method's name prepended to the message.
        /// </summary>
        /// <param name="logger">The current <see cref="ILogger"/> instance that this method is being called on. Auto-populated.</param>
        /// <param name="message">A <see cref="string"/>? representing the message to be logged.</param>
        /// <param name="caller">A <see cref="string"/>? containing the name of the method that called this log method. Auto-populated.</param>
        /// <param name="args">An <see cref="object"/>?[] array containing any remaining arguments for the <see cref="ILogger"/> instance, such as
        /// <br>variables that will be interpolated and substituted for placeholders in the <c>message</c> string being logged.</br></param>
        public static void CLogWarning(this ILogger logger, string? message, [CallerMemberName] string? caller = "Unknown", params object?[] args)
        {
            message = $"{caller}[fuchsia]()[/]: {message}";
            logger.Log(LogLevel.Warning, message, args);
        }

        /// <summary>
        /// Logs a message at the <see cref="Microsoft.Extensions.Logging.LogLevel.Error"/> level with the calling method's name prepended to the message.
        /// </summary>
        /// <param name="logger">The current <see cref="ILogger"/> instance that this method is being called on. Auto-populated.</param>
        /// <param name="message">A <see cref="string"/>? representing the message to be logged.</param>
        /// <param name="caller">A <see cref="string"/>? containing the name of the method that called this log method. Auto-populated.</param>
        /// <param name="args">An <see cref="object"/>?[] array containing any remaining arguments for the <see cref="ILogger"/> instance, such as
        /// <br>variables that will be interpolated and substituted for placeholders in the <c>message</c> string being logged.</br></param>
        public static void CLogError(this ILogger logger, string? message, [CallerMemberName] string? caller = "Unknown", params object?[] args)
        {
            message = $"{caller}[fuchsia]()[/]: {message}";
            logger.Log(LogLevel.Error, message, args);
        }

        /// <summary>
        /// Logs a message at the <see cref="Microsoft.Extensions.Logging.LogLevel.Critical"/> level with the calling method's name prepended to the message.
        /// </summary>
        /// <param name="logger">The current <see cref="ILogger"/> instance that this method is being called on. Auto-populated.</param>
        /// <param name="message">A <see cref="string"/>? representing the message to be logged.</param>
        /// <param name="caller">A <see cref="string"/>? containing the name of the method that called this log method. Auto-populated.</param>
        /// <param name="args">An <see cref="object"/>?[] array containing any remaining arguments for the <see cref="ILogger"/> instance, such as
        /// <br>variables that will be interpolated and substituted for placeholders in the <c>message</c> string being logged.</br></param>
        public static void CLogCritical(this ILogger logger, string? message, [CallerMemberName] string? caller = "Unknown", params object?[] args)
        {
            message = $"{caller}[fuchsia]()[/]: {message}";
            logger.Log(LogLevel.Critical, message, args);
        }

        /// <summary>
        /// Logs a message at the specified <see cref="Microsoft.Extensions.Logging.LogLevel"/> with the calling method's name prepended to the message.
        /// </summary>
        /// <param name="logger">The current <see cref="ILogger"/> instance that this method is being called on. Auto-populated.</param>
        /// <param name="logLevel">The <see cref="Microsoft.Extensions.Logging.LogLevel"/> of the incoming message. If the <c>LogLevel</c> is less than the configured-minimum LogLevel, the message is discarded.</param>
        /// <param name="message">A <see cref="string"/>? representing the message to be logged.</param>
        /// <param name="caller">A <see cref="string"/>? containing the name of the method that called this log method. Auto-populated.</param>
        /// <param name="args">An <see cref="object"/>?[] array containing any remaining arguments for the <see cref="ILogger"/> instance, such as
        /// <br>variables that will be interpolated and substituted for placeholders in the <c>message</c> string being logged.</br></param>
        public static void CLog(this ILogger logger, LogLevel logLevel, string? message, [CallerMemberName] string? caller = "Unknown", params object?[] args)
        {
            message = $"{caller}[fuchsia]()[/]: {message}";
            logger.Log(logLevel, 0, null, message, args);
        }
    }
}
