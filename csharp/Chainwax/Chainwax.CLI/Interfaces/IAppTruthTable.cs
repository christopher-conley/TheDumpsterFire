using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainwax.CLI.Interfaces {
    public interface IAppTruthTable {
        bool IsConsoleAlloced { get; set; }
        bool IsConsoleVisible { get; set; }
        bool IsFirstRun { get; set; }
        //bool ShowConsole { get; set; }

        string FallbackAppTitle { get; }
    }
}
