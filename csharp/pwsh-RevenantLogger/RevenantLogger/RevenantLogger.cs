//using RosettaTools.Pwsh.Text.RevenantLogger.Common;
//using RosettaTools.Pwsh.Text.RevenantLogger.Helpers;
//using RosettaTools.Pwsh.Text.RevenantLogger.Interfaces;
//using RosettaTools.Pwsh.Text.RevenantLogger.Cmdlets;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Options;
//using Microsoft.Extensions.Configuration;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Management.Automation;
//using System.Text;
//using System.Threading.Tasks;
//using static RosettaTools.Pwsh.Text.RevenantLogger.Common.StaticStrings;

//namespace RosettaTools.Pwsh.Text.RevenantLogger
//{
//    internal class RevenantLogger
//    {
//        private static DateTime _creationTime;
//        private IServiceCollection? _services;
//        private IHostBuilder? _genericHostBuilder;
//        private ILoggerFactory? _sharedLoggerFactory;
//        private IHost? _genericHost;
//        private ILogger<RevenantLogger>? _logger;
//        internal static List<ILogger>? _loggerList;
//        internal static Dictionary<string, ILogger>? _loggerDict;
//        private DIContainer? _diContainer;
//        private LoggerObject? _loggerObject;
//        private PSVariable _existingDIContainerVariable;

//        public DateTime CreationTime { get => _creationTime; }
//        protected internal IHostBuilder? GenericHostBuilder {
//            get => _genericHostBuilder;
//            private set => _genericHostBuilder = value;
//        }
//        protected internal IHost? GenericHost {
//            get => _genericHost;
//            private set => _genericHost = value;
//        }
//        protected internal IServiceCollection? Services {
//            get => _services;
//            private set => _services = value;
//        }
//        protected internal ILoggerFactory? SharedLoggerFactory {
//            get => _sharedLoggerFactory;
//            private set => _sharedLoggerFactory = value;
//        }
//        protected internal ILogger<RevenantLogger>? Logger {
//            get => _logger;
//            private set => _logger = value;
//        }
//        protected internal DIContainer? DIContainer {
//            get => _diContainer;
//            private set => _diContainer = value;
//        }
//        public LoggerObject? LoggerObject
//        {
//            get => _loggerObject;
//            protected internal set => _loggerObject = value;
//        }

//        protected internal List<ILogger>? LoggerList
//        {
//            get => _loggerList;
//            internal set => _loggerList = value;
//        }

//        protected internal Dictionary<string, ILogger> LoggerDict
//        {
//            get => _loggerDict;
//            internal set => _loggerDict = value;
//        }
//        protected internal PSVariable ExistingDIContainerVariable
//        {
//            get {
//                _existingDIContainerVariable ??= new(_PSVariableDIContainer, null, ScopedItemOptions.AllScope | ScopedItemOptions.None);
//                return _existingDIContainerVariable;
//            }
//            private set => _existingDIContainerVariable = value;
//        }

//        public RevenantLogger()
//        {
            
//        }

//    }
//}
