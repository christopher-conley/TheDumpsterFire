using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainwax.CLI.Helpers {
    public enum CreateFileType {
        FILE,
        DIRECTORY,
        SYMLINK,
        HARDLINK,
        JUNCTION
    }

    public enum ReadFileType {
        TEXT,
        BINARY,
        DIRECTORY
    }

    public enum WriteFileType {
        TEXT,
        BINARY,
        DIRECTORY
    }

    public enum ConsoleState {
        HIDE = 0,
        SHOW = 5,
        TOGGLE = 10
    }

    // Yeah, it's not an enum, but it's close enough and it's a good place to put it

    public static class AppLogLevel {
        public static LogLevel Trace => LogLevel.Trace;
        public static LogLevel Debug => LogLevel.Debug;
        public static LogLevel Information => LogLevel.Information;
        public static LogLevel Warning => LogLevel.Warning;
        public static LogLevel Error => LogLevel.Error;
        public static LogLevel Critical => LogLevel.Critical;
        public static LogLevel None => LogLevel.None;
    }
}
