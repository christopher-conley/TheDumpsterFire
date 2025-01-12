using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.SpectreLogger.Helpers {
    public class Configuration {

        protected internal static DateTime _creationTime;
        protected internal string _configHome = string.Empty;
        protected internal string _defaultConfigFilename = "spectrelogger.config.json";
        protected internal string _defaultConfigFile;
        protected internal string _logPath = string.Empty;
        protected internal ConfigDefinition.ConfigRoot _defaultConfig;
        protected internal ConfigDefinition.ConfigRoot _runningConfig;
        protected internal bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        protected internal bool _isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        protected internal string _os;

        public DateTime CreationTime
        {
            get {
                return _creationTime;
            }
        }
        public ConfigDefinition.ConfigRoot DefaultConfig
        {
            get {
                _defaultConfig ??= GetDefaultConfig();
                return _defaultConfig;
            }
            private set {
                _defaultConfig = value;
            }
        }

        public ConfigDefinition.ConfigRoot RunningConfig
        {
            get {
                _runningConfig ??= GetDefaultConfig();
                return _runningConfig;
            }
            set {
                _defaultConfig = value;
            }
        }

        protected internal string ConfigHome
        {
            get {
                return _configHome;
            }
            private set {
                _configHome = value;
            }
        }

        protected internal string DefaultConfigFilename
        {
            get {
                return _defaultConfigFilename;
            }
            private set {
                _defaultConfigFilename = value;
            }
        }

        protected internal string DefaultConfigFile
        {
            get {
                return _defaultConfigFile;
            }
            private set {
                _defaultConfigFile = value;
            }
        }

        public bool LoggingEnabled
        {
            get {
                return _runningConfig.Logging.Enabled;
            }
            set {
                _runningConfig.Logging.Enabled = value;
            }
        }
        protected internal string LogPath
        {
            get {
                return _logPath;
            }
            private set {
                _logPath = value;
            }
        }

        protected internal bool IsWindows
        {
            get {
                return _isWindows;
            }
            private set {
                _isWindows = value;
            }
        }

        protected internal bool IsLinux
        {
            get {
                return _isLinux;
            }
            private set {
                _isLinux = value;
            }
        }

        protected internal string OS
        {
            get {
                return _os;
            }
            private set {
                _os = value;
            }
        }

        public Configuration() {
            _defaultConfig = GetDefaultConfig();
            _runningConfig ??= GetDefaultConfig();

            _os = _isWindows ? "Windows" : _isLinux ? "Linux" : "Unknown";
            _configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _configHome = Path.Combine(
                new string[] {
                    _configHome,
                    "rosettatools",
                    "pwsh",
                    "text",
                    "spectrelogger"
                });
            _defaultConfigFile = Path.Combine(_configHome, _defaultConfigFilename);
#pragma warning disable CA1416 // Validate platform compatibility
            if (!Directory.Exists(_configHome)) {
                if (_isLinux) {
                    Directory.CreateDirectory(_configHome,
                        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                        UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                        );
                }
                else {
                    Directory.CreateDirectory(_configHome);
                }
            }
#pragma warning restore CA1416 // Validate platform compatibility
            LoadConfig();
            _logPath = Path.Combine(_configHome, _runningConfig.Logging.LogDirectory);
            _creationTime = DateTime.Now;
        }

        protected internal void LoadConfig() {
            if (!File.Exists(_defaultConfigFile)) {
                SaveConfig();
            }
            string json = File.ReadAllText(_defaultConfigFile);
            _runningConfig = JsonConvert.DeserializeObject<ConfigDefinition.ConfigRoot>(json);
        }

        protected internal void SaveConfig() {
            string json = JsonConvert.SerializeObject(_runningConfig, Formatting.Indented);
            File.WriteAllText(_defaultConfigFile, json);
        }

        protected internal ConfigDefinition.ConfigRoot GetDefaultConfig() {

            return new ConfigDefinition.ConfigRoot {
                ShowLogo = false,
                CheckForUpdates = true,
                Logging = new ConfigDefinition.LoggingRoot {
                    Enabled = true,
                    LogDirectory = "logs",
                    LogFilename = "spectrelogger.log",
                    TimestampFormat = "yyyy-MM-ddTHH:mm:ss.ffffK",
                    UTC = false,
                    MinimumLogLevel = "Information"
                }
            };
        }
    }
}
