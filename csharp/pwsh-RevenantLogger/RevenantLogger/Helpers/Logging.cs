using Microsoft.Extensions.Logging;
using RosettaTools.Pwsh.Text.RevenantLogger.Common;
using RosettaTools.Pwsh.Text.RevenantLogger.Common.ExtensionMethods;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Helpers
{
    public class LoggerObject : RevenantLoggerPSCmdlet, ILogger {
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

        public LoggerObject(ILogger logger) {
            _logger = logger;
            _pseudoLogger = logger;
        }

        public LoggerObject(ILogger<LoggerObject> logger, IRevenantConfiguration config)
        {
            _logger = _pseudoLogger = logger;
            _config = config;

        }


        public void Log(string message, string logLevel, [CallerMemberName] string? caller = null)
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
                    Log(message, userLogLevel, caller: caller);
                }
            }

            else
            {
                Log(message, shortlogLevel: userShortLogLevel, useShortLogLevel: true, caller: caller);
            }
        }

        public void Log(
            string message,
            LogLevel logLevel = LogLevel.Information,
            ShortLogLevel shortlogLevel = ShortLogLevel.info,
            bool useShortLogLevel = false,
            [CallerMemberName] string? caller = null
            ) {

            LogLevel switchLogLevel = (useShortLogLevel) ? (LogLevel)shortlogLevel : logLevel;

            switch (switchLogLevel) {
                case LogLevel.Information:
                    _pseudoLogger.RLogInformation(message: message, caller: caller);
                    break;
                case LogLevel.Warning:
                    _pseudoLogger.RLogWarning(message: message, caller: caller);
                    break;
                case LogLevel.Error:
                    _pseudoLogger.RLogError(message: message, caller: caller);
                    break;
                case LogLevel.Critical:
                    _pseudoLogger.RLogCritical(message: message, caller: caller);
                    break;
                case LogLevel.Debug:
                    _pseudoLogger.RLogDebug(message: message, caller: caller);
                    break;
                case LogLevel.Trace:
                    _pseudoLogger.RLogTrace(message: message, caller: caller);
                    break;
                default:
                    _pseudoLogger.RLogInformation(message: message, caller: caller);
                    break;
            }
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
            return _logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel) {
            return _logger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
            _pseudoLogger.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    public class FileLogProvider : RevenantLoggerPSCmdlet, ILoggerProvider, IFileLogProvider
    {
        private readonly ILogger<FileLogProvider>? _logger;
        private RevenantFileLogger? _builtLogger;

        public IRevenantConfiguration LoggingConfig { get => _config; }
        public ILogger<FileLogProvider>? Logger { get => _logger; }

        public FileLogProvider()
        {
            
        }
        public FileLogProvider(IRevenantConfiguration config)
        {
            _config = config;
        }

        public FileLogProvider(ILogger<FileLogProvider> logger, IRevenantConfiguration config)
        {
            _logger = logger;
            _config = config;
        }
        public ILogger CreateLogger(string? categoryName = null)
        {

            _builtLogger = new RevenantFileLogger(LoggingConfig, categoryName);
            return _builtLogger;
        }

        public async void Dispose()
        {
            GC.SuppressFinalize(this);

            if (null == _builtLogger)
            {
                return;
            }

            try
            {
                await _builtLogger.LogFileLock.WaitAsync();
                using FileStream stream = File.Open(_builtLogger.LogFilePath, FileMode.Append);
                await stream.FlushAsync();
            }
            finally
            {
                _builtLogger.LogFileLock.Release();
            }

            _builtLogger = null;
        }
    }

    public class RevenantFileLogger : RevenantLoggerPSCmdlet, ILogger, IRevenantFileLogger
    {

        private readonly string _logPath;
        private readonly string _logFilename;
        private readonly string _logFilePath;
        private readonly SemaphoreSlim _logFileLock = new(1, 1);
        private readonly LogLevel _minimumLogLevel;
        private string? _categoryName;
        private IRevenantConfiguration _runtimeConfig;
        private ConfigDefinition.LoggingRoot _logConfigRoot;

        internal string LogPath { get => _logPath; }

        internal string LogFilename { get => _logFilename; }

        internal string LogFilePath { get => _logFilePath; }
        protected internal SemaphoreSlim LogFileLock { get => _logFileLock; }
        internal LogLevel MinimumLogLevel { get => _minimumLogLevel; }

        internal string? CategoryName
        {
            get => _categoryName;
            private set => _categoryName = value;
        }

        internal IRevenantConfiguration RuntimeConfig
        {
            get => _runtimeConfig;
            private set => _runtimeConfig = value;
        }

        internal ConfigDefinition.LoggingRoot LogConfigRoot
        {
            get => _logConfigRoot;
            set => _logConfigRoot = value;
        }

        public RevenantFileLogger(IRevenantConfiguration runtimeConfig, string? categoryName = null)
        {
            _runtimeConfig = runtimeConfig;
            _logConfigRoot = runtimeConfig.RunningConfig.Logging;

            if (Path.IsPathRooted(_logConfigRoot.LogDirectory))
            {
                _logPath = _logConfigRoot.LogDirectory;
            }
            else
            {
                _logPath = Path.Combine(runtimeConfig.ConfigHome, _logConfigRoot.LogDirectory);
            }
            _logFilename = _logConfigRoot.LogFilename;
            _logFilePath = Path.Combine(_logPath, _logFilename);

            bool levelParse = Enum.TryParse<LogLevel>(LogConfigRoot.MinimumLogLevel, ignoreCase: true, out _minimumLogLevel);
            if (!levelParse)
            {
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

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return default;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _minimumLogLevel;
        }

        public async void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            ShortLogLevel shortLogLevel = (ShortLogLevel)logLevel;
            var message = formatter(state, exception);

            DateTime userTimestamp = (LogConfigRoot.UTC) ? DateTime.UtcNow : DateTime.Now;
            string formattedTimestamp = $"{userTimestamp.ToString(LogConfigRoot.DateFormat)}{Sep.Value}{userTimestamp.ToString(LogConfigRoot.TimeFormat)}";
            //string fileLogState = state?.ToString() ?? String.Empty;
            //fileLogState = Markup.Remove(fileLogState);


            //message = $"[{userTimestamp.ToString(LogConfigRoot.TimestampFormat)} {shortLogLevel}] {fileLogState}";

            message = $"[{formattedTimestamp} {shortLogLevel}] {state}";


            //message = $"[{DateTime.Now.ToString(LogConfigRoot.TimestampFormat)} {shortLogLevel}] {_categoryName}(): {state}";

            await WriteToFile(message);
        }

        private async Task WriteToFile(string message)
        {

            try
            {
                await _logFileLock.WaitAsync();

                byte[] utf8Text = Encoding.UTF8.GetBytes(message + Environment.NewLine);
                using FileStream logStream = new(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096);
                await logStream.WriteAsync(utf8Text);
            }
            finally
            {
                _logFileLock.Release();
            }
        }

        async void InitializeLog()
        {
#pragma warning disable CA1416 // Validate platform compatibility

            if (!Directory.Exists(LogPath))
            {
                if (_runtimeConfig.IsLinux)
                {
                    Directory.CreateDirectory(LogPath,
                        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                        UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                        );
                }
                else
                {
                    Directory.CreateDirectory(LogPath);
                }
            }
            try
            {
                await _logFileLock.WaitAsync();

                if (!File.Exists(_logFilePath))
                {
                    using (var stream = File.Create(_logFilePath, 4096, FileOptions.WriteThrough | FileOptions.RandomAccess | FileOptions.Asynchronous))
                    {
                        await stream.FlushAsync();
                    }
                }
                else
                {
                    File.AppendAllText(_logFilePath, "", Encoding.UTF8);
                }
            }

            finally
            {
                _logFileLock.Release();
            }

#pragma warning restore CA1416 // Validate platform compatibility
        }
    }
}
