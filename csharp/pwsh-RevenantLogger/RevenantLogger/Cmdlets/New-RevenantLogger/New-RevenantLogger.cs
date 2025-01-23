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
        private IRevenantConfiguration? _config;
        private ILogger? _cmdletLogger;
        private ILoggerFactory? _diLoggerFactory;
        private LoggerObject _returnObject;
        private string testingString = "blah";

        [Parameter(Mandatory = false)]
        [Alias("Configuration", "ConfigFile")]
        [ValidateNotNullOrWhiteSpace()]
        [ValidateString(minLength: 2)]
        //[ValidateTypes(typeof(string), typeof(PSCustomObject))]
        [ValidateTypes(typeof(string), typeof(FileInfo))]
        public PSObject Config {
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
            string blahblah = testingString;
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
                //_cmdletLogger = _diLoggerFactory?.CreateLogger<CmdNewRevenantLogger>();
            }

            //var testLogger = CmdletDIContainer?.DSharedLoggerFactory.CreateLogger<CmdNewRevenantLogger>();

            _diLoggerFactory = SharedLoggerFactory ?? GetDIService<ILoggerFactory>(required: false);

            _cmdletLogger = GetExistingLogger<CmdNewRevenantLogger>();
            if (null == _cmdletLogger)
            {
                _cmdletLogger = _diLoggerFactory?.CreateLogger<CmdNewRevenantLogger>();
                AddToLoggersList<CmdNewRevenantLogger>(_cmdletLogger);
            }

            CmdletLogger?.BeginScope(methodName);
            CmdletLogger?.RLogInformation("Default constructor called, calling DI constructor");

            ;
            ;
            CmdletLogger?.BeginScope(methodName);
            CmdletLogger?.RLogInformation("testing blah");
            CmdletLogger?.Log(LogLevel.Information, "Raw log method");
            ;
            ;
            ;
            //BaseBootstrap.DGenericHost = BaseBootstrap.GenericHostBuilder.Build();

            BaseBootstrap = GetExistingPSVariable<Bootstrap>(SessionState: this.SessionState, psVariable: "__RevenantLoggerExistingBootstrap");
            BaseBootstrap ??= new Bootstrap(SessionState: this.SessionState);

            //LoggingConfig = GetExistingPSVariable<Configuration>(SessionState: this.SessionState, psVariable: "__RevenantLoggerExistingConfig");
            //LoggingConfig ??= new Configuration();


            //BaseBootstrap = GetBootstrapper(this.SessionState);

            CmdletLogger?.BeginScope(methodName);
            CmdletLogger?.RLogInformation("lskdjflksdjflksdjlfkjsdlf");
            CmdletLogger?.LogInformation("{success}: {methodName}(): Inside New-RevenantLogger BeginProcessing", SuccessMessage.Value, methodName);
            WriteVerbose("Begin!");
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
            //BaseBootstrap.DGenericHost.RunAsync();
            //BaseBootstrap.DGenericHost.StopAsync();
            //BaseBootstrap.DGenericHost.WaitForShutdown();
            //BaseBootstrap.DGenericHost.Dispose();

            //_returnObject = new PSObject();
            //_returnObject.Methods.Add((PSMemberInfo)BaseBootstrap.Logger.LogInformation);

            //WriteObject(BaseBootstrap.Logger);

            _returnObject = new LoggerObject(BaseBootstrap.Logger);

            BuiltLoggers = ILoggersList;

            WriteObject(this);
            //WriteObject(_returnObject);
        }
    }
}
