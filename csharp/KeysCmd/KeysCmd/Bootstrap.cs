using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RosettaTools.CLI.KeysCmd.Common;
using RosettaTools.CLI.KeysCmd.Common.ExtensionMethods;
using RosettaTools.CLI.KeysCmd.Helpers;
using RosettaTools.CLI.KeysCmd.Interfaces;
using RosettaTools.CLI.KeysCmd.Logging;
using RosettaTools.CLI.KeysCmd.SpectreCLI;
using RosettaTools.CLI.KeysCmd.SpectreCLI.Meta;
using Spectre.Console.Cli;
using static RosettaTools.CLI.KeysCmd.Common.Utilities;

namespace RosettaTools.CLI.KeysCmd
{
    /// <summary>
    /// Class that bootstraps the application, including initial setup of logging, configuration, and command-line parsing.
    /// <br></br>
    /// <br>Contains the following methods:</br>
    /// <list type="bullet">
    /// <item><see cref="Main(string[])" /></item>
    /// <item><see cref="DoCleanup(int)" /></item>
    /// <item><see cref="BuildAppHost(ILoggerFactory?)" /></item>
    /// </list>
    /// </summary>
    internal class Bootstrap
    {

        /// <summary>
        /// <see langword="private" /> class variable backing the <see cref="Config"/> <see langword="protected internal" /> property.
        /// Instantiates a new <see cref="Configuration" /> object.
        /// </summary>
        private static Configuration _config = new();

        /// <summary>
        /// <see langword="private" /> class variable which contains the username of the user running the application. 
        /// Its value is ultimately determined by the <see cref="Configuration.WhoAmI" /> property.
        /// <br></br>
        /// <br>In this class, this variable is used to log the username of the user running the application. OpenSSH can be
        /// finicky with permissions, so it's handy to have this information in the log.</br>
        /// </summary>
        private static readonly string? WhoAmI = Config.WhoAmI;

        /// <summary>
        /// Custom <see cref="ILoggerProvider" /> to enable logging to a file.
        /// </summary>
        private static FileLogILogProvider? logProvider;

        /// <summary>
        /// A shared <see cref="ILoggerFactory" /> instance for the application. This instance is
        /// <br>used as a Singleton in the DI container for injection into dependent classes.</br>
        /// </summary>
        private static ILoggerFactory? loggerFactory;

        /// <summary>
        /// <see langword="private" /> class variable backing the <see cref="Logger"/> <see langword="protected internal" /> property.
        /// <br>Contains an <see cref="ILogger" /> instance used as a Singleton in the DI container.</br>
        /// </summary>
        private static ILogger? _logger;

        /// <summary>
        /// <see langword="private" /> class variable backing the <see cref="ShouldLogToConsole"/> <see langword="public static" /> property.
        /// <br>This determines whether logging/other messages should be echoed to the console or not. When OpenSSH sends a request</br>
        /// <br>via "AuthorizedKeysCommand," it expects to receive back nothing but the keys, so anything else written to the console</br>
        /// <br>would get jumbled into that and interpreted as a key. This setting ensures that nothing but keys get written to stdout</br>
        /// <br>when the -o/--openssh commandline option is used while still enabling loging messages to be written to disk.</br>
        /// </summary>
        private static bool _shouldLogToConsole = true;

        /// <summary>
        /// <see langword="protected internal static" /> class variable backing the <see cref="GenericHost"/> <see langword="public static" /> property.
        /// <br>Holds a .NET generic host used for DI and the SpectreCLI CLI app.</br>
        /// </summary>
        protected internal static IHostBuilder? _genericHost;

        /// <summary>
        /// <see langword="protected internal static" /> class variable backing the <see cref="AppTypeRegistrar"/> <see langword="public static" /> property.
        /// <br>Contains the SpectreCLI CLI Type registrar.</br>
        /// </summary>
        protected internal static TypeRegistrar? _appTypeRegistrar;

        /// <summary>
        /// <see langword="protected internal static" /> class variable backing the <see cref="SpectreCommandApp"/> <see langword="public static" /> property.
        /// <br>Holds the built SpectreCLI CLI app which parses commandline arguments and generally controls program control flow.</br>
        /// </summary>
        protected internal static CommandApp? _spectreCommandApp;

        /// <summary>
        /// Holds configuration settings for the application.
        /// <br></br><br>See also: <see cref="_config"/> and <seealso cref="Configuration" /> for more information.</br>
        /// </summary>
        protected internal static Configuration Config
        {
            get
            {
                _config ??= new Configuration();
                return _config;
            }
            set => _config = value;
        }

        /// <summary>
        /// An <see cref="ILogger" /> instance used as a Singleton in the DI container.
        /// <br></br><br>See also: <seealso cref="_logger"/></br>
        /// </summary>
        protected internal static ILogger? Logger { get => _logger; protected set => _logger = value; }

        /// <summary>
        /// A .NET generic host used for DI and the SpectreCLI CLI app.
        /// <br></br><br>See also: <seealso cref="_genericHost"/></br>
        /// </summary>
        public static IHostBuilder? GenericHost { get => _genericHost; protected internal set => _genericHost = value; }

        /// <summary>
        /// A SpectreCLI CLI Type registrar used for dependency registration and resolution.
        /// <br></br><br>See also: <seealso cref="_appTypeRegistrar"/></br>
        /// </summary>
        public static TypeRegistrar? AppTypeRegistrar { get => _appTypeRegistrar; protected internal set => _appTypeRegistrar = value; }

        /// <summary>
        /// Holds the SpectreCLI CLI app, which parses commandline arguments and generally controls program control flow.
        /// <br></br><br>See also: <seealso cref="_spectreCommandApp"/></br>
        /// </summary>
        public static CommandApp? SpectreCommandApp { get => _spectreCommandApp; protected internal set => _spectreCommandApp = value; }

        /// <summary>
        /// Controls whether logging/other messages should be echoed to the console or not. If false, only ssh keys will be written to stdout.
        /// <br>This is to ensure bad data doesn't get passed to OpenSSH while still being able to log to a file.</br>
        /// <br></br><br>See also: <seealso cref="_shouldLogToConsole"/></br>
        /// </summary>
        public static bool ShouldLogToConsole { get => _shouldLogToConsole; protected internal set => _shouldLogToConsole = value; }

        /// <summary>
        /// Entrypoint and bootstrapping method for the application. Builds the .NET generic host, sets up logging, and configures/runs the SpectreCLI CLI app.
        /// </summary>
        /// <param name="args">The args<see cref="string"/>[]</param>
        static void Main(string[] args)
        {
            string[] lowerArgs = Array.ConvertAll(args, x => x.ToLower());

            // If no args were provided, don't log startup/end messages to console.
            // If args were provided, but they're help or version flags, don't log
            // startup/end messages to console. Just an explanation because I know the
            // conditional below is gnarly.

            if (
                (null == lowerArgs || lowerArgs.Length == 0) ||
                ((null != lowerArgs && lowerArgs.Length > 0) &&
                (lowerArgs.Contains("-o") || lowerArgs.Contains("--openssh") ||
                lowerArgs.Contains("-h") || lowerArgs.Contains("--help") ||
                lowerArgs.Contains("-v") || lowerArgs.Contains("--version") ||
                lowerArgs.Contains("-?") || lowerArgs.Contains("--?") ||
                lowerArgs.Contains("config") || lowerArgs.Contains("configuration") ||
                lowerArgs.Contains("settings")
                ))
                )
            {
                ShouldLogToConsole = false;
            }

            (logProvider, loggerFactory) = LoggerCreation.NewLoggerFactory();

            if (Config.LoggingEnabled)
            {
                _logger = LoggerCreation.NewLogger<Bootstrap>(loggerFactory);
            }

            GenericHost = BuildAppHost(loggerFactory);

            Logger?.BeginScope(FormatCaller());
            Logger?.CLogInformation("-- KeysCmd start --", FormatCaller());
            Logger?.CLogInformation("KeysCmd running as user: [bold darkgoldenrod]{WhoAmI}[/]", FormatCaller(), WhoAmI);

            AppTypeRegistrar = new TypeRegistrar(GenericHost);
            SpectreCommandApp = new CommandApp(AppTypeRegistrar);

            SpectreCommandApp.Configure(c =>
            c.PropagateExceptions()
            .SetApplicationName((string.IsNullOrEmpty(AppDomain.CurrentDomain.FriendlyName)) ? "KeysCmd" : AppDomain.CurrentDomain.FriendlyName)
            .UseAssemblyInformationalVersion()
            .AddBranch<GetKeysSettings>(AppInfo.KeysBranch.BranchName, config =>
            {
                config.SetDescription(AppInfo.KeysBranch.Description);
                config.AddCommand<GetKeysCommand>(AppInfo.KeysBranch.Commands.Get.CommandName)
                .WithDescription(AppInfo.KeysBranch.Commands.Get.Description)
                .WithAlias(AppInfo.KeysBranch.Commands.Get.Aliases[0])
                .WithAlias(AppInfo.KeysBranch.Commands.Get.Aliases[1])
                .WithAlias(AppInfo.KeysBranch.Commands.Get.Aliases[2]);

                if (null != AppInfo.KeysBranch.Commands.Get.Examples)
                {
                    foreach (string example in AppInfo.KeysBranch.Commands.Get.Examples)
                    {
                        config.AddExample(example.ToFormattableStringArray());
                    }
                }

            })
            .WithAlias(AppInfo.KeysBranch.Aliases[0])
            .WithAlias(AppInfo.KeysBranch.Aliases[1]));

            SpectreCommandApp.Configure(c =>
            c.AddBranch<ConfigSettings>(AppInfo.ConfigBranch.BranchName, config =>
            {
                config.SetDescription(AppInfo.ConfigBranch.Description);
                config.AddCommand<ShowConfigCommand>(AppInfo.ConfigBranch.Commands.Show.CommandName)
                .WithDescription(AppInfo.ConfigBranch.Commands.Show.Description)
                .WithAlias(AppInfo.ConfigBranch.Commands.Show.Aliases[0])
                .WithAlias(AppInfo.ConfigBranch.Commands.Show.Aliases[1]);

                if (null != AppInfo.ConfigBranch.Commands.Show.Examples)
                {
                    foreach (string example in AppInfo.ConfigBranch.Commands.Show.Examples)
                    {
                        config.AddExample(example.ToFormattableStringArray());
                    }
                }

                config.AddCommand<EditConfigCommand>(AppInfo.ConfigBranch.Commands.Edit.CommandName)
                .WithDescription(AppInfo.ConfigBranch.Commands.Edit.Description)
                .WithAlias(AppInfo.ConfigBranch.Commands.Edit.Aliases[0])
                .WithAlias(AppInfo.ConfigBranch.Commands.Edit.Aliases[1]);

                if (null != AppInfo.ConfigBranch.Commands.Edit.Examples)
                {
                    foreach (string example in AppInfo.ConfigBranch.Commands.Edit.Examples)
                    {
                        config.AddExample(example.ToFormattableStringArray());
                    }
                }

                config.AddCommand<NewDefaultConfigCommand>(AppInfo.ConfigBranch.Commands.New.CommandName)
                .WithDescription(AppInfo.ConfigBranch.Commands.New.Description)
                .WithAlias(AppInfo.ConfigBranch.Commands.New.Aliases[0])
                .WithAlias(AppInfo.ConfigBranch.Commands.New.Aliases[1]);

                if (null != AppInfo.ConfigBranch.Commands.New.Examples)
                {
                    foreach (string example in AppInfo.ConfigBranch.Commands.New.Examples)
                    {
                        config.AddExample(example.ToFormattableStringArray());
                    }
                }
            })
            .WithAlias(AppInfo.ConfigBranch.Aliases[0])
            .WithAlias(AppInfo.ConfigBranch.Aliases[1]));

            int returnCode = SpectreCommandApp.Run(args);
            DoCleanup(returnCode);

            Environment.Exit(returnCode);
        }

        /// <summary>
        /// Method used to log error/success status to the logfile and dispose of the logger and logger factory.
        /// </summary>
        /// <param name="exitCode">The exitCode<see cref="int"/></param>
        /// <returns><see cref="Void"/></returns>
        private static void DoCleanup(int exitCode)
        {
            string exitVerbiage;
            if (exitCode == 0)
            {
                exitVerbiage = "(ok), exit code";
            }
            else
            {
                exitVerbiage = "(error), non-zero exit code";
            }

            Logger?.BeginScope(FormatCaller());
            if (exitCode == 0)
            {
                Logger?.CLogInformation("Run end {exitVerbiage}: {exitCode}", FormatCaller(), exitVerbiage, exitCode);
            }
            else
            {
                Logger?.CLogError("Run end {exitVerbiage}: {exitCode}", FormatCaller(), exitVerbiage, exitCode);
            }

            Logger?.CLogInformation("-- KeysCmd end --\n", FormatCaller());
            loggerFactory?.Dispose();
            logProvider?.Dispose();

            return;
        }

        /// <summary>
        /// Method which builds the .NET generic host used for DI and the SpectreCLI CLI app.
        /// </summary>
        /// <param name="sharedFactory">An instantiated <see cref="ILoggerFactory"/> object used to build <see cref="ILogger"/> instances.</param>
        /// <returns><see cref="IHostBuilder"/></returns>


        internal static IHostBuilder BuildAppHost(ILoggerFactory? sharedFactory)
        {
            if (null == sharedFactory)
            {
                sharedFactory = LoggerCreation.NewLoggerFactory(noIlogProvider: true);
            }

            string basePath = Directory.GetCurrentDirectory();
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(c =>
                {
                    c.SetBasePath(basePath);
                })
                .ConfigureHostOptions(options =>
                {
                    options.ShutdownTimeout = TimeSpan.FromSeconds(15);
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = Config.RunningConfig.Logging.TimestampFormat;
                    });
                    builder.ClearProviders();
                    builder.AddProvider(new FileLogILogProvider());
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<ILoggerFactory>(sharedFactory);
                    services.AddSingleton<IFileLogILogProvider, FileLogILogProvider>();
                    services.AddSingleton<IFileLogger, FileLogger>();
                    services.AddSingleton<ICommandInterceptor, BaseInterceptor>();
                    services.AddSingleton<IKeysCmdConfiguration, Configuration>();
                    services.AddSingleton<ILDAPHelper, LDAPHelper>();
                });

            if (Config.LoggingEnabled && (null != Logger))
            {
                hostBuilder.ConfigureServices((context, services) =>
                {
                    services.AddSingleton<ILogger>(Logger);
                });
            }

            return hostBuilder;
        }
    }
}
