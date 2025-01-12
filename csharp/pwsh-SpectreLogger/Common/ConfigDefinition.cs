using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.SpectreLogger.Common {
    public class ConfigDefinition {

        [JsonProperty(nameof(Config))]
        public ConfigRoot Config = new();
        public class ConfigRoot {

            [Description("Whether to show the logo on startup. Default is false.")]
            [JsonProperty(nameof(ShowLogo))]
            public bool ShowLogo { get; set; }


            [Description("Whether to check for updates to the module. Default is true.")]
            [JsonProperty(nameof(CheckForUpdates))]
            public bool CheckForUpdates { get; set; }

            [Description("Root node for logging configuration.")]
            [JsonProperty(nameof(Logging))]
            public LoggingRoot Logging = new();
        }

        public class LoggingRoot {

            [Description("Whether logging to a file is enabled. Default is true.")]
            [JsonProperty(nameof(Enabled))]
            public bool Enabled { get; set; }

            [Description("The directory where log files will be stored.")]
            [JsonProperty(nameof(LogDirectory))]
            public string LogDirectory { get; set; }

            [JsonProperty(nameof(LogFilename))]
            public string LogFilename { get; set; }

            [Description("The format of the timestamp in the log file, as defined here:" +
                "https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings")]
            [JsonProperty(nameof(TimestampFormat))]
            public string TimestampFormat { get; set; }

            [JsonProperty(nameof(UTC))]
            public bool UTC { get; set; }

            [Description("The minimum log level to write to the log file.")]
            [JsonProperty(nameof(MinimumLogLevel))]
            public string MinimumLogLevel { get; set; }
        }
    }
}
