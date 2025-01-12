using Microsoft.Extensions.Logging;
using RosettaTools.CLI.KeysCmd.Interfaces;

namespace RosettaTools.CLI.KeysCmd.Logging
{
    public class FileLogILogProvider : ILoggerProvider, IFileLogILogProvider
    {

        private static FileLogger? _loggerInstance;
        private static IKeysCmdConfiguration? Config { get; set; }


        public FileLogILogProvider()
        {
            Config = Bootstrap.Config;
        }
        public ILogger CreateLogger(string categoryName)
        {
            if (null == Config)
            {
                Config = Bootstrap.Config;
            }

            _loggerInstance = new FileLogger(Config, categoryName);
            return _loggerInstance;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            DisposeLogger(_loggerInstance);
            return;
        }

        private async void DisposeLogger(FileLogger? logger)
        {
            if (null == logger)
            {
                return;
            }
            try
            {
                await logger.LogFileLock.WaitAsync();

                using FileStream stream = File.Open(logger.LogFilePath, FileMode.Append);
                await stream.FlushAsync();
            }
            finally
            {
                logger.LogFileLock.Release();
            }
            logger = null;
        }
    }
}
