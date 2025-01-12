using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.SpectreLogger.Common {
    public abstract class StaticLoggerBase {
        protected static ILogger Logger
        {
            get; private set;
        }
        public static void InitializeLogger(ILoggerFactory factory) {
            Logger = factory.CreateLogger(typeof(StaticLoggerBase));
        }
    }
}
