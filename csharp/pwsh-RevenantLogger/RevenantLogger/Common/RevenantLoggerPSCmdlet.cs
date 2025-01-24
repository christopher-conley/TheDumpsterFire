using RosettaTools.Pwsh.Text.RevenantLogger;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Common {
    public abstract partial class RevenantLoggerPSCmdlet : RevenantLoggerBase {
        private protected StringInfo StringsToParse;
        public struct StringInfo {
            public int TotalItems;
            public int TotalOriginalItems;
            public int SkippedItems;
            public string[] FlattenedArray;

            public StringInfo() {
                TotalItems = 0;
                TotalOriginalItems = 0;
                SkippedItems = 0;
                FlattenedArray = new string[] { };
            }
        }

        protected RevenantLoggerPSCmdlet() {

            // Generic IHostbuilder doesn't seem to actually respect this environment variable, but
            // putting it here just in case it does in the future.

            Environment.SetEnvironmentVariable("ASPNETCORE_SUPPRESSSTATUSMESSAGES", true.ToString(), EnvironmentVariableTarget.Process);
        }
    }
}
