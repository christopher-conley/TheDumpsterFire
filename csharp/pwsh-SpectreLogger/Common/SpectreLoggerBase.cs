using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace RosettaTools.Pwsh.Text.SpectreLogger {
    public abstract class SpectreLoggerBase : PSCmdlet {
        private protected Bootstrap? BaseBootstrap
        {
            get; set;
        }

        private protected static Configuration LoggerConfig
        {
            get; set;
        }

        private protected SpectreLoggerBase() {
            //string modulePath = Path.GetDirectoryName(typeof(SpectreLoggerBase).Assembly.Location);
            //var loadContext = new CustomAssemblyLoadContext(modulePath);
            init();
        }

        private protected void init() {
            ;
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
