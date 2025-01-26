using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Common {
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

            [Description("The format of the date portion of a timestamped log line, as defined here:" +
    "https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings")]
            [JsonProperty(nameof(DateFormat))]
            public string DateFormat { get; set; }

            [Description("The format of the time portion of a timestamped log line, as defined here:" +
    "https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings")]
            [JsonProperty(nameof(TimeFormat))]
            public string TimeFormat { get; set; }

            [Description("Character or string that separates the date and time portions of a DateTime in a log line.")]
            [JsonProperty(nameof(DateTimeSeperator))]
            public string DateTimeSeperator { get; set; }

            [Description("Set to \"true\" to timestamp using UTC time.")]
            [JsonProperty(nameof(UTC))]
            public bool UTC { get; set; }

            [Description("The minimum log level to write to the log file.")]
            [JsonProperty(nameof(MinimumLogLevel))]
            public string MinimumLogLevel { get; set; }


            [Description("Root node for logging configuration.")]
            [JsonProperty(nameof(Colors))]
            public LoggingColorRoot Colors = new();
        }

        public class LoggingColorRoot
        {
            [Description("The color of the timestamp in a log line.")]
            [JsonProperty(nameof(Timestamp))]
            public string Timestamp { get; set; }

            [Description("The color of the character/string seperating the date from the time in a timestamped log line.")]
            [JsonProperty(nameof(TimestampSeperator))]
            public string TimestampSeperator { get; set; }
        }
    }
}
