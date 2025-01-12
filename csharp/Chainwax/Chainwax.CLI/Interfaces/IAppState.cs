using Chainwax.CLI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Chainwax.CLI.Interfaces {
    public interface IAppState {
        Hashtable EnvironmentVariables { get; set; }
        nint HWndConsole { get; set; }
        IBareLogger LogWrapper { get; set; }
        ConfigDefinition.ConfigRoot RunningConfig { get; }
        string ShutdownReason { get; set; }
        IAppTruthTable TruthTable { get; set; }
        Hashtable VersionInfo { get; }

        public static int SW_HIDE = 0;
        public static int SW_SHOW = 5;

        void SetDefaultVars();
        void RefreshEnvironmentVariables();
    }
}
