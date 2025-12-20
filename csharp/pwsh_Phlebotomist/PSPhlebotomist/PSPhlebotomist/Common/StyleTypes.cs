using System.ComponentModel;

namespace PSPhlebotomist.Common
{
    public sealed class OpenBracket
    {
        public static readonly OpenBracket Value = new();

        private OpenBracket() { }

        public override string ToString() => "[bold grey][[[/]";
    }

    public sealed class CloseBracket
    {
        public static readonly CloseBracket Value = new();

        private CloseBracket() { }

        public override string ToString() => "[bold grey]]][/]";
    }

    public sealed class TimestampSeperator
    {
        public static readonly TimestampSeperator Value = new();

        private TimestampSeperator() { }

        public override string ToString() => "dim grey";
    }

    public sealed class Sep
    {
        public static readonly Sep Value = new();

        private Sep() { }

        public override string ToString() => $"T";
    }

    public sealed class SuccessMessage
    {
        public static readonly SuccessMessage Value = new();

        private SuccessMessage() { }

        public override string ToString() => "SUCCESS";
    }

    public sealed class WarnMessage
    {
        public static readonly WarnMessage Value = new();

        private WarnMessage() { }

        public override string ToString() => "WARNING";
    }

    public sealed class FailMessage
    {
        public static readonly FailMessage Value = new();

        private FailMessage() { }

        public override string ToString() => "FAILURE";
    }

    public sealed class FormatDictKey
    {
        public static readonly FormatDictKey Value = new();

        private FormatDictKey() { }

        public override string ToString() => "Key:";
    }

    public sealed class FormatDictValue
    {
        public static readonly FormatDictValue Value = new();

        private FormatDictValue() { }

        public override string ToString() => "Value:";
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
        //[JsonProperty(nameof(Timestamp), DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("dim cyan")]
        public string Timestamp
        {
            get
            {
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
        //[JsonProperty(nameof(TimestampSeperator), DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("dim grey")]
        public string TimestampSeperator
        {
            get
            {
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
        //[JsonProperty(nameof(BoolTrue), DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("palegreen3")]
        public string BoolTrue
        {
            get
            {
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
        //[JsonProperty(nameof(BoolFalse), DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("red")]
        public string BoolFalse
        {
            get
            {
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
        //[JsonProperty(nameof(LevelTrace), DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("blue")]
        public string LevelTrace
        {
            get
            {
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
        //[JsonProperty(nameof(LevelDebug), DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("purple")]
        public string LevelDebug
        {
            get
            {
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
        //[JsonProperty(nameof(LevelInformation), DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("green")]
        public string LevelInformation
        {
            get
            {
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
        //[JsonProperty(nameof(LevelWarning), DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("yellow")]
        public string LevelWarning
        {
            get
            {
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
        //[JsonProperty(nameof(LevelError), DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("red")]
        public string LevelError
        {
            get
            {
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
        //[JsonProperty(nameof(LevelCritical), DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("reverse rapidblink red")]
        public string LevelCritical
        {
            get
            {
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
