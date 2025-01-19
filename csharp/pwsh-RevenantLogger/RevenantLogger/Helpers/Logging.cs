using Microsoft.Extensions.Logging;
using RosettaTools.Pwsh.Text.RevenantLogger.Common;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Helpers {

    public class LoggerObject : RevenantLoggerPSCmdlet, ILogger {
        private ILogger<Type> _loggerWithType;
        private ILogger _logger;
        private ILogger _pseudoLogger;
        private bool _hasType = false;

        public ILogger<Type> Logger
        {
            get => _loggerWithType;
            set => _loggerWithType = value;
        }

        public LoggerObject(ILogger<Type> logger) {
            _loggerWithType = logger;
            _hasType = true;
            _pseudoLogger = logger;
        }

        public LoggerObject(ILogger logger) {
            _logger = logger;
            _pseudoLogger = logger;
        }


        public void Log(string message, string logLevel)
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
                    Log(message, userLogLevel);
                }
            }

            else
            {
                Log(message, (LogLevel)userShortLogLevel);
            }
        }

        public void Log(string message, LogLevel logLevel = LogLevel.Information) {
            switch (logLevel) {
                case LogLevel.Information:
                    _pseudoLogger.LogInformation(message);
                    break;
                case LogLevel.Warning:
                    _pseudoLogger.LogWarning(message);
                    break;
                case LogLevel.Error:
                    _pseudoLogger.LogError(message);
                    break;
                case LogLevel.Critical:
                    _pseudoLogger.LogCritical(message);
                    break;
                case LogLevel.Debug:
                    _pseudoLogger.LogDebug(message);
                    break;
                case LogLevel.Trace:
                    _pseudoLogger.LogTrace(message);
                    break;
                default:
                    _pseudoLogger.LogInformation(message);
                    break;
            }
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
            if (_hasType) {
                return _loggerWithType.BeginScope(state);
            }
            else {
                return _logger.BeginScope(state);
            }
        }

        public bool IsEnabled(LogLevel logLevel) {
            if (_hasType) {
                return _loggerWithType.IsEnabled(logLevel);
            }
            else {
                return _logger.IsEnabled(logLevel);
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
            _pseudoLogger.Log(logLevel, eventId, state, exception, formatter);
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


    public class FileLogProvider : RevenantLoggerPSCmdlet, ILoggerProvider {

        public ILogger CreateLogger(string categoryName) {

            return new SpectreFileLogger(LoggerConfig, categoryName);
        }

        public void Dispose() {
            return;
        }
    }

    public class SpectreFileLogger : RevenantLoggerPSCmdlet, ILogger {

        private readonly string _logPath;
        private readonly string _logFilename;
        private readonly string _logFilePath;
        private readonly SemaphoreSlim _logFileLock = new(1, 1);
        private readonly LogLevel _minimumLogLevel;
        private string _categoryName;
        private Configuration _runtimeConfig;
        private ConfigDefinition.LoggingRoot _logConfigRoot;

        internal string LogPath { get => _logPath; }

        internal string LogFilename { get => _logFilename; }

        internal string LogFilePath { get => _logFilePath; }
        protected internal SemaphoreSlim LogFileLock { get => _logFileLock; }
        internal LogLevel MinimumLogLevel { get => _minimumLogLevel; }

        internal string CategoryName
        {
            get => _categoryName;
            private set => _categoryName = value;
        }

        internal Configuration RuntimeConfig
        {
            get => _runtimeConfig;
            private set => _runtimeConfig = value;
        }

        internal ConfigDefinition.LoggingRoot LogConfigRoot
        {
            get => _logConfigRoot;
            set => _logConfigRoot = value;
        }

        public SpectreFileLogger(Configuration runtimeConfig, string categoryName) {
            _runtimeConfig = runtimeConfig;
            _logConfigRoot = runtimeConfig.RunningConfig.Logging;

            _logPath = LoggerConfig.LogPath;
            _logFilename = _logConfigRoot.LogFilename;
            _logFilePath = Path.Combine(_logPath, _logFilename);

            bool levelParse = Enum.TryParse<LogLevel>(LogConfigRoot.MinimumLogLevel, ignoreCase: true, out _minimumLogLevel);
            if (!levelParse) {
                ShortLogLevel tempLogLevel;
                levelParse = Enum.TryParse<ShortLogLevel>(LogConfigRoot.MinimumLogLevel, ignoreCase: true, out tempLogLevel);
                if (!levelParse)
                {
                    _minimumLogLevel = LogLevel.Information;
                }
                else
                {
                    _minimumLogLevel = (LogLevel)tempLogLevel;
                }
            }

            this._categoryName = categoryName;
            InitializeLog();
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
            return default;
        }

        public bool IsEnabled(LogLevel logLevel) {
            return logLevel >= _minimumLogLevel;
        }

        public async void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
            if (!IsEnabled(logLevel)) {
                return;
            }

            ShortLogLevel shortLogLevel = (ShortLogLevel)logLevel;
            DateTime userTimestamp = (LogConfigRoot.UTC) ? DateTime.UtcNow : DateTime.Now;

            var message = formatter(state, exception);
            //string fileLogState = state?.ToString() ?? String.Empty;
            //fileLogState = Markup.Remove(fileLogState);


            //message = $"[{userTimestamp.ToString(LogConfigRoot.TimestampFormat)} {shortLogLevel}] {fileLogState}";

            message = $"[{userTimestamp.ToString(LogConfigRoot.TimestampFormat)} {shortLogLevel}] {state}";


            //message = $"[{DateTime.Now.ToString(LogConfigRoot.TimestampFormat)} {shortLogLevel}] {_categoryName}(): {state}";



            await WriteToFile(message);
        }

        private async Task WriteToFile(string message) {

            try {
                await _logFileLock.WaitAsync();

                byte[] utf8Text = Encoding.UTF8.GetBytes(message + Environment.NewLine);
                using FileStream logStream = new(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096);
                await logStream.WriteAsync(utf8Text);
            }
            finally {
                _logFileLock.Release();
            }
        }

        void InitializeLog() {
#pragma warning disable CA1416 // Validate platform compatibility

            if (!Directory.Exists(LoggerConfig.LogPath)) {
                if (_runtimeConfig.IsLinux) {
                    Directory.CreateDirectory(LoggerConfig.LogPath,
                        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                        UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                        );
                }
                else {
                    Directory.CreateDirectory(LoggerConfig.LogPath);
                }
            }

            if (!File.Exists(_logFilePath)) {
                File.Create(_logFilePath, 4096, FileOptions.WriteThrough | FileOptions.RandomAccess);
            }
            else {
                File.AppendAllText(_logFilePath, "", Encoding.UTF8);
            }

#pragma warning restore CA1416 // Validate platform compatibility
        }
    }

}
