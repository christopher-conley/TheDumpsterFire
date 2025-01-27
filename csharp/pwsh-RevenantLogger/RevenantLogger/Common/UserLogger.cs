using Microsoft.Extensions.Logging;
using RosettaTools.Pwsh.Text.RevenantLogger.Helpers;
using RosettaTools.Pwsh.Text.RevenantLogger.Common.ExtensionMethods;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Common
{
    public class UserCustomLogger : RevenantLoggerPSCmdlet, ILogger
    {
        private ILogger _logger;
        private ILogger _pseudoLogger;
        private bool _hasType = true;

        public ILogger Logger
        {
            get => _pseudoLogger;
            set => _pseudoLogger = value;
        }

        public IRevenantConfiguration LConfig
        {
            get => RevenantConfig;
        }

        public UserCustomLogger(ILogger logger)
        {
            _logger = logger;
            _pseudoLogger = logger;
        }

        public UserCustomLogger(ILogger<UserCustomLogger> logger, IRevenantConfiguration config)
        {
            _logger = _pseudoLogger = logger;
            _config = config;

        }

        public void Log(string message)
        {
            object[] args = [];
            Log(message: message, logLevel: RevenantConfig.LoggingConfig.MinimumLogLevel, caller: null, args: args);
        }
        public void Log(string message, string logLevel)
        {
            object[] args = [];
            Log(message: message, logLevel: logLevel, caller: null, args: args);
        }

        public void Log(string message, string logLevel, string caller)
        {
            object[] args = [];
            Log(message: message, logLevel: logLevel, caller: caller, args: args);
        }

        public void Log(string message, string logLevel, string? caller, params object?[] args)
        {
            ShortLogLevel userShortLogLevel;

            bool levelParse = Enum.TryParse<ShortLogLevel>(logLevel, ignoreCase: true, out userShortLogLevel);
            if (!levelParse)
            {
                LogLevel userLogLevel;
                levelParse = Enum.TryParse<LogLevel>(logLevel, ignoreCase: true, out userLogLevel);

                if (!levelParse)
                {
                    Exception invalidLoglevel = new Exception($"The loglevel \"{logLevel}\" is not a valid log level.");
                    //WriteError(new ErrorRecord(invalidLoglevel, "InvalidLogLevel", ErrorCategory.InvalidArgument, logLevel));
                    throw invalidLoglevel;
                }
                else
                {
                    Log(message, userLogLevel, caller: caller, args: args);
                }
            }

            else
            {
                Log(message, shortlogLevel: userShortLogLevel, useShortLogLevel: true, caller: caller, args: args);
            }
        }

        public void Log(
            string message,
            LogLevel logLevel = LogLevel.Information,
            ShortLogLevel shortlogLevel = ShortLogLevel.info,
            bool useShortLogLevel = false,
            [CallerMemberName] string? caller = null,
            params object?[] args
            )
        {

            LogLevel switchLogLevel = (useShortLogLevel) ? (LogLevel)shortlogLevel : logLevel;

            switch (switchLogLevel)
            {
                case LogLevel.Information:
                    _pseudoLogger.RLogInformation(message: message, caller: caller, args: args);
                    break;
                case LogLevel.Warning:
                    _pseudoLogger.RLogWarning(message: message, caller: caller, args: args);
                    break;
                case LogLevel.Error:
                    _pseudoLogger.RLogError(message: message, caller: caller, args: args);
                    break;
                case LogLevel.Critical:
                    _pseudoLogger.RLogCritical(message: message, caller: caller, args: args);
                    break;
                case LogLevel.Debug:
                    _pseudoLogger.RLogDebug(message: message, caller: caller, args: args);
                    break;
                case LogLevel.Trace:
                    _pseudoLogger.RLogTrace(message: message, caller: caller, args: args);
                    break;
                default:
                    _pseudoLogger.RLogInformation(message: message, caller: caller, args: args);
                    break;
            }
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return _logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _pseudoLogger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
