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
using System.Management.Automation;
using System.Text;

namespace RosettaTools.Pwsh.Text.RevenantLogger {
    public abstract class RevenantLoggerBase : PSCmdlet {

        private static Dictionary<string, ILogger?>? _iloggersList;
        internal static DIContainer? CmdletDIContainer
        {
            get; set;
        }

        internal static IServiceProvider? DIServiceProvider
        {
            get; set;
        }
        internal static IHost? GenericHost
        {
            get; set;
        }

        internal static ILoggerFactory? SharedLoggerFactory
        {
            get; set;
        }

        protected internal ILogger? CmdletLogger
        {
            get; set;
        }

        public static Dictionary<string, ILogger?>? ILoggersList
        {
            get {
                _iloggersList ??= [];
                return _iloggersList;
            }
            set => _iloggersList = value;
        }
        internal static Bootstrap? BaseBootstrap
        {
            get; set;
        }

        internal static IRevenantConfiguration? RevenantConfig
        {
            get; set;
        }

        public static Dictionary<LogLevel, string> LogLevelColors
        {
            get {
                return new Dictionary<LogLevel, string> {
                    { LogLevel.Trace, "[blue]" },
                    { LogLevel.Debug, "[purple]" },
                    { LogLevel.Information, "[green]" },
                    { LogLevel.Warning, "[yellow]" },
                    { LogLevel.Error, "[red]" },
                    { LogLevel.Critical, "[rapidblink red]" }
                };
            }
        }

        private protected RevenantLoggerBase() {
            //string modulePath = Path.GetDirectoryName(typeof(RevenantLoggerBase).Assembly.Location);
            //var loadContext = new CustomAssemblyLoadContext(modulePath);
            init();
        }

        private protected void init() {
            ;
        }

        protected internal static TService? GetDIService<TService>(bool required = false) where TService : class
        {
            if (required)
            {
                return CmdletDIContainer?.DDIServiceProvider.GetRequiredService<TService>();
            }
            else
            {
                return CmdletDIContainer?.DDIServiceProvider.GetService<TService>();
            }
            //return CmdletDIContainer.DDIServiceProvider.GetService<TService>();
        }
        protected internal static TObject? GetExistingPSVariable<TObject>(SessionState SessionState, string psVariable) where TObject : class
        {
            //var existingObject = SessionState.PSVariable.Get(psVariable);
            TObject? existingObject = (TObject?)SessionState.PSVariable.GetValue(psVariable, null);

            return (null == existingObject) ? null : existingObject;
            //if (null == existingObject) {
            //    return null;
            //}
            //return (TObject)existingObject;
        }

        protected internal static ILogger? GetExistingLogger(Type loggerType)
        {
            if (null == ILoggersList)
            {
                return null;
            }

            string loggerKey = loggerType.ToString();
            return GetExistingLogger(loggerKey);
        }

        protected internal static ILogger? GetExistingLogger<TLoggerType>() where TLoggerType : class
        {
            return GetExistingLogger(typeof(TLoggerType).Name.ToString());
        }
        protected internal static ILogger? GetExistingLogger(string loggerType)
        {
            if (null == ILoggersList)
            {
                return null;
            }

            if (ILoggersList.TryGetValue(loggerType, out ILogger? existingLogger) == false)
            {
                return null;
            }
            return existingLogger;
        }

        protected internal static void AddToLoggersList(Type loggerType, ILogger? logger)
        {
            if (null == logger)
            {
                return;
            }
            AddToLoggersList(loggerType.ToString(), logger);
        }
        protected internal static void AddToLoggersList<TLoggerType>(ILogger? logger) where TLoggerType : class
        {
            if (null == logger)
            {
                return;
            }
            AddToLoggersList(typeof(TLoggerType).Name.ToString(), logger);
        }

        protected internal static void AddToLoggersList(string loggerType, ILogger? logger)
        {
            if (null == ILoggersList)
            {
                ILoggersList = [];
            }

            if (null == logger)
            {
                return;
            }

            if (ILoggersList.TryAdd(loggerType, logger) == false)
            {
                ILoggersList[loggerType] = logger;
                return;
            }
        }

        //private protected void SetMarkupOperators() {
        //    List<string> operators = new();
        //    foreach (string code in ANSIMap.Keys) {
        //        operators.Add(code);
        //    }
        //    MarkupOperators = operators.ToArray();
        //}
    }
}
