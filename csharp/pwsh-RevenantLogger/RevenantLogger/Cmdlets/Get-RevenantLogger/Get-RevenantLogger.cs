using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Cmdlets
{

    [Cmdlet(VerbsCommon.Get, "RevenantLogger")]
    [OutputType(typeof(Dictionary<string, ILogger?>))]
    public class CmdGetRevenantLogger : RevenantLoggerPSCmdlet
    {
        public new ILogger? CmdletLogger { get => _cmdletLogger; }

        public CmdGetRevenantLogger()
        {

        }

        protected override void BeginProcessing()
        {
            base.init();
            
            if (null == CmdletDIContainer)
            {
                WriteObject("\nNo loggers have been initialized.\n");
                return;
            }
            
            else
            {
                InitDIContainer<CmdGetRevenantLogger>();
            }
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
        }



        protected override void EndProcessing()
        {
            base.EndProcessing();

            WriteObject(ILoggersList);

        }

    }
}
