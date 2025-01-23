using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.RevenantLogger {
    internal class Bootstrap : RevenantLoggerPSCmdlet {

        private static DateTime _creationTime;
        private static ILoggerFactory? _sharedLoggerFactory;
        private static ILogger? _logger;
        private static Configuration _loggerConfig;

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

        private PSVariable logoShownVariable = new("__RevenantLoggerLogoShown", "shown", ScopedItemOptions.AllScope | ScopedItemOptions.None);
        private PSVariable existingLoggerFactoryVariable = new("__RevenantLoggerExistingLoggerFactory", null, ScopedItemOptions.AllScope | ScopedItemOptions.None);
        private PSVariable existingLoggerVariable = new("__RevenantLoggerExistingLogger", null, ScopedItemOptions.AllScope | ScopedItemOptions.None);
        private PSVariable existingBootstrapVariable = new("__RevenantLoggerExistingBootstrap", null, ScopedItemOptions.AllScope | ScopedItemOptions.None);
        private PSVariable existingConfigVariable = new("__RevenantLoggerExistingConfig", null, ScopedItemOptions.AllScope | ScopedItemOptions.None);

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


            //RevenantConfig = GetExistingPSVariable<Configuration>(SessionState: SessionState, psVariable: "__RevenantLoggerExistingConfig");
            RevenantConfig ??= new Configuration();
            existingConfigVariable.Value = RevenantConfig;
            SessionState.PSVariable.Set(existingConfigVariable);

            bool logoShown = true;
            try {
                var logoShownPS = SessionState.PSVariable.GetValue("__RevenantLoggerLogoShown", "not shown");
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

            var existingFactory = SessionState.PSVariable.GetValue("__RevenantLoggerExistingLoggerFactory", null);
            var existingLogger = SessionState.PSVariable.GetValue("__RevenantLoggerExistingLogger", null);

            if ((null == existingFactory)) {
                _sharedLoggerFactory = Utilities.NewLoggerFactory(RevenantConfig);
                existingLoggerFactoryVariable.Value = _sharedLoggerFactory;
                SessionState.PSVariable.Set(existingLoggerFactoryVariable);
            }

            if ((null == existingLogger)) {
                _logger = Utilities.NewLogger(type: typeof(Bootstrap), factory: _sharedLoggerFactory);
                AddToLoggersList<Bootstrap>(_logger);

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
                            AddToLoggersList<Bootstrap>(_logger);

                            existingLoggerVariable.Value = _logger;
                            SessionState.PSVariable.Set(existingLoggerVariable);

                            _logger?.LogDebug("No existing logger found, creating a new one.");
                            _logger?.LogDebug("{success}: Created a new logger instance", SuccessMessage.Value);
                        }
                        catch {
                            _sharedLoggerFactory = Utilities.NewLoggerFactory(RevenantConfig);
                            existingLoggerFactoryVariable.Value = _sharedLoggerFactory;
                            SessionState.PSVariable.Set(existingLoggerFactoryVariable);

                            _logger = Utilities.NewLogger(type: typeof(Bootstrap), factory: _sharedLoggerFactory);
                            AddToLoggersList<Bootstrap>(_logger);

                            existingLoggerVariable.Value = _logger;
                            SessionState.PSVariable.Set(existingLoggerVariable);

                            _logger?.LogDebug("{warn}: Logger factory existed, but was not valid, created a new one", WarnMessage.Value);
                        }
                    }
                    else {
                        _sharedLoggerFactory = Utilities.NewLoggerFactory(RevenantConfig);
                        existingLoggerFactoryVariable.Value = _sharedLoggerFactory;
                        SessionState.PSVariable.Set(existingLoggerFactoryVariable);

                        _logger = Utilities.NewLogger(type: typeof(Bootstrap), factory: _sharedLoggerFactory);
                        AddToLoggersList<Bootstrap>(_logger);

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
