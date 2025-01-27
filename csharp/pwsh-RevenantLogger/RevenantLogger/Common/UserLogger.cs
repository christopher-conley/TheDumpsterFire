using Microsoft.Extensions.Logging;
using RosettaTools.Pwsh.Text.RevenantLogger.Helpers;
using RosettaTools.Pwsh.Text.RevenantLogger.Common.ExtensionMethods;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Diagnostics;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Common
{

    public class UserLogger
    {
        private string _name;
        private Guid _guid;
        private DateTime _creationTime;
        private IRevenantConfiguration _config;
        private ILogger _rawLoggerObject;
        private UserCustomLogger _logger;

        public string Name { get => _name; set => _name = value; }
        public Guid Guid { get => _guid; set => _guid = value; }
        public DateTime CreationTime { get => _creationTime; set => _creationTime = value; }
        public IRevenantConfiguration Config { get => _config; set => _config = value; }
        public ILogger RawLoggerObject { get => _rawLoggerObject; set => _rawLoggerObject = value; }
        public UserCustomLogger Logger { get => _logger; set => _logger = value; }

        public UserLogger(string name,
            ILogger logger,
            Guid? guid = null,
            DateTime? creationTime = null,
            IRevenantConfiguration? config = null)
        {
            _name = name;
            _guid = guid ?? Guid.NewGuid();
            _creationTime = creationTime ?? DateTime.Now;
            _logger = new UserCustomLogger(logger);
            _config = config ?? _logger.Config;
            _rawLoggerObject = logger;
        }
        public UserLogger(string name, ILogger logger)
        {
            _name = name;
            _guid = Guid.NewGuid();
            _creationTime = DateTime.Now;
            _logger = new UserCustomLogger(logger);
            _config = _logger.Config;
            _rawLoggerObject = logger;
        }
        public UserLogger(string name, DateTime creationTime, ILogger logger)
        {
            _name = name;
            _guid = Guid.NewGuid();
            _creationTime = creationTime;
            _logger = new UserCustomLogger(logger);
            _config = _logger.Config;
            _rawLoggerObject = logger;
        }

        public UserLogger(string name, string guid, DateTime creationTime, ILogger logger)
        {
            try
            {
                _guid = Guid.Parse(guid);
            }
            catch
            {
                _guid = Guid.NewGuid();
                Debug.WriteLine($"The GUID provided ({guid}) is not a valid GUID. A new GUID was generated: {_guid}");
            }
            _name = name;
            _creationTime = creationTime;
            _logger = new UserCustomLogger(logger);
            _config = _logger.Config;
            _rawLoggerObject = logger;
        }
        public UserLogger(string name, Guid guid, DateTime creationTime, ILogger logger)
        {
            _name = name;
            _guid = guid;
            _creationTime = creationTime;
            _logger = new UserCustomLogger(logger);
            _config = _logger.Config;
            _rawLoggerObject = logger;
        }

        public UserLogger(
            string name,
            Guid guid,
            DateTime creationTime,
            IRevenantConfiguration config,
            ILogger logger,
            UserCustomLogger fullLoggerObject)
        {
            _name = name;
            _guid = guid;
            _creationTime = creationTime;
            _config = config;
            _logger = fullLoggerObject;
            _rawLoggerObject = logger;
        }

        public UserLogger(
            string name,
            string guid,
            DateTime creationTime,
            IRevenantConfiguration config,
            ILogger logger,
            UserCustomLogger fullLoggerObject)
        {
            try
            {
                _guid = Guid.Parse(guid);
            }
            catch
            {
                _guid = Guid.NewGuid();
                Debug.WriteLine($"The GUID provided ({guid}) is not a valid GUID. A new GUID was generated: {_guid}");
            }
            _name = name;
            _creationTime = creationTime;
            _config = config;
            _logger = fullLoggerObject;
            _rawLoggerObject = logger;
        }


    }
    public class UserCustomLogger : RevenantLoggerPSCmdlet, ILogger
    {
        private ILogger _logger;
        private ILogger _pseudoLogger;
        private bool _hasType = true;

        public ILogger Logger
        {
            get => _pseudoLogger;
            set => _pseudoLogger = value;
        }

        public IRevenantConfiguration Config
        {
            get => RevenantConfig;
        }

        public UserCustomLogger(ILogger logger)
        {
            _logger = logger;
            _pseudoLogger = logger;
        }

        public UserCustomLogger(ILogger<UserCustomLogger> logger, IRevenantConfiguration config)
        {
            _logger = _pseudoLogger = logger;
            _config = config;

        }

        public void Log(string message)
        {
            object[] args = [];
            Log(message: message, logLevel: RevenantConfig.LoggingConfig.MinimumLogLevel, caller: null, args: args);
        }
        public void Log(string message, string logLevel)
        {
            object[] args = [];
            Log(message: message, logLevel: logLevel, caller: null, args: args);
        }

        public void Log(string message, string logLevel, string caller)
        {
            object[] args = [];
            Log(message: message, logLevel: logLevel, caller: caller, args: args);
        }

        public void Log(string message, string logLevel, string? caller, params object?[] args)
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
                    Log(message, userLogLevel, caller: caller, args: args);
                }
            }

            else
            {
                Log(message, shortlogLevel: userShortLogLevel, useShortLogLevel: true, caller: caller, args: args);
            }
        }

        public void Log(
            string message,
            LogLevel logLevel = LogLevel.Information,
            ShortLogLevel shortlogLevel = ShortLogLevel.info,
            bool useShortLogLevel = false,
            [CallerMemberName] string? caller = null,
            params object?[] args
            )
        {

            LogLevel switchLogLevel = (useShortLogLevel) ? (LogLevel)shortlogLevel : logLevel;

            switch (switchLogLevel)
            {
                case LogLevel.Information:
                    _pseudoLogger.RLogInformation(message: message, caller: caller, args: args);
                    break;
                case LogLevel.Warning:
                    _pseudoLogger.RLogWarning(message: message, caller: caller, args: args);
                    break;
                case LogLevel.Error:
                    _pseudoLogger.RLogError(message: message, caller: caller, args: args);
                    break;
                case LogLevel.Critical:
                    _pseudoLogger.RLogCritical(message: message, caller: caller, args: args);
                    break;
                case LogLevel.Debug:
                    _pseudoLogger.RLogDebug(message: message, caller: caller, args: args);
                    break;
                case LogLevel.Trace:
                    _pseudoLogger.RLogTrace(message: message, caller: caller, args: args);
                    break;
                default:
                    _pseudoLogger.RLogInformation(message: message, caller: caller, args: args);
                    break;
            }
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return _logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _pseudoLogger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
