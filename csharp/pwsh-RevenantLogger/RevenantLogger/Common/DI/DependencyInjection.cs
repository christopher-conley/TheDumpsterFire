using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using RosettaTools.Pwsh.Text.RevenantLogger.Common;
using RosettaTools.Pwsh.Text.RevenantLogger.Interfaces;
using RosettaTools.Pwsh.Text.RevenantLogger.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RosettaTools.Pwsh.Text.RevenantLogger.Common.StaticStrings;
using Spectre.Console.Cli;
using RosettaTools.Pwsh.Text.RevenantLogger.Common.ExtensionMethods;
using Vertical.SpectreLogger.Rendering;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Common
{
    internal class DIContainer : RevenantLoggerBase
    {
        private DateTime _creationTime;
        private IServiceCollection? _services;
        private IServiceProvider _diServiceProvider;
        private IHostBuilder? _genericHostBuilder;
        private ILoggingBuilder? _loggingBuilder;
        private ILoggerFactory? _sharedLoggerFactory;
        private IHost? _genericHost;
        private ILogger<DIContainer>? _logger;
        private IRevenantConfiguration? _config;
        private PSVariable _existingDIContainerVariable = new(_PSVariableDIContainer, null, ScopedItemOptions.AllScope | ScopedItemOptions.None);

        public DateTime CreationTime { get => _creationTime; }
        protected internal IHostBuilder? GenericHostBuilder
        {
            get => _genericHostBuilder;
            private set => _genericHostBuilder = value;
        }

        protected internal ILoggingBuilder? LoggingBuilder
        {
            get => _loggingBuilder;
            private set => _loggingBuilder = value;
        }
        protected internal IHost? DGenericHost
        {
            get => _genericHost;
            private set => _genericHost = value;
        }
        protected internal IServiceCollection? Services
        {
            get => _services;
            private set => _services = value;
        }

        protected internal IServiceProvider DDIServiceProvider
        {
            get => _diServiceProvider;
            private set => _diServiceProvider = value;
        }
        protected internal ILoggerFactory? DSharedLoggerFactory
        {
            get => _sharedLoggerFactory;
            private set => _sharedLoggerFactory = value;
        }
        protected internal ILogger<DIContainer>? Logger
        {
            get => _logger;
            private set => _logger = value;
        }

        protected internal IRevenantConfiguration? Config
        {
            get {
                _config ??= new Configuration();
                return _config;
            }
            private set => _config = value;
        }
        protected internal PSVariable ExistingDIContainerVariable
        {
            get {
                _existingDIContainerVariable ??= new(_PSVariableDIContainer, null, ScopedItemOptions.AllScope | ScopedItemOptions.None);
                return _existingDIContainerVariable;
            }
            private set => _existingDIContainerVariable = value;
        }


        public DIContainer(SessionState sessionState)
        {
            _existingDIContainerVariable = new(_PSVariableDIContainer, null, ScopedItemOptions.AllScope | ScopedItemOptions.None);
            CmdletDIContainer = GetExistingPSVariable<DIContainer>(SessionState: sessionState, psVariable: _PSVariableDIContainer);
            //CmdletDIContainer ??= new Bootstrap(SessionState: this.SessionState);
            if (null != CmdletDIContainer)
            {
                //foreach (var prop in CmdletDIContainer.GetType().GetProperties())
                //{
                //    prop.SetValue(this, prop.GetValue(CmdletDIContainer));
                //}
                _creationTime = CmdletDIContainer.CreationTime;
                _genericHost = GenericHost = CmdletDIContainer.DGenericHost;
                _genericHost?.RunAsync();

                _config = CmdletDIContainer.Config;

                // in case the config changed between the last time a Cmdlet was ran and now
                _config?.LoadConfig();

                _logger = CmdletDIContainer.Logger;
                _genericHostBuilder = CmdletDIContainer.GenericHostBuilder;
                _services = CmdletDIContainer.Services;
                _diServiceProvider = DIServiceProvider = CmdletDIContainer.DDIServiceProvider;
                _sharedLoggerFactory = SharedLoggerFactory = CmdletDIContainer.DSharedLoggerFactory;
                return;
            }

            //_services = NewServiceCollection();
            _genericHostBuilder = BuildAppHost();
            _genericHost = GenericHost = _genericHostBuilder.Build();
            _genericHost.RunAsync();
            _diServiceProvider = DIServiceProvider = _genericHost.Services;
            //_diServiceProvider = _services.BuildServiceProvider();
            _config = RevenantConfig = DIServiceProvider.GetRequiredService<IRevenantConfiguration>();


            if (_config.LoggingConfig.UTC)
            {
                _creationTime = DateTime.UtcNow;
            }
            else
            {
                _creationTime = DateTime.Now;
            }

            //_sharedLoggerFactory = DDIServiceProvider.GetRequiredService<ILoggerFactory>();
            //_sharedLoggerFactory = Utilities.NewLoggerFactory(_config);

            //_logger = _sharedLoggerFactory.CreateLogger<DIContainer>();
            //var _logger2 = Utilities.NewLogger(type: typeof(DIContainer), factory: _sharedLoggerFactory);
            //var _diLoggerFactory = _diServiceProvider.GetRequiredService<ILoggerFactory>();
            //var _logger2 = _diLoggerFactory.CreateLogger(typeof(DIContainer).ToString());
            var _logger2 = SharedLoggerFactory?.CreateLogger(categoryName: "DIContainer");
            AddToLoggersList<DIContainer>(_logger2);

            _logger2.RLogInformation("DIContainer created.");
            _logger2.Log(LogLevel.Information, "Raw log call in DI constructor, DIContainer created.");
            //_sharedLoggerFactory = NewLoggerFactory(_config);

        }

        private ILoggingBuilder NewLoggingBuilder()
        {
            _config ??= new Configuration();
            return NewLoggingBuilder(_config);
        }

        private ILoggingBuilder NewLoggingBuilder(IRevenantConfiguration Config)
        {
            LoggerFactory.Create(builder => {
                builder.ClearProviders();
                builder.AddSpectreConsole(config => {
                    config.AddTemplateRenderers()
                    .WriteInForeground()
                    .ConfigureProfiles(profiles => {
                        profiles.PreserveMarkupInFormatStrings = true;
                        profiles.AddTypeStyle<SuccessMessage>("[green1]");
                        profiles.AddTypeStyle<WarnMessage>("[yellow1]");
                        profiles.AddTypeStyle<FailMessage>("[red1]");
                    });
                });
                builder.AddProvider(new FileLogProvider(config: RevenantConfig));
                _loggingBuilder = builder;
            });

            return _loggingBuilder;
        }

        private ILoggerFactory NewLoggerFactory(ILoggingBuilder loggingBuilder)
        {
            return LoggerFactory.Create(builder => {
                builder.ClearProviders();
                builder = loggingBuilder;
            });
        }
        private ILoggerFactory NewLoggerFactory()
        {

            return LoggerFactory.Create(builder => {
                builder.ClearProviders();
                builder.AddSpectreConsole(config => {
                    config.AddTemplateRenderers()
                    .WriteInForeground()
                    .ConfigureProfiles(profiles => {
                        profiles.PreserveMarkupInFormatStrings = true;
                        profiles.AddTypeStyle<SuccessMessage>("[green1]");
                        profiles.AddTypeStyle<WarnMessage>("[yellow1]");
                        profiles.AddTypeStyle<FailMessage>("[red1]");
                    });
                });
                builder.AddProvider(new FileLogProvider());
            });
        }
        private ILoggerFactory NewLoggerFactory(IRevenantConfiguration LoggerConfig)
        {
            string timestampFormat = LoggerConfig.LoggingConfig.TimestampFormat;
            StringBuilder outputTemplate = new("[bold grey][[[grey66]{DateTime:");
            outputTemplate.Append($"{timestampFormat}");
            outputTemplate.Append("}[/] {LogLevel} ]][/] [bold grey46]{Category}:[/] {Message}");

            return LoggerFactory.Create(builder => {
                builder.ClearProviders();
                if (LoggerConfig.FileLoggingEnabled)
                {
                    builder.AddProvider(new FileLogProvider(config: LoggerConfig));
                }
                builder.AddSpectreConsole(config => {
                    config.AddTemplateRenderers()
                    .WriteInForeground()
                    .ConfigureProfiles(profiles => {
                        profiles.OutputTemplate = outputTemplate.ToString();
                        profiles.PreserveMarkupInFormatStrings = true;
                        profiles.AddTypeStyle<SuccessMessage>("[green1]");
                        profiles.AddTypeStyle<WarnMessage>("[yellow1]");
                        profiles.AddTypeStyle<FailMessage>("[red1]");
                        profiles.ConfigureOptions<DateTimeRenderer.Options>(renderer => {
                            if (LoggerConfig.LoggingConfig.UTC)
                            {
                                renderer.ValueFactory = () => DateTime.UtcNow;
                            }
                            else
                            {
                                renderer.ValueFactory = () => DateTime.Now;
                            }
                        });
                    });
                });
            });
        }

        //private IHostBuilder BuildAppHost(IServiceCollection serviceCollection)
        //{
        //    _config ??= new Configuration();
        //    ILoggerFactory internalFactory = _sharedLoggerFactory ?? NewLoggerFactory(_config);
        //    return BuildAppHost(internalFactory, serviceCollection);
        //}

        private IHostBuilder BuildAppHost()
        {
            _config ??= new Configuration();
            //_loggingBuilder ??= NewLoggingBuilder(_config);
            //_sharedLoggerFactory ??= NewLoggerFactory(_loggingBuilder);

            //_sharedLoggerFactory = NewLoggerFactory(_config);
            _sharedLoggerFactory = SharedLoggerFactory = Utilities.NewLoggerFactory(_config);

            //string basePath = Directory.GetCurrentDirectory();
            string basePath = _config.ConfigHome;
            string timestampFormat = _config.LoggingConfig.TimestampFormat;
            StringBuilder outputTemplate = new("[bold grey][[[grey66]{DateTime:");
            outputTemplate.Append($"{timestampFormat}");
            outputTemplate.Append("}[/] {LogLevel} ]][/] [bold grey46]{Category}:[/] {Message}\n");

            IHostBuilder hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(c => {
                    c.SetBasePath(basePath);
                })
                .ConfigureHostOptions(options => {
                    options.ShutdownTimeout = TimeSpan.FromSeconds(15);
                })
                //.ConfigureLogging(builder => {
                //    builder.ClearProviders();
                //    if (_config.FileLoggingEnabled)
                //    {
                //        builder.AddProvider(new FileLogProvider(config: RevenantConfig));
                //    }
                //    builder.AddSpectreConsole(config => {
                //        config.AddTemplateRenderers()
                //        .WriteInForeground()
                //        .ConfigureProfiles(profiles => {
                //            profiles.OutputTemplate = outputTemplate.ToString();
                //            profiles.PreserveMarkupInFormatStrings = true;
                //            profiles.AddTypeStyle<SuccessMessage>("[green1]");
                //            profiles.AddTypeStyle<WarnMessage>("[yellow1]");
                //            profiles.AddTypeStyle<FailMessage>("[red1]");
                //            profiles.ConfigureOptions<DateTimeRenderer.Options>(renderer => {
                //                if (RevenantConfig.LoggingConfig.UTC)
                //                {
                //                    renderer.ValueFactory = () => DateTime.UtcNow;
                //                }
                //                else
                //                {
                //                    renderer.ValueFactory = () => DateTime.Now;
                //                }
                //            });
                //        });
                //    });
                //})
                .ConfigureServices((context, services) => {
                    services.AddSingleton<IRevenantConfiguration, Configuration>();
                    services.AddSingleton<ILoggerFactory>(_sharedLoggerFactory);
                    services.AddSingleton<ILoggerProvider, FileLogProvider>();
                    services.AddSingleton<IFileLogProvider, FileLogProvider>();
                    services.AddSingleton<IRevenantFileLogger, RevenantFileLogger>();
                    services.AddSingleton<ILogger, RevenantFileLogger>();
                    services.AddLogging();
                });

            //if (Config.FileLoggingEnabled && (null != Logger))
            //{
            //    hostBuilder.ConfigureServices((context, services) => {
            //        services.AddSingleton<ILogger>(Logger);
            //    });
            //}

            return hostBuilder;
        }
        private IHostBuilder BuildAppHostOriginal()
        {
            _services ??= new ServiceCollection();
            _config ??= new Configuration();
            _loggingBuilder ??= NewLoggingBuilder(_config);

            //ILoggerFactory internalFactory = _sharedLoggerFactory ?? sharedFactory ?? NewLoggerFactory(_config);

            string basePath = Directory.GetCurrentDirectory();

            IHostBuilder hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(c => {
                    c.SetBasePath(basePath);
                })
                .ConfigureHostOptions(options => {
                    options.ShutdownTimeout = TimeSpan.FromSeconds(15);
                })
                .ConfigureLogging(builder => {
                    builder.ClearProviders();
                    builder = _loggingBuilder;
                })
                .ConfigureServices((context, services) => {
                    services = _services;
                });

            //if (Config.FileLoggingEnabled && (null != Logger))
            //{
            //    hostBuilder.ConfigureServices((context, services) => {
            //        services.AddSingleton<ILogger>(Logger);
            //    });
            //}

            return hostBuilder;
        }


        private IServiceCollection NewServiceCollection()
        {
            _services ??= new ServiceCollection();
            _services.AddSingleton<IRevenantConfiguration, Configuration>();

            _config ??= RevenantConfig ??= new Configuration();
            //_loggingBuilder = NewLoggingBuilder(_config);
            //_sharedLoggerFactory = NewLoggerFactory(_loggingBuilder);

            //_sharedLoggerFactory = Utilities.NewLoggerFactory(_config);
            //_services.AddSingleton<ILoggerFactory>(_sharedLoggerFactory);
            //_services.AddSingleton<IFileLogProvider, FileLogProvider>();
            //_services.AddSingleton<ILoggerProvider, FileLogProvider>();
            //_services.AddSingleton<IRevenantFileLogger, RevenantFileLogger>();
            //_services.AddSingleton<ILogger, RevenantFileLogger>();

            //_services.BuildServiceProvider();

            return _services;
        }






        //internal IHostBuilder GenericHostBuilder
        //{
        //    get {
        //        _sharedLoggerFactory ??= Utilities.NewLoggerFactory();
        //        Bootstrap.StaticLoggerFactory = _sharedLoggerFactory;
        //        _loggerWithType ??= Utilities.NewLogger(type: typeof(Bootstrap), factory: _sharedLoggerFactory);
        //        _genericHostBuilder ??= BuildAppHost(_sharedLoggerFactory);
        //        return _genericHostBuilder;
        //    }
        //    set {
        //        _genericHostBuilder = value;
        //    }
        //}

        //internal IHost DGenericHost
        //{
        //    get {
        //        _sharedLoggerFactory ??= Utilities.NewLoggerFactory();
        //        Bootstrap.StaticLoggerFactory = _sharedLoggerFactory;
        //        _loggerWithType ??= Utilities.NewLogger(type: typeof(Bootstrap), factory: _sharedLoggerFactory);
        //        _genericHostBuilder ??= BuildAppHost(_sharedLoggerFactory);
        //        _genericHost ??= GenericHostBuilder.Build();
        //        return _genericHost;
        //    }
        //    set {
        //        _genericHost = value;
        //    }
        //}


    }
}
