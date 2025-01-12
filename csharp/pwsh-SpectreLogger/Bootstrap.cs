using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.SpectreLogger {
    internal class Bootstrap : SpectreLoggerPSCmdlet {

        private static DateTime _creationTime;
        private static ILoggerFactory? _sharedLoggerFactory;
        private static ILogger? _logger;
        private static Configuration _loggerConfig;
        //private static IHostBuilder? _genericHostBuilder;
        //private static IHost? _genericHost;
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

        //internal IHost GenericHost
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

        public DateTime CreationTime
        {
            get => _creationTime;
        }

        public ILoggerFactory? SharedLoggerFactory
        {
            get => _sharedLoggerFactory;
            set {
                _sharedLoggerFactory = value;
            }
            //get {
            //    _sharedLoggerFactory ??= Utilities.NewLoggerFactory();
            //    Bootstrap.StaticLoggerFactory = _sharedLoggerFactory;
            //    return _sharedLoggerFactory;
            //}
            //set {
            //    _sharedLoggerFactory = value;
            //}
        }

        public ILogger? Logger
        {
            get => _logger;
            set {
                _logger = value;
            }
            //get {
            //    _sharedLoggerFactory ??= Utilities.NewLoggerFactory();
            //    Bootstrap.StaticLoggerFactory = _sharedLoggerFactory;
            //    _loggerWithType ??= Utilities.NewLogger(type: typeof(Bootstrap), factory: _sharedLoggerFactory);
            //    return _loggerWithType;
            //}
            //set {
            //    _loggerWithType = value;
            //}
        }

        private PSVariable logoShownVariable = new("__SpectreLoggerLogoShown", "shown", ScopedItemOptions.AllScope | ScopedItemOptions.Private);
        private PSVariable existingLoggerFactoryVariable = new("__SpectreLoggerExistingLoggerFactory", null, ScopedItemOptions.AllScope | ScopedItemOptions.Private);
        private PSVariable existingLoggerVariable = new("__SpectreLoggerExistingLogger", null, ScopedItemOptions.AllScope | ScopedItemOptions.Private);
        private PSVariable existingBootstrapVariable = new("__SpectreLoggerExistingBootstrap", null, ScopedItemOptions.AllScope | ScopedItemOptions.Private);
        private PSVariable existingConfigVariable = new("__SpectreLoggerExistingConfig", null, ScopedItemOptions.AllScope | ScopedItemOptions.Private);

        public Bootstrap(SessionState SessionState) {
            logoShownVariable.Visibility = SessionStateEntryVisibility.Public;
            existingLoggerFactoryVariable.Visibility = SessionStateEntryVisibility.Public;
            existingLoggerVariable.Visibility = SessionStateEntryVisibility.Public;
            existingBootstrapVariable.Visibility = SessionStateEntryVisibility.Public;
            existingConfigVariable.Visibility = SessionStateEntryVisibility.Public;
            existingLoggerFactoryVariable.Description = "This variable holds the ILoggerFactory instance for the current session.";
            existingLoggerVariable.Description = "This variable holds the ILogger instance for the current session.";
            logoShownVariable.Description = "This variable holds the state of the Spectre logo display for the current session.";
            existingBootstrapVariable.Description = "This variable holds the Bootstrap instance for the current session.";
            existingConfigVariable.Description = "This variable holds the Configuration instance for the current session.";


            LoggerConfig = GetExistingObject<Configuration>(SessionState: SessionState, psVariable: "__SpectreLoggerExistingConfig");
            LoggerConfig ??= new Configuration();
            existingConfigVariable.Value = LoggerConfig;
            SessionState.PSVariable.Set(existingConfigVariable);

            bool logoShown = true;
            try {
                var logoShownPS = SessionState.PSVariable.GetValue("__SpectreLoggerLogoShown", "not shown");
                if ((string)logoShownPS != "shown") {
                    logoShown = false;
                }
            }
            catch {
                logoShown = false;
            }

            if (!logoShown) {
                Utilities.ShowLogo();
            }
            SessionState.PSVariable.Set(logoShownVariable);

            var existingFactory = SessionState.PSVariable.GetValue("__SpectreLoggerExistingLoggerFactory", null);
            var existingLogger = SessionState.PSVariable.GetValue("__SpectreLoggerExistingLogger", null);

            if ((null == existingFactory)) {
                _sharedLoggerFactory = Utilities.NewLoggerFactory(LoggerConfig);
                existingLoggerFactoryVariable.Value = _sharedLoggerFactory;
                SessionState.PSVariable.Set(existingLoggerFactoryVariable);
            }

            if ((null == existingLogger)) {
                _logger = Utilities.NewLogger(type: typeof(Bootstrap), factory: _sharedLoggerFactory);
                existingLoggerVariable.Value = _logger;
                SessionState.PSVariable.Set(existingLoggerVariable);

                _logger?.LogDebug("{success}: Created a new logger instance", SuccessMessage.Value);
            }

            if (existingLogger != null) {
                try {
                    _logger = (ILogger)existingLogger;
                    _logger.LogDebug("{success}: Existing logger found, reusing", SuccessMessage.Value);
                }
                catch {

                    if (_sharedLoggerFactory != null) {
                        try {
                            _logger = Utilities.NewLogger(type: typeof(Bootstrap), factory: _sharedLoggerFactory);
                            existingLoggerVariable.Value = _logger;
                            SessionState.PSVariable.Set(existingLoggerVariable);

                            _logger?.LogDebug("No existing logger found, creating a new one.");
                            _logger?.LogDebug("{success}: Created a new logger instance", SuccessMessage.Value);
                        }
                        catch {
                            _sharedLoggerFactory = Utilities.NewLoggerFactory(LoggerConfig);
                            existingLoggerFactoryVariable.Value = _sharedLoggerFactory;
                            SessionState.PSVariable.Set(existingLoggerFactoryVariable);

                            _logger = Utilities.NewLogger(type: typeof(Bootstrap), factory: _sharedLoggerFactory);
                            existingLoggerVariable.Value = _logger;
                            SessionState.PSVariable.Set(existingLoggerVariable);

                            _logger?.LogDebug("{warn}: Logger factory existed, but was not valid, created a new one", WarnMessage.Value);
                        }
                    }
                    else {
                        _sharedLoggerFactory = Utilities.NewLoggerFactory(LoggerConfig);
                        existingLoggerFactoryVariable.Value = _sharedLoggerFactory;
                        SessionState.PSVariable.Set(existingLoggerFactoryVariable);

                        _logger = Utilities.NewLogger(type: typeof(Bootstrap), factory: _sharedLoggerFactory);
                        existingLoggerVariable.Value = _logger;
                        SessionState.PSVariable.Set(existingLoggerVariable);
                    }
                }
                
            }

            Logger?.LogDebug("Bootstrapping finished");

            _creationTime = DateTime.Now;
            existingBootstrapVariable.Value = this;
            SessionState.PSVariable.Set(existingBootstrapVariable);
        }

    }
}
