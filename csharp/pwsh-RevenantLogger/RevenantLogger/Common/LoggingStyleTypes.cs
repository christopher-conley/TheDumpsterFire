using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RosettaTools.Pwsh.Text.RevenantLogger;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Common {
    public sealed class SuccessMessage : RevenantLoggerBase  {
        public static readonly SuccessMessage Value = new();

        private SuccessMessage() { }

        public override string ToString() => "SUCCESS";
    }

    public sealed class WarnMessage : RevenantLoggerBase
    {
        public static readonly WarnMessage Value = new();

        private WarnMessage() { }

        public override string ToString() => "WARNING";
    }

    public sealed class FailMessage : RevenantLoggerBase
    {
        public static readonly FailMessage Value = new();

        private FailMessage() { }

        public override string ToString() => "FAILURE";
    }








    public sealed class OpenBracket : RevenantLoggerBase
    {
        public static readonly OpenBracket Value = new();

        private OpenBracket() { }

        public override string ToString() => "[bold grey][[[/]";
    }

    public sealed class CloseBracket : RevenantLoggerBase
    {
        public static readonly CloseBracket Value = new();

        private CloseBracket() { }

        public override string ToString() => "[bold grey]]][/]";
    }

}
