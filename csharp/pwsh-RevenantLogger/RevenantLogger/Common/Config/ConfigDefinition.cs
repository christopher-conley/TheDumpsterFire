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
        private ConfigRoot _configRoot = new();

        [JsonProperty(nameof(Config), DefaultValueHandling = DefaultValueHandling.Populate)]
        public ConfigRoot Config
        {
            get {
                _configRoot ??= new ConfigRoot();
                return _configRoot;
            }
            set => _configRoot = value;
        }
        public class ConfigRoot {
            private bool _showLogo = false;
            private bool _checkForUpdates = true;
            private LoggingRoot _logging = new();

            [Description("Whether to show the logo (once) on DI container startup in a random Figlet text font, just because it's fun. Default is false.")]
            [JsonProperty(nameof(ShowLogo), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue(false)]
            public bool ShowLogo { get => _showLogo; set => _showLogo = value; }

            [Description("Whether to check for updates to the module. Default is true.")]
            [JsonProperty(nameof(CheckForUpdates), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue(true)]
            public bool CheckForUpdates { get => _checkForUpdates; set => _checkForUpdates = value; }

            [Description("Root node for logging configuration.")]
            [JsonProperty(nameof(Logging), DefaultValueHandling = DefaultValueHandling.Populate)]
            public LoggingRoot Logging {
                get {
                    _logging ??= new LoggingRoot();
                    return _logging;
                }
                set => _logging = value;
            }
        }

        public class LoggingRoot {
            private bool _enabled = true;
            private string _logDirectory = "logs";
            private string _logFilename = "revenantlogger.log";
            private string _dateFormat = "yyyy-MM-dd";
            private string _timeFormat = "HH:mm:ss.fffK";
            private string _dateTimeSeperator = "T";
            private bool _UTC = false;
            private string _minimumLogLevel = "Information";
            private LoggingColorRoot _colors = new();

            [Description("Whether logging to a file is enabled. Default is true.")]
            [JsonProperty(nameof(Enabled), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue(true)]
            public bool Enabled { get => _enabled; set => _enabled = value; }

            [Description("The directory where log files will be stored.")]
            [JsonProperty(nameof(LogDirectory), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("logs")]
            public string LogDirectory
            {
                get {
                    if (string.IsNullOrWhiteSpace(_logDirectory))
                    {
                        _logDirectory = "logs";
                        return _logDirectory;
                    }
                    else
                    {
                        return _logDirectory;
                    }
                }
                set => _logDirectory = value;
            }

            [Description("The default filename of the log file.")]
            [JsonProperty(nameof(LogFilename), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("revenantlogger.log")]
            public string LogFilename
            {
                get {
                    if (string.IsNullOrWhiteSpace(_logFilename))
                    {
                        _logFilename = "revenantlogger.log";
                        return _logFilename;
                    }
                    else
                    {
                        return _logFilename;
                    }
                }
                set => _logFilename = value;
            }

            [Description("The format of the date portion of a timestamped log line, as defined here:" +
    "https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings")]
            [JsonProperty(nameof(DateFormat), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("yyyy-MM-dd")]
            public string DateFormat
            {
                get {
                    if (string.IsNullOrWhiteSpace(_dateFormat))
                    {
                        _dateFormat = "yyyy-MM-dd";
                        return _dateFormat;
                    }
                    else
                    {
                        return _dateFormat;
                    }
                }
                set => _dateFormat = value;
            }

            [Description("The format of the time portion of a timestamped log line, as defined here:" +
    "https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings")]
            [JsonProperty(nameof(TimeFormat), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("HH:mm:ss.fffK")]
            public string TimeFormat
            {
                get {
                    if (string.IsNullOrWhiteSpace(_timeFormat))
                    {
                        _timeFormat = "HH:mm:ss.fffK";
                        return _timeFormat;
                    }
                    else
                    {
                        return _timeFormat;
                    }
                }
                set => _timeFormat = value;
            }

            [Description("Character or string that separates the date and time portions of a DateTime in a log line.")]
            [JsonProperty(nameof(DateTimeSeperator), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("T")]
            public string DateTimeSeperator
            {
                get {
                    if (null == _dateTimeSeperator)
                    {
                        _dateTimeSeperator = "T";
                        return _dateTimeSeperator;
                    }
                    else
                    {
                        return _dateTimeSeperator;
                    }
                }
                set => _dateTimeSeperator = value;
            }

            [Description("Set to \"true\" to timestamp using UTC time.")]
            [JsonProperty(nameof(UTC), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue(false)]
            public bool UTC { get => _UTC; set => _UTC = value; }

            [Description("The minimum log level to write to the log file.")]
            [JsonProperty(nameof(MinimumLogLevel), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("Information")]
            public string MinimumLogLevel
            {
                get {
                    if (string.IsNullOrWhiteSpace(_minimumLogLevel))
                    {
                        _minimumLogLevel = "Information";
                        return _minimumLogLevel;
                    }
                    else
                    {
                        return _minimumLogLevel;
                    }
                }
                set => _minimumLogLevel = value;
            }


            [Description("Root node for logging configuration.")]
            [JsonProperty(nameof(Colors), DefaultValueHandling = DefaultValueHandling.Populate)]
            public LoggingColorRoot Colors
            {
                get {
                    _colors ??= new LoggingColorRoot();
                    return _colors;
                }
                set => _colors = value;
            }

        }

        public class LoggingColorRoot
        {
            private string _timestamp = "dim cyan";
            private string _timestampSeperator = "dim grey";
            private string _boolTrue = "palegreen3";
            private string _boolFalse = "red";
            private string _levelTrace = "blue";
            private string _levelDebug = "purple";
            private string _levelInformation = "green";
            private string _levelWarning = "yellow";
            private string _levelError = "red";
            private string _levelCritical = "reverse rapidblink red";

            [Description("The color of the timestamp in a log line.")]
            [JsonProperty(nameof(Timestamp), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("dim cyan")]
            public string Timestamp
            {
                get {
                    if (string.IsNullOrWhiteSpace(_timestamp))
                    {
                        _timestamp = "dim cyan";
                        return _timestamp;
                    }
                    else
                    {
                        return _timestamp;
                    }
                }
                set => _timestamp = value;
            }

            [Description("The color of the timestamp seperator in a log line.")]
            [JsonProperty(nameof(TimestampSeperator), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("dim grey")]
            public string TimestampSeperator
            {
                get {
                    if (string.IsNullOrWhiteSpace(_timestampSeperator))
                    {
                        _timestampSeperator = "dim grey";
                        return _timestampSeperator;
                    }
                    else
                    {
                        return _timestampSeperator;
                    }
                }
                set => _timestampSeperator = value;
            }

            [Description("The color of a \"true\" bool value in a log line.")]
            [JsonProperty(nameof(BoolTrue), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("palegreen3")]
            public string BoolTrue
            {
                get {
                    if (string.IsNullOrWhiteSpace(_boolTrue))
                    {
                        _boolTrue = "palegreen3";
                        return _boolTrue;
                    }
                    else
                    {
                        return _boolTrue;
                    }
                }
                set => _boolTrue = value;
            }

            [Description("The color of a \"false\" bool value in a log line.")]
            [JsonProperty(nameof(BoolFalse), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("red")]
            public string BoolFalse
            {
                get {
                    if (string.IsNullOrWhiteSpace(_boolFalse))
                    {
                        _boolFalse = "red";
                        return _boolFalse;
                    }
                    else
                    {
                        return _boolFalse;
                    }
                }
                set => _boolFalse = value;
            }

            [Description("The color of the \"Trace\" Log Level/Severity indicator in a log line.")]
            [JsonProperty(nameof(LevelTrace), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("blue")]
            public string LevelTrace
            {
                get {
                    if (string.IsNullOrWhiteSpace(_levelTrace))
                    {
                        _levelTrace = "blue";
                        return _levelTrace;
                    }
                    else
                    {
                        return _levelTrace;
                    }
                }
                set => _levelTrace = value;
            }

            [Description("The color of the \"Debug\" Log Level/Severity indicator in a log line.")]
            [JsonProperty(nameof(LevelDebug), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("purple")]
            public string LevelDebug
            {
                get {
                    if (string.IsNullOrWhiteSpace(_levelDebug))
                    {
                        _levelDebug = "purple";
                        return _levelDebug;
                    }
                    else
                    {
                        return _levelDebug;
                    }
                }
                set => _levelDebug = value;
            }

            [Description("The color of the \"Information\" Log Level/Severity indicator in a log line.")]
            [JsonProperty(nameof(LevelInformation), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("green")]
            public string LevelInformation
            {
                get {
                    if (string.IsNullOrWhiteSpace(_levelInformation))
                    {
                        _levelInformation = "green";
                        return _levelInformation;
                    }
                    else
                    {
                        return _levelInformation;
                    }
                }
                set => _levelInformation = value;
            }

            [Description("The color of the \"Warning\" Log Level/Severity indicator in a log line.")]
            [JsonProperty(nameof(LevelWarning), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("yellow")]
            public string LevelWarning
            {
                get {
                    if (string.IsNullOrWhiteSpace(_levelWarning))
                    {
                        _levelWarning = "yellow";
                        return _levelWarning;
                    }
                    else
                    {
                        return _levelWarning;
                    }
                }
                set => _levelWarning = value;
            }

            [Description("The color of the \"Error\" Log Level/Severity indicator in a log line.")]
            [JsonProperty(nameof(LevelError), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("red")]
            public string LevelError
            {
                get {
                    if (string.IsNullOrWhiteSpace(_levelError))
                    {
                        _levelError = "red";
                        return _levelError;
                    }
                    else
                    {
                        return _levelError;
                    }
                }
                set => _levelError = value;
            }

            [Description("The color of the \"Critical\" Log Level/Severity indicator in a log line.")]
            [JsonProperty(nameof(LevelCritical), DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue("reverse rapidblink red")]
            public string LevelCritical
            {
                get {
                    if (string.IsNullOrWhiteSpace(_levelCritical))
                    {
                        _levelCritical = "reverse rapidblink red";
                        return _levelCritical;
                    }
                    else
                    {
                        return _levelCritical;
                    }
                }
                set => _levelCritical = value;
            }

        }
    }
}
