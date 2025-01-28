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
using System.Collections;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Cmdlets {

    [Cmdlet(VerbsCommunications.Write, "RevenantLog")]
    [Alias("Write-RevenantLogger", "Write-RevenantLogMessage", "Write-RevenantLoggerMessage")]
    [OutputType(typeof(void))]
    //[OutputType(typeof(CmdWriteRevenantLog))]
    public class CmdWriteRevenantLog : RevenantLoggerPSCmdlet
    {
        private string? _userLogLevel;
        private string? _userCaller;

        [Parameter(Mandatory = false)]
        [Alias("Configuration", "ConfigFile")]
#if NET8_0_OR_GREATER
        [ValidateNotNullOrWhiteSpace()]
#else
        [ValidateNotNullOrEmpty()]
#endif
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
        [ValidateTypes(typeof(string), typeof(string[]), typeof(PSObject), typeof(PSObject[]), typeof(object), typeof(object[]), typeof(Array))]

        public PSObject Message
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
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

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true)]
        [Alias("Scope", "Category")]
        [AllowNull()]
        //[ValidateTypes(typeof(string))]

        public string? Caller
        {
            get => _userCaller;
            set => _userCaller = value;
        }

        public new ILogger? CmdletLogger { get => _cmdletLogger; }

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

            InitDIContainer<CmdWriteRevenantLog>();
        }

        protected override void ProcessRecord()
        {
            CmdletLogger?.BeginScope("ProcessRecord");
            CmdletLogger?.RLogDebug("Inside Write-RevenantLogger ProcessRecord");
            Type? inputBaseType = Message.BaseObject.GetType();

            if (null == Message)
            {
                CmdletLogger?.RLogDebug("Message is null, returning");
                return;
            }

            if (inputBaseType == typeof(string))
            {
                LogMessage(Severity, Message.ToString(), Caller?.ToString());
            }
            else if (inputBaseType == typeof(object[]) || inputBaseType == typeof(System.Object[]))
            {
                LogArray(Severity, inputBaseType, Caller);
            }

            //else if (inputBaseType == typeof(Hashtable) || inputBaseType == typeof(OrderedHashtable) || inputBaseType == typeof(System.Collections.Specialized.OrderedDictionary))
            else if (inputBaseType.ToString().Contains("Hashtable") || inputBaseType.ToString().Contains("Dictionary"))
            {
                LogDictionary(Severity, inputBaseType, Caller);
            }

            else
            {
                try
                {
                    CmdletLogger?.RLogDebug($"Message type is {StringExtensions.EscapeMarkup(inputBaseType.ToString())}");
                    LogMessage(Severity, StringExtensions.EscapeMarkup(Message.ToString()), Caller?.ToString());
                }
                catch (Exception ex)
                {
                    WriteWarning($"Error processing message: {ex.Message}");
                    CmdletLogger?.RLogWarning($"Error processing message: {ex.Message}");
                }
            }
        }

        protected override void EndProcessing()
        {
            CmdletLogger?.BeginScope("EndProcessing");
            CmdletLogger?.RLogDebug("Inside Write-RevenantLogger EndProcessing");
            BuiltLoggers = ILoggersList;

            //LogMessage("Warn", "Colors are: ");
            //foreach (var prop in RevenantConfig.LoggingConfig.Colors.GetType().GetProperties())
            //{
            //    var propValue = prop.GetValue(RevenantConfig.LoggingConfig.Colors);
            //    LogMessage("Warn", $"\n\nName: {prop.Name}\nValue: {propValue}");
            //}
            //WriteObject(this);
            //WriteObject(null);
        }
        private void LogArray(string logLevel, Type inputType, string? caller = null)
        {
            GetFlattenedArray((object[])Message.BaseObject, recursiveCall: false);

            if (null != FlattenedArray)
            {
                if (inputType.ToString().Contains("Hashtable") || inputType.ToString().Contains("Dictionary"))
                {
                    LogDictionary(logLevel, inputType, caller);
                }
                else
                {
                    foreach (string? item in FlattenedArray)
                    {
                        LogMessage(logLevel, StringExtensions.EscapeMarkup(item), caller);
                    }
                }
            }
        }
        private void LogDictionary(string logLevel, Type inputType, string? caller = null)
        {
            IDictionary? inputHash = Message.BaseObject as IDictionary;

            if (null != inputHash)
            {
                LogMessage(logLevel, $"Object: {StringExtensions.EscapeMarkup(inputType.ToString())}", caller);
                foreach (DictionaryEntry? item in inputHash)
                {
                    string itemKey = StringExtensions.EscapeMarkup(item?.Key?.ToString());
                    string itemValue = StringExtensions.EscapeMarkup(item?.Value?.ToString());
                    LogMessage(logLevel, $"    {{DictKey}}   {StringExtensions.EscapeMarkup(itemKey)}", caller, args: FormatDictKey.Value);
                    LogMessage(logLevel, $"    {{DictValue}} {StringExtensions.EscapeMarkup(itemValue)}", caller, args: FormatDictValue.Value);
                }
            }
        }
        private void LogMessage(string logLevel, string message, string? caller = null, bool? dontEscape = false, params object?[] args)
        {
            string safeMessage;

            // To allow for explicit user Spectre markup and still be able to
            // correctly render and log those messages. This abstraction is
            // necessary to prevent the logger from interpreting messages
            // that may contain Spectre's markup characters as actual markup.

            if (dontEscape == true)
            {
                safeMessage = message;
            }
            else
            {
                safeMessage = StringExtensions.EscapeMarkup(message.ToString())
                .Replace("{_}", "[/]")
                .Replace("{_", "[")
                .Replace("_}", "]");
            }

            if (String.IsNullOrWhiteSpace(logLevel))
            {
                return;
            }

            if (IsValidMarkup(safeMessage) == false)
            {
                WriteVerbose($"Error processing message: {LastException.Message}; Escaping markup");
                safeMessage = StringExtensions.EscapeMarkup(message.ToString());
            }

            switch (logLevel.ToLower())
            {
                case "trace":
                case "trc":
                    CmdletLogger?.RLogTrace(message: safeMessage, caller: caller, args);
                    break;
                case "debug":
                case "dbg":
                    CmdletLogger?.RLogDebug(message: safeMessage, caller: caller, args);
                    break;
                case "information":
                case "info":
                    CmdletLogger?.RLogInformation(message: safeMessage, caller: caller, args);
                    break;
                case "warning":
                case "warn":
                    CmdletLogger?.RLogWarning(message: safeMessage, caller: caller, args);
                    break;
                case "error":
                case "err":
                    CmdletLogger?.RLogError(message: safeMessage, caller: caller, args);
                    break;
                case "critical":
                case "crit":
                    CmdletLogger?.RLogCritical(message: safeMessage, caller: caller, args);
                    break;
                case "none":
                case "off":
                case "null":
                case "discard":
                    break;
                default:
                    CmdletLogger?.RLogInformation(message: safeMessage, caller: caller, args);
                    break;
            }
        }
    }
}
