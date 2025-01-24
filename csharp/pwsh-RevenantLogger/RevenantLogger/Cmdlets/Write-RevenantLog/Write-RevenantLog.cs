using Microsoft.Extensions.Logging;
using RosettaTools.Pwsh.Text.RevenantLogger.Common.ExtensionMethods;
using Spectre.Console;
using Spectre.Console.Advanced;
using Spectre.Console.Extensions;
using Spectre.Console.Rendering;
using Spectre.Console.Json;
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Cmdlets {

    [Cmdlet(VerbsCommunications.Write, "RevenantLog")]
    [Alias("Write-RevenantLogger", "Write-RevenantLogMessage", "Write-RevenantLoggerMessage")]
    [OutputType(typeof(void))]
    //[OutputType(typeof(CmdWriteRevenantLog))]
    public class CmdWriteRevenantLog : RevenantLoggerPSCmdlet
    {
        private IRevenantConfiguration? _config;
        private ILogger? _cmdletLogger;
        private ILoggerFactory? _diLoggerFactory;
        private string? _userLogLevel;
        private string? _userCaller;

        [Parameter(Mandatory = false)]
        [Alias("Configuration", "ConfigFile")]
        [ValidateNotNullOrWhiteSpace()]
        [ValidateString(minLength: 2)]
        [ValidateTypes(typeof(string), typeof(FileInfo))]
        public PSObject Config
        {
            get;
            private set;
        }

        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Position = 0)]
        [Alias("Messages", "MessageArray")]
        [AllowNull()]
        [ValidateTypes(typeof(string), typeof(Array))]

        public PSObject Message
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Alias("LogLevel", "Level")]
        [AllowNull()]
        [ValidateSet("Trace", "trc", "Debug", "dbg", "Information", "info", "Warning", "warn",
            "Error", "err", "Critical", "crit", "None", "off", "null", "discard", IgnoreCase = true)]
        [PSDefaultValue(Value = "Information")]

        public string Severity
        {
            get {
                _userLogLevel ??= Microsoft.Extensions.Logging.LogLevel.Information.ToString();
                return _userLogLevel;
            }
            set => _userLogLevel = value.ToString();
        }

        [Parameter(Mandatory = false, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [Alias("Scope", "Category")]
        [AllowNull()]
        //[ValidateTypes(typeof(string))]

        public string? Caller
        {
            get => _userCaller;
            set => _userCaller = value;
        }

        public ILogger? CmdletLogger
        {
            get => _cmdletLogger;
        }

        public Dictionary<string, ILogger?>? BuiltLoggers
        {
            get;
            private set;
        }

        public CmdWriteRevenantLog()
        {

        }

        protected override void BeginProcessing()
        {
            base.init();
            string methodName = MethodBase.GetCurrentMethod().Name;

            if (null == CmdletDIContainer)
            {
                if (null == this.SessionState)
                {
                    CmdletDIContainer = new DIContainer(sessionState: new SessionState());
                }
                else
                {
                    CmdletDIContainer = new DIContainer(sessionState: this.SessionState);
                }
            }

            _diLoggerFactory = SharedLoggerFactory ?? GetDIService<ILoggerFactory>(required: false);

            _cmdletLogger = GetExistingLogger<CmdWriteRevenantLog>();
            if (null == _cmdletLogger)
            {
                _cmdletLogger = _diLoggerFactory?.CreateLogger<CmdWriteRevenantLog>();
                AddToLoggersList<CmdWriteRevenantLog>(_cmdletLogger);
            }
            CmdletLogger?.BeginScope(methodName);
            CmdletLogger?.RLogDebug("Default constructor called, calling DI constructor");
        }

        protected override void ProcessRecord()
        {
            CmdletLogger?.BeginScope("ProcessRecord");
            CmdletLogger?.RLogDebug("Inside Write-RevenantLogger ProcessRecord");

            if (null == Message)
            {
                CmdletLogger?.RLogDebug("Message is null, returning");
                return;
            }

            Type? inputBaseType = Message.BaseObject.GetType();
            CmdletLogger?.RLogDebug($"Message type is {StringExtensions.EscapeMarkup(inputBaseType.ToString())}");
            
            if (inputBaseType == typeof(string))
            {
                LogMessage(Severity, StringExtensions.EscapeMarkup(Message.ToString()), Caller?.ToString());
                //CmdletLogger?.RLogInformation(StringExtensions.EscapeMarkup(Message.ToString()));
            }
            else if (inputBaseType == typeof(object[]) || inputBaseType == typeof(System.Object[]))
            {
                GetFlattenedArray((object[])Message.BaseObject, recursiveCall: false);

                if (null != FlattenedArray)
                {
                    foreach (string? item in FlattenedArray)
                    {
                        LogMessage(Severity, StringExtensions.EscapeMarkup(item), Caller?.ToString());
                        //CmdletLogger?.RLogInformation(StringExtensions.EscapeMarkup(item));
                    }
                }
            }
        }

        protected override void EndProcessing()
        {
            CmdletLogger?.BeginScope("EndProcessing");
            CmdletLogger?.RLogDebug("Inside Write-RevenantLogger EndProcessing");
            BuiltLoggers = ILoggersList;
            //WriteObject(this);
            //WriteObject(null);
        }


        private void LogMessage(string logLevel, string message, string? caller = null)
        {
            if (String.IsNullOrWhiteSpace(logLevel))
            {
                return;
            }
            switch (logLevel.ToLower())
            {
                case "trace":
                case "trc":
                    CmdletLogger?.RLogTrace(message: message, caller: caller);
                    break;
                case "debug":
                case "dbg":
                    CmdletLogger?.RLogDebug(message: message, caller: caller);
                    break;
                case "information":
                case "info":
                    CmdletLogger?.RLogInformation(message: message, caller: caller);
                    break;
                case "warning":
                case "warn":
                    CmdletLogger?.RLogWarning(message: message, caller: caller);
                    break;
                case "error":
                case "err":
                    CmdletLogger?.RLogError(message: message, caller: caller);
                    break;
                case "critical":
                case "crit":
                    CmdletLogger?.RLogCritical(message: message, caller: caller);
                    break;
                case "none":
                case "off":
                case "null":
                case "discard":
                    break;
                default:
                    CmdletLogger?.RLogInformation(message: message, caller: caller);
                    break;
            }
        }
    }
}
