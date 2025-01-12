using Microsoft.Extensions.Logging;

namespace RosettaTools.CLI.KeysCmd.Logging
{
    internal class LoggerCreation
    {
        public static (FileLogILogProvider, ILoggerFactory) NewLoggerFactory()
        {
            FileLogILogProvider logProvider = new();
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.Configure(config =>
                {
                });
                builder.AddProvider(logProvider);
            });

            return (logProvider, loggerFactory);
        }

        public static ILoggerFactory NewLoggerFactory(bool noIlogProvider)
        {
            (FileLogILogProvider? logProvider, ILoggerFactory loggerFactory) = NewLoggerFactory();
            return loggerFactory;
        }
        public static ILogger? NewLogger<TLogger>() where TLogger : class
        {

            (var logProvider, var loggerFactory) = NewLoggerFactory();

            var builtLogger = loggerFactory?.CreateLogger(typeof(TLogger));
            return builtLogger;
        }

        public static ILogger? NewLogger<TLogger>(ILoggerFactory? factory) where TLogger : class
        {
            var builtLogger = factory?.CreateLogger(typeof(TLogger));
            return builtLogger;
        }
    }
}
