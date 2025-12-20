using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PSPhlebotomist.Core;
using PSPhlebotomist.Core.Helpers;
using PSPhlebotomist.Helpers;
using Serilog;
using Serilog.Context;
using Serilog.Extensions.Logging;
using Spectre.Console;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using static PSPhlebotomist.Common.Statics;


namespace PSPhlebotomist.Common
{
    /// <summary>
    /// Provides a container for configuring and managing dependency injection, application hosting, and logging
    /// services within the Cmdlet. Serves as a central point for accessing service providers, host builders, and
    /// logging infrastructure.
    /// </summary>
    /// <remarks>
    /// The DIContainer class encapsulates the setup and lifetime management of core application
    /// services, including dependency injection, logging, and hosting. It is used to initialize and access
    /// shared services and infrastructure components throughout the Cmdlet's lifecycle. Only one instance is
    /// intended to be active at a time, and an existing container will be reused by future calls to the Cmdlet
    /// if it's available.
    /// </remarks>
    public class DIContainer
    {
        private protected DateTime _creationTime;
        private protected IServiceCollection? _services;
        private protected IServiceProvider? _diServiceProvider;
        private protected IHostBuilder? _genericHostBuilder;
        private protected ILoggingBuilder? _loggingBuilder;
        private protected IHost? _genericHost;
        private protected ILogger<DIContainer>? _logger;
        private protected ILoggerFactory? _sharedLoggerFactory;
        private static Serilog.ILogger? _rootLogger;
        private protected PSVariable _existingDIContainerVariable = new(StaticStrings._PSVariableDIContainer, null, ScopedItemOptions.AllScope | ScopedItemOptions.None);
        private protected static LoggingColorRoot _loggingColorRoot = new LoggingColorRoot();

        /// <summary>
        /// Gets the root configured Serilog logger instance used for general Cmdlet-wide logging.
        /// </summary>
        /// <remarks>Use this property to access the global logger for logging general messages throughout the
        /// Cmdlet. Individual classes will typically grab an ILogger-abstracted class-specific instance manually via GetRequiredService
        /// or via constructor dependency injection.
        /// The returned instance may be null if the root logger has not been configured.</remarks>
        public static Serilog.ILogger? RootLoggerInstance
        {
            get
            {
                return _rootLogger;
            }
        }

        /// <summary>
        /// Gets the date and time when the object was created.
        /// </summary>
        public DateTime CreationTime { get => _creationTime; }
        
        /// <summary>
        /// Gets the underlying generic host builder used to configure and build the Cmdlet's DI host.
        /// </summary>
        public IHostBuilder? GenericHostBuilder
        {
            get => _genericHostBuilder;
            private set => _genericHostBuilder = value;
        }

        /// <summary>
        /// Gets the logging builder used to configure logging services for the Cmdlet.
        /// </summary>
        public ILoggingBuilder? LoggingBuilder
        {
            get => _loggingBuilder;
            private set => _loggingBuilder = value;
        }

        /// <summary>
        /// Gets the underlying generic host instance used to manage the Cmdlet DI container's lifetime and services.
        /// </summary>
        public IHost? GenericHost
        {
            get => _genericHost;
            private set => _genericHost = value;
        }

        /// <summary>
        /// Gets the collection of service descriptors for dependency injection configuration.
        /// </summary>
        public IServiceCollection? Services
        {
            get => _services;
            private set => _services = value;
        }

        /// <summary>
        /// Gets the dependency injection service provider used to resolve Cmdlet DI services.
        /// </summary>
        public IServiceProvider? DIServiceProvider
        {
            get => _diServiceProvider;
            private set => _diServiceProvider = value;
        }

        /// <summary>
        /// Gets the shared logger factory instance used for creating loggers throughout the Cmdlet.
        /// </summary>
        public ILoggerFactory? SharedLoggerFactory
        {
            get => _sharedLoggerFactory;
            private set => _sharedLoggerFactory = value;
        }

        /// <summary>
        /// Gets the logger instance used for recording diagnostic and operational messages for this DI container.
        /// </summary>
        public ILogger<DIContainer>? Logger
        {
            get => _logger;
            private set => _logger = value;
        }

        /// <summary>
        /// Gets the PowerShell variable that holds the existing dependency injection container instance, if available.
        /// </summary>
        /// <remarks>This property provides access to the variable used for storing the dependency
        /// injection container within the PowerShell session. The variable is created if it does not already
        /// exist.</remarks>
        public PSVariable ExistingDIContainerVariable
        {
            get
            {
                _existingDIContainerVariable ??= new(StaticStrings._PSVariableDIContainer, null, ScopedItemOptions.AllScope | ScopedItemOptions.None);
                return _existingDIContainerVariable;
            }
            private set => _existingDIContainerVariable = value;
        }

        /// <summary>
        /// Initializes a new instance of the DIContainer class and sets up the Cmdlet's dependency injection
        /// infrastructure.
        /// </summary>
        /// <remarks>This constructor configures the application's dependency injection container,
        /// logging, and console encoding. If a DIContainer instance already exists in
        /// SyringeSingletons.CmdletDIContainer, the constructor just returns without reinitializing the container
        /// or creating a duplicate. The constructor also starts the generic host asynchronously and makes the
        /// service provider and logger factory available for dependency resolution throughout the Cmdlet.
        /// </remarks>
        public DIContainer()
        {
            if (SyringeSingletons.CmdletDIContainer != null)
            {
                return;
            }

            _rootLogger ??= DICreateRootLogger();

            _genericHostBuilder = BuildAppHost();
            _genericHost = GenericHost = _genericHostBuilder.Build();
            _genericHost.RunAsync();
            _diServiceProvider = DIServiceProvider = _genericHost.Services;
            _creationTime = DateTime.Now;
            _sharedLoggerFactory = _genericHost.Services.GetRequiredService<ILoggerFactory>();
            _logger = _genericHost.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<DIContainer>>();

            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            SyringeSingletons.CmdletDIContainer = this;
        }

        /// <summary>
        /// Configures and creates a new host builder with default settings, custom configuration, logging, and service
        /// registrations for the Cmdlet.
        /// </summary>
        /// <remarks>The returned host builder includes Serilog integration for logging and registers
        /// several singleton services required by the Cmdlet.
        /// </remarks>
        /// <returns>An <see cref="IHostBuilder"/> instance preconfigured with Cmdlet-specific services, logging, and
        /// configuration providers.</returns>
        private IHostBuilder BuildAppHost()
        {
            string assmLocation = Assembly.GetExecutingAssembly().Location;
            string manifestName = Assembly.GetExecutingAssembly().ManifestModule.Name;
            string basePath = assmLocation.Replace(manifestName, "");

            IHostBuilder hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(c =>
                {
                    c.SetBasePath(basePath);
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<ILoggerFactory>(new SerilogLoggerFactory());
                })
                .ConfigureLogging((context, logging) =>
                {

                    logging.AddSerilog(SyringeSingletons.RootLoggerInstance, dispose: true)
                    .SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<Injector>();
                    services.AddSingleton<NativeMethods>();
                    services.AddSingleton<ProcessHelper>();
                    services.AddSingleton<ValidationHelper>();
                    services.AddSingleton<TriageNurse>();
                });

            return hostBuilder;
        }

        /// <summary>
        /// Initializes the Cmdlet's root Serilog logger instance and configures global logging context properties.
        /// </summary>
        /// <remarks>This method is intended to be called automatically during module initialization and
        /// should not be invoked directly. It sets up the root logger using configuration from a JSON file
        /// and establishes global context properties for logging. This ensures that logging is available and properly
        /// configured before any other code executes.
        /// 
        /// This is the configured logger instance from which all other logger instances should derive, either directly
        /// or via the Microsoft.Extensions.Logging abstractions.
        /// </remarks>
        [ModuleInitializer]
        internal static void CreateRootLogger()
        {
            string assmLocation = Assembly.GetExecutingAssembly().Location;
            string manifestName = Assembly.GetExecutingAssembly().ManifestModule.Name;
            string basePath = assmLocation.Replace(manifestName, "");
            ;
            var rootLoggerConfig = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("Config\\Logging\\Runtime\\serilog-config.json")
                    .Build();

            Log.Logger = _rootLogger = SyringeSingletons._rootLogger = new LoggerConfiguration()
                .ReadFrom.Configuration(rootLoggerConfig)
                .CreateLogger();

            Dictionary<string, bool> globalContextItems = new Dictionary<string, bool>()
            {
                { "RootLoggerCreated", true }
            };

            GlobalLogContext.PushProperty("Bools", globalContextItems);
        }

        /// <summary>
        /// Gets the root Serilog logger instance used for Cmdlet-wide general logging.
        /// </summary>
        /// <returns>The root <see cref="Serilog.ILogger"/> instance for logging across the Cmdlet.</returns>
        internal static Serilog.ILogger GetRootLogger()
        {
            return SyringeSingletons.RootLoggerInstance;
        }


        /// <summary>
        /// Initializes and returns the root Serilog logger instance. Private, class-specific method used as
        /// a wrapper around the static CreateRootLogger and GetRootLogger methods.
        /// </summary>
        /// <remarks>This method ensures that the root logger is created before returning it. The class
        /// calls this method to create & obtain the Cmdlet's primary logger for logging operations.</remarks>
        /// <returns>A <see cref="Serilog.ILogger"/> representing the root logger. The same instance is returned on subsequent
        /// calls.
        /// </returns>
        private Serilog.ILogger DICreateRootLogger()
        {
            CreateRootLogger();
            return GetRootLogger();
        }
    }
}
