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

    [Cmdlet(VerbsCommunications.Write, "RevenantLogger")]
    //[OutputType(typeof(void))]
    [OutputType(typeof(CmdWriteRevenantLog))]
    public class CmdWriteRevenantLog : RevenantLoggerPSCmdlet
    {
        private IRevenantConfiguration? _config;
        private ILogger? _cmdletLogger;
        private ILoggerFactory? _diLoggerFactory;

        [Parameter(Mandatory = false)]
        [Alias("Configuration", "ConfigFile")]
        [ValidateNotNullOrWhiteSpace()]
        [ValidateString(minLength: 2)]
        [ValidateTypes(typeof(string), typeof(FileInfo))]
        public PSObject Config
        {
            get;
            set;
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
            CmdletLogger?.RLogInformation("Default constructor called, calling DI constructor");
        }

        protected override void ProcessRecord()
        {
            CmdletLogger?.BeginScope("ProcessRecord");
            CmdletLogger?.LogInformation("Inside Write-RevenantLogger ProcessRecord");
        }

        protected override void EndProcessing()
        {
            CmdletLogger?.BeginScope("EndProcessing");
            CmdletLogger?.LogInformation("Inside Write-RevenantLogger EndProcessing");
            BuiltLoggers = ILoggersList;
            WriteObject(this);
            //WriteObject(null);
        }
    }
}
