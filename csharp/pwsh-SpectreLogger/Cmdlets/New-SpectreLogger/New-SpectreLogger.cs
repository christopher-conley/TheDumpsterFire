using Microsoft.Extensions.Logging;
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RosettaTools.Pwsh.Text.SpectreLogger.Cmdlets {
    [Cmdlet(VerbsCommon.New, "SpectreLogger")]
    [OutputType(typeof(ILogger))]
    public class CmdNewSpectreLogger : SpectreLoggerPSCmdlet {
        private string _config;
        private LoggerObject _returnObject;

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

        public CmdNewSpectreLogger() {
        }

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing() {
            base.init();
            string methodName = MethodBase.GetCurrentMethod().Name;
            //BaseBootstrap.GenericHost = BaseBootstrap.GenericHostBuilder.Build();

            BaseBootstrap = GetExistingObject<Bootstrap>(SessionState: this.SessionState, psVariable: "__SpectreLoggerExistingBootstrap");
            BaseBootstrap ??= new Bootstrap(SessionState: this.SessionState);

            //LoggerConfig = GetExistingObject<Configuration>(SessionState: this.SessionState, psVariable: "__SpectreLoggerExistingConfig");
            //LoggerConfig ??= new Configuration();


            //BaseBootstrap = GetBootstrapper(this.SessionState);

            BaseBootstrap.Logger?.BeginScope(methodName);
            BaseBootstrap.Logger?.LogInformation("{success}: {methodName}(): Inside New-SpectreLogger BeginProcessing", SuccessMessage.Value, methodName);
            WriteVerbose("Begin!");
        }

        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord() {
            BaseBootstrap.Logger?.BeginScope("ProcessRecord");
            BaseBootstrap.Logger?.LogInformation("Inside New-SpectreLogger ProcessRecord");
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing() {
            BaseBootstrap.Logger?.BeginScope("EndProcessing");
            BaseBootstrap.Logger?.LogInformation("Inside New-SpectreLogger EndProcessing");
            //BaseBootstrap.GenericHost.RunAsync();
            //BaseBootstrap.GenericHost.StopAsync();
            //BaseBootstrap.GenericHost.WaitForShutdown();
            //BaseBootstrap.GenericHost.Dispose();

            //_returnObject = new PSObject();
            //_returnObject.Methods.Add((PSMemberInfo)BaseBootstrap.Logger.LogInformation);

            //WriteObject(BaseBootstrap.Logger);

            _returnObject = new LoggerObject(BaseBootstrap.Logger);
            WriteObject(_returnObject);
        }
    }
}
