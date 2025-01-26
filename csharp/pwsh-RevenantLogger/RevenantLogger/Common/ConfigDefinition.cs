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

        [JsonProperty(nameof(Config), DefaultValueHandling = DefaultValueHandling.Populate)]
        public ConfigRoot Config = new();
        public class ConfigRoot {

            [Description("Whether to show the logo on startup. Default is false.")]
            [JsonProperty(nameof(ShowLogo), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue(false)]
            public bool ShowLogo { get; set; }


            [Description("Whether to check for updates to the module. Default is true.")]
            [JsonProperty(nameof(CheckForUpdates), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue(true)]
            public bool CheckForUpdates { get; set; }

            [Description("Root node for logging configuration.")]
            [JsonProperty(nameof(Logging), DefaultValueHandling = DefaultValueHandling.Populate)]
            public LoggingRoot Logging = new();
        }

        public class LoggingRoot {

            [Description("Whether logging to a file is enabled. Default is true.")]
            [JsonProperty(nameof(Enabled), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue(true)]
            public bool Enabled { get; set; }

            [Description("The directory where log files will be stored.")]
            [JsonProperty(nameof(LogDirectory), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("logs")]
            public string LogDirectory { get; set; }

            [Description("The default filename of the log file.")]
            [JsonProperty(nameof(LogFilename), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("revenantlogger.log")]
            public string LogFilename { get; set; }

            [Description("The format of the date portion of a timestamped log line, as defined here:" +
    "https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings")]
            [JsonProperty(nameof(DateFormat), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("yyyy-MM-dd")]
            public string DateFormat { get; set; }

            [Description("The format of the time portion of a timestamped log line, as defined here:" +
    "https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings")]
            [JsonProperty(nameof(TimeFormat), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("HH:mm:ss.fffK")]
            public string TimeFormat { get; set; }

            [Description("Character or string that separates the date and time portions of a DateTime in a log line.")]
            [JsonProperty(nameof(DateTimeSeperator), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("T")]
            public string DateTimeSeperator { get; set; }

            [Description("Set to \"true\" to timestamp using UTC time.")]
            [JsonProperty(nameof(UTC), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue(false)]
            public bool UTC { get; set; }

            [Description("The minimum log level to write to the log file.")]
            [JsonProperty(nameof(MinimumLogLevel), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("Information")]
            public string MinimumLogLevel { get; set; }


            [Description("Root node for logging configuration.")]
            [JsonProperty(nameof(Colors), DefaultValueHandling = DefaultValueHandling.Populate)]
            public LoggingColorRoot Colors = new();
        }

        public class LoggingColorRoot
        {
            [Description("The color of the timestamp in a log line.")]
            [JsonProperty(nameof(Timestamp), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("dim cyan")]
            public string Timestamp { get; set; }

            [Description("The color of the timestamp seperator in a log line.")]
            [JsonProperty(nameof(TimestampSeperator), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("dim grey")]
            public string TimestampSeperator { get; set; }

            [Description("The color of a \"true\" bool value in a log line.")]
            [JsonProperty(nameof(BoolTrue), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("palegreen3")]
            public string BoolTrue { get; set; }

            [Description("The color of a \"false\" bool value in a log line.")]
            [JsonProperty(nameof(BoolFalse), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("red")]
            public string BoolFalse { get; set; }

            [Description("The color of the \"Trace\" Log Level/Severity indicator in a log line.")]
            [JsonProperty(nameof(LevelTrace), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("blue")]
            public string LevelTrace { get; set; }

            [Description("The color of the \"Debug\" Log Level/Severity indicator in a log line.")]
            [JsonProperty(nameof(LevelDebug), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("purple")]
            public string LevelDebug { get; set; }

            [Description("The color of the \"Information\" Log Level/Severity indicator in a log line.")]
            [JsonProperty(nameof(LevelInformation), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("green")]
            public string LevelInformation { get; set; }

            [Description("The color of the \"Warning\" Log Level/Severity indicator in a log line.")]
            [JsonProperty(nameof(LevelWarning), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("yellow")]
            public string LevelWarning { get; set; }

            [Description("The color of the \"Error\" Log Level/Severity indicator in a log line.")]
            [JsonProperty(nameof(LevelError), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("red")]
            public string LevelError { get; set; }

            [Description("The color of the \"Critical\" Log Level/Severity indicator in a log line.")]
            [JsonProperty(nameof(LevelCritical), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("reverse rapidblink red")]
            public string LevelCritical { get; set; }
        }
    }
}
