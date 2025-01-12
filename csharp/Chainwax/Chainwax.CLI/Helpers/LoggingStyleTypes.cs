using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainwax.CLI.Helpers {
    // Define the value types
    public sealed class SuccessMessage {
        public static readonly SuccessMessage Value = new SuccessMessage();

        private SuccessMessage() { }

        public override string ToString() => "SUCCESS";
    }

    public sealed class FailMessage {
        public static readonly FailMessage Value = new FailMessage();

        private FailMessage() { }

        public override string ToString() => "FAILURE";
    }
}
