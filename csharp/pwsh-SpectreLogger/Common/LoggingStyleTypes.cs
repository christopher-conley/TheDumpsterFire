using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.SpectreLogger.Common {
    public sealed class SuccessMessage {
        public static readonly SuccessMessage Value = new SuccessMessage();

        private SuccessMessage() { }

        public override string ToString() => "SUCCESS";
    }

    public sealed class WarnMessage {
        public static readonly WarnMessage Value = new WarnMessage();

        private WarnMessage() { }

        public override string ToString() => "WARNING";
    }

    public sealed class FailMessage {
        public static readonly FailMessage Value = new FailMessage();

        private FailMessage() { }

        public override string ToString() => "FAILURE";
    }
}
