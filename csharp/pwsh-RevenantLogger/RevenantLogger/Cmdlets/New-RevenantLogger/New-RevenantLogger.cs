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
    [Cmdlet(VerbsCommon.New, "RevenantLogger")]
    //[OutputType(typeof(ILogger))]
    [OutputType(typeof(CmdNewRevenantLogger))]
    public class CmdNewRevenantLogger : RevenantLoggerPSCmdlet {
        private LoggerObject _returnObject;

        [Parameter(Mandatory = false)]
        [Alias("Configuration", "ConfigFile")]
#if NET8_0_OR_GREATER
        [ValidateNotNullOrWhiteSpace()]
#else
        [ValidateNotNullOrEmpty()]
#endif
        [ValidateString(minLength: 2)]
        //[ValidateTypes(typeof(string), typeof(PSCustomObject))]
        [ValidateTypes(typeof(string), typeof(FileInfo))]
        public PSObject Config {
            get;
            set;
        }

        public new ILogger? CmdletLogger { get => _cmdletLogger; }

        public Dictionary<string, ILogger?>? BuiltLoggers
        {
            get;
            private set;
        }
        public CmdNewRevenantLogger() {

        }

        public CmdNewRevenantLogger(ILogger<CmdNewRevenantLogger> logger, IRevenantConfiguration config)
        {
            _config = config;
            _cmdletLogger = logger;
            BeginProcessing();
        }

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing() {
            base.init();
            string methodName = MethodBase.GetCurrentMethod().Name;

            InitDIContainer<CmdEditRevenantLoggerConfig>();

            BaseBootstrap = GetExistingPSVariable<Bootstrap>(SessionState: this.SessionState, psVariable: "__RevenantLoggerExistingBootstrap");
            BaseBootstrap ??= new Bootstrap(SessionState: this.SessionState);

            CmdletLogger?.BeginScope(methodName);
            CmdletLogger?.LogInformation("{success}: {methodName}(): Inside New-RevenantLogger BeginProcessing", SuccessMessage.Value, methodName);
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord() {
            CmdletLogger?.BeginScope("ProcessRecord");
            CmdletLogger?.LogInformation("Inside New-RevenantLogger ProcessRecord");
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing() {
            CmdletLogger?.BeginScope("EndProcessing");
            CmdletLogger?.LogInformation("Inside New-RevenantLogger EndProcessing");

            _returnObject = new LoggerObject(BaseBootstrap.Logger);

            BuiltLoggers = ILoggersList;

            WriteObject(this);
        }
    }
}
