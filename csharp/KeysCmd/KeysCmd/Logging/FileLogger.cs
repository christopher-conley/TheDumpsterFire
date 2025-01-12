using Microsoft.Extensions.Logging;
using RosettaTools.CLI.KeysCmd.Common;
using RosettaTools.CLI.KeysCmd.Interfaces;
using Spectre.Console;
using System.Text;

namespace RosettaTools.CLI.KeysCmd.Logging
{
    public class FileLogger : ILogger, IFileLogger
    {

        private readonly string _logPath;
        private readonly string _logFilename;
        private readonly string _logFilePath;
        private readonly SemaphoreSlim _logFileLock = new(1, 1);
        private readonly LogLevel _minimumLogLevel;
        private string categoryName;
        private IKeysCmdConfiguration _runtimeConfig;
        private ConfigDefinition.LoggingRoot _logConfigRoot;

        internal string LogPath { get => _logPath; }

        internal string LogFilename { get => _logFilename; }

        internal string LogFilePath { get => _logFilePath; }

        protected internal SemaphoreSlim LogFileLock { get => _logFileLock; }

        internal LogLevel MinimumLogLevel { get => _minimumLogLevel; }

        internal string CategoryName
        {
            get => categoryName;
            private set => categoryName = value;
        }

        internal IKeysCmdConfiguration RuntimeConfig
        {
            get => _runtimeConfig;
            private set => _runtimeConfig = value;
        }

        internal ConfigDefinition.LoggingRoot LogConfigRoot
        {
            get => _logConfigRoot;
            set => _logConfigRoot = value;
        }

        public FileLogger(IKeysCmdConfiguration runtimeConfig, string categoryName)
        {
            _runtimeConfig = runtimeConfig;
            _logConfigRoot = runtimeConfig.LoggingConfig;

            _logPath = Path.Combine(runtimeConfig.ConfigHome, LogConfigRoot.LogDirectory);
            _logFilename = LogConfigRoot.LogFilename;
            _logFilePath = Path.Combine(_logPath, _logFilename);
            bool levelParse = Enum.TryParse<LogLevel>(_logConfigRoot.MinimumLogLevel, out _minimumLogLevel);
            if (!levelParse)
            {
                _minimumLogLevel = LogLevel.Information;
            }

            this.categoryName = categoryName;
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
            DateTime userTimestamp = (LogConfigRoot.UTC) ? DateTime.UtcNow : DateTime.Now;

            var message = formatter(state, exception);
            string fileLogState = state?.ToString() ?? String.Empty;
            fileLogState = Markup.Remove(fileLogState);

            message = $"[{userTimestamp.ToString(LogConfigRoot.TimestampFormat)} {shortLogLevel}] {fileLogState}";
            //message = $"[{userTimestamp.ToString(LogConfigRoot.TimestampFormat)} {shortLogLevel}] {categoryName}(): {state}";

            await WriteToFile(message);

            if (Bootstrap.ShouldLogToConsole)
            {

                string sev = GetLogLevelColor(shortLogLevel);
                string msgColor = (shortLogLevel == ShortLogLevel.error || shortLogLevel == ShortLogLevel.crit) ? "red" : "grey85";
                string shortCategory = categoryName.LastIndexOf('.') > 0 ? categoryName.Substring(categoryName.LastIndexOf('.') + 1) : categoryName;

                AnsiConsole.MarkupLine($"[bold grey][[[grey66]{userTimestamp.ToString(LogConfigRoot.TimestampFormat)}[/] [{sev}]{shortLogLevel}[/]]][/] [bold grey46]{shortCategory}:[/] [{msgColor}]{state}[/]");
            }
        }

        private async Task WriteToFile(string message)
        {

            try
            {
                await LogFileLock.WaitAsync();

                byte[] utf8Text = Encoding.UTF8.GetBytes(message + Environment.NewLine);
                using FileStream logStream = new(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096);
                await logStream.WriteAsync(utf8Text, 0, utf8Text.Length);
            }
            finally
            {
                LogFileLock.Release();
            }
        }

        async void InitializeLog()
        {
#pragma warning disable CA1416 // Validate platform compatibility

            if (!Directory.Exists(LogPath))
            {
                if (_runtimeConfig.IsLinux)
                {
#if NET7_0_OR_GREATER
                    Directory.CreateDirectory(LogPath,
                        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                        UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                        );
#else
                    Directory.CreateDirectory(LogPath);
#endif
                }
                else
                {
                    Directory.CreateDirectory(LogPath);
                }
            }

            try
            {
                await LogFileLock.WaitAsync();

                if (!File.Exists(LogFilePath))
                {
                    using (var stream = File.Create(LogFilePath, 4096, FileOptions.WriteThrough | FileOptions.RandomAccess | FileOptions.Asynchronous))
                    {
                        await stream.FlushAsync();
                    }
                }
                if (!File.Exists(LogFilePath))
                {
                    using (var stream = File.Create(LogFilePath, 4096, FileOptions.WriteThrough | FileOptions.RandomAccess | FileOptions.Asynchronous))
                    {
                        await stream.FlushAsync();
                    }
                }
                else
                {
                    File.AppendAllText(LogFilePath, "", Encoding.UTF8);
                }
            }

            finally
            {
                LogFileLock.Release();
            }

#pragma warning restore CA1416 // Validate platform compatibility
        }


        private string GetLogLevelColor(ShortLogLevel logLevel)
        {
            return logLevel switch
            {
                ShortLogLevel.trace => "blue",
                ShortLogLevel.debug => "purple",
                ShortLogLevel.info => "green",
                ShortLogLevel.warn => "yellow",
                ShortLogLevel.error => "red",
                ShortLogLevel.crit => "red",
                ShortLogLevel.none => "white",
                _ => "white",
            };
        }

    }

}
