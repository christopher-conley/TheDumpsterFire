using Microsoft.Extensions.Logging;
using Chainwax.CLI.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Chainwax.CLI.Interfaces {
    public interface IBareLogger : ILogger {

        //public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter);

    }
}
