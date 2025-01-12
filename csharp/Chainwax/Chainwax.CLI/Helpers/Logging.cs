using Microsoft.Extensions.Logging;
using static Chainwax.CLI.Bootstrap;
using Chainwax.CLI.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vertical.SpectreLogger;
using Spectre.Console;
using Spectre.Console.Advanced;

namespace Chainwax.CLI.Helpers {

    public static class StaticBareLogger {
        private static ILogger<IBareLogger>? _logger;
        public static ILogger<IBareLogger> Logger
        {
            get {
                if ((null == _logger)) {
                    if ((null == CBase.Logger)) {
                        ILogger<IBareLogger> _vlog = LoggerFactory.Create(builder => {
                            builder.ClearProviders();
                            builder.AddSpectreConsole(config => {
                            });
                        }).CreateLogger<BareLogger>();
                        return _vlog;
                    }
                    else {
                        return CBase.Logger;
                    }
                }
                return _logger;
            }
            set {
                _logger = value;
            }
        }

        public static IDisposable? BeginScope<TState>(TState state) where TState : notnull {
            return _logger.BeginScope(state);
        }

        public static bool IsEnabled(LogLevel logLevel) {
            return _logger.IsEnabled(logLevel);
        }

        public static void LogDebug(this ILogger logger, EventId eventId, Exception? exception, string? message, params object?[] args) {
            _logger.Log(LogLevel.Debug, eventId, exception, message, args);
        }

        public static void LogDebug(this ILogger logger, EventId eventId, string? message, params object?[] args) {
            _logger.Log(LogLevel.Debug, eventId, message, args);
        }

        public static void LogDebug(this ILogger logger, Exception? exception, string? message, params object?[] args) {
            _logger.Log(LogLevel.Debug, exception, message, args);
        }

        public static void LogDebug(this ILogger logger, string? message, params object?[] args) {
            _logger.Log(LogLevel.Debug, message, args);
        }

        public static void LogTrace(this ILogger logger, EventId eventId, Exception? exception, string? message, params object?[] args) {
            _logger.Log(LogLevel.Trace, eventId, exception, message, args);
        }

        public static void LogTrace(this ILogger logger, EventId eventId, string? message, params object?[] args) {
            _logger.Log(LogLevel.Trace, eventId, message, args);
        }

        public static void LogTrace(this ILogger logger, Exception? exception, string? message, params object?[] args) {
            _logger.Log(LogLevel.Trace, exception, message, args);
        }

        public static void LogTrace(this ILogger logger, string? message, params object?[] args) {
            _logger.Log(LogLevel.Trace, message, args);
        }

        public static void LogInformation(this ILogger logger, EventId eventId, Exception? exception, string? message, params object?[] args) {
            _logger.Log(LogLevel.Information, eventId, exception, message, args);
        }

        public static void LogInformation(this ILogger logger, EventId eventId, string? message, params object?[] args) {
            _logger.Log(LogLevel.Information, eventId, message, args);
        }

        public static void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    public class BareLogger : IBareLogger {
        private readonly ILogger<IBareLogger> logger;

        public BareLogger(ILogger<IBareLogger> _logger) {
            logger = _logger;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
            return logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel) {
            return logger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
            logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}