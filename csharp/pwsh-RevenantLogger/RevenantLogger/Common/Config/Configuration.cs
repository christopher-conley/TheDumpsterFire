using Microsoft.Extensions.Logging;
using RosettaTools.Pwsh.Text.RevenantLogger.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Helpers
{
    public class Configuration : IRevenantConfiguration
    {

        private readonly ILogger<Configuration>? _logger;
        private static DateTime _creationTime;
        private string _appDataDir;
        private string _configHome = string.Empty;
        private string _defaultConfigFilename = "revenantlogger.config.json";
        private string _defaultConfigFile;
        private string _logPath = string.Empty;
        private ConfigDefinition.ConfigRoot _defaultConfig;
        private ConfigDefinition.ConfigRoot _runningConfig;
        private bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private bool _isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        private string _os;

        public DateTime CreationTime { get => _creationTime; }

#pragma warning disable CA1416
        public string AppDataDir
        {
            get {
                if (null == _appDataDir)
                {
                    if (_isLinux)
                    {
                        _appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
                        if (Directory.Exists(_appDataDir) == false)
                        {
#if NET8_0_OR_GREATER
                            Directory.CreateDirectory(_appDataDir,
                                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                                UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                                );
#else
                            Directory.CreateDirectory(_appDataDir);
#endif
                        }
                        return _appDataDir;
                    }

                    if (null == Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
                    {
                        if (_isWindows)
                        {
                            _appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Roaming");
                            if (Directory.Exists(_appDataDir) == false)
                            {
                                Directory.CreateDirectory(_appDataDir);
                            }
                        }
                        else
                        {
                            throw new PlatformNotSupportedException($"The current platform: \"{RuntimeInformation.OSDescription}\" is not supported.");
                        }
                    }
                    else
                    {
                        _appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    }
                }

                return _appDataDir;
            }
        }
        public ConfigDefinition.ConfigRoot DefaultConfig
        {
            get {
                _defaultConfig ??= GetDefaultConfig();
                return _defaultConfig;
            }
            private set { _defaultConfig = value; }
        }

        public ConfigDefinition.ConfigRoot RunningConfig
        {
            get {
                _runningConfig ??= GetDefaultConfig();
                return _runningConfig;
            }
            set { _defaultConfig = value; }
        }

        public ConfigDefinition.LoggingRoot LoggingConfig
        {
            get => _runningConfig.Logging;
        }

        public ConfigDefinition.LoggingColorRoot Colors
        {
            get => _runningConfig.Logging.Colors;
        }

        public string ConfigHome
        {
            get => _configHome;
            private set => _configHome = value;
        }

        public string DefaultConfigFilename
        {
            get => _defaultConfigFilename;
            private set => _defaultConfigFilename = value;
        }

        public string DefaultConfigFile
        {
            get => _defaultConfigFile;
            private set => _defaultConfigFile = value;
        }

        public bool FileLoggingEnabled
        {
            get => _runningConfig.Logging.Enabled;
            set => _runningConfig.Logging.Enabled = value;
        }
        public string LogPath
        {
            get => _logPath;
            set => _logPath = value;
        }

        public bool IsWindows
        {
            get => _isWindows;
            private set => _isWindows = value;
        }

        public bool IsLinux
        {
            get => _isLinux;
            private set => _isLinux = value;
        }

        public string OS
        {
            get => _os;
            private set => _os = value;
        }

        protected internal ILogger<Configuration>? Logger
        {
            get => _logger;
        }

        public Configuration(ILogger<Configuration> logger)
        {
            _logger = logger;
            Init();
        }
        public Configuration()
        {
            Init();
        }

        private void Init()
        {
            _defaultConfig = GetDefaultConfig();
            _runningConfig ??= GetDefaultConfig();

            _os = _isWindows ? "Windows" : _isLinux ? "Linux" : "Unknown";

            // per-directory logging settings support if dotfile of config filename exists
            if (File.Exists($".{_defaultConfigFilename}"))
            {
                _configHome = Directory.GetCurrentDirectory();
            }
            else
            {
                _configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? AppDataDir;
                _configHome = Path.Combine(
                [
                    _configHome,
                        "rosettatools",
                        "pwsh",
                        "text",
                        "revenantlogger"
                ]);
            }
            _defaultConfigFile = Path.Combine(_configHome, _defaultConfigFilename);
#pragma warning disable CA1416 // Validate platform compatibility

            // Yeah, this looks dumb, but it's a whole hell of a lot easier to read
            // in this context than just slapping a negation operator at the front
            if (Directory.Exists(_configHome) == false)
            {
                if (_isLinux)
                {
#if NET8_0_OR_GREATER
                    Directory.CreateDirectory(_configHome,
                        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                        UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                        );
#else
                    Directory.CreateDirectory(_configHome);
#endif
                }
                else
                {
                    Directory.CreateDirectory(_configHome);
                }
            }
#pragma warning restore CA1416 // Validate platform compatibility
            LoadConfig();

            if (String.IsNullOrWhiteSpace(_runningConfig.Logging.DateFormat))
            {
                _runningConfig.Logging.DateFormat = "yyyy-MM-dd";
            }
            else
            {
                try
                {
                    DateTime.Now.ToString(_runningConfig.Logging.DateFormat);
                }
                catch (Exception)
                {
                    _runningConfig.Logging.DateFormat = "yyyy-MM-dd";
                }
            }

            if (String.IsNullOrWhiteSpace(_runningConfig.Logging.TimeFormat))
            {
                _runningConfig.Logging.TimeFormat = "HH:mm:ss.fffK";
            }
            else
            {
                try
                {
                    DateTime.Now.ToString(_runningConfig.Logging.TimeFormat);
                }
                catch (Exception)
                {
                    _runningConfig.Logging.TimeFormat = "HH:mm:ss.fffK";
                }
            }

            if (null == _runningConfig.Logging.DateTimeSeperator)
            {
                _runningConfig.Logging.DateTimeSeperator = "";
            }

            _logPath = Path.Combine(_configHome, _runningConfig.Logging.LogDirectory);

            if (_runningConfig.Logging.UTC)
            {
                _creationTime = DateTime.UtcNow;
            }
            else
            {
                _creationTime = DateTime.Now;
            }
        }

        public void LoadConfig()
        {
            if (File.Exists(_defaultConfigFile) == false)
            {
                SaveConfig();
            }

            try
            {
                string json = File.ReadAllText(_defaultConfigFile);
                _runningConfig = JsonConvert.DeserializeObject<ConfigDefinition.ConfigRoot>(json) ?? GetDefaultConfig();
            }
            catch (Exception ex)
            {
                Logger?.LogCritical("Failed to load configuration file from disk: {ex.Message}", ex.Message);
                throw;
            }
        }

        public void SaveConfig()
        {
            SaveConfig(_runningConfig, _defaultConfigFile);
        }

        public void SaveConfig(ConfigDefinition.ConfigRoot _incomingConfig)
        {
            SaveConfig(_incomingConfig, _defaultConfigFile);
        }

        public void SaveConfig(string _savePath)
        {
            SaveConfig(_runningConfig, _savePath);
        }

        public void SaveConfig(ConfigDefinition.ConfigRoot _incomingConfig, string _savePath)
        {
            string json = JsonConvert.SerializeObject(_incomingConfig, Formatting.Indented);
            try
            {
                File.WriteAllText(_savePath, json);
            }
            catch (Exception ex)
            {
                Logger?.LogCritical("Failed to write configuration file to disk: {ex.Message}", ex.Message);
                throw;
            }
        }

        public ConfigDefinition.ConfigRoot GetDefaultConfig()
        {

            return new ConfigDefinition.ConfigRoot {
                Logging = new ConfigDefinition.LoggingRoot {
                    Colors = new ConfigDefinition.LoggingColorRoot()
                }
            };

            //return new ConfigDefinition.ConfigRoot {
            //    ShowLogo = false,
            //    CheckForUpdates = true,
            //    Logging = new ConfigDefinition.LoggingRoot {
            //        Enabled = true,
            //        LogDirectory = "logs",
            //        LogFilename = "revenantlogger.log",
            //        DateFormat = "yyyy-MM-dd",
            //        TimeFormat = "HH:mm:ss.ffffK",
            //        DateTimeSeperator = "T",
            //        UTC = false,
            //        MinimumLogLevel = "Information",
            //        Colors = new ConfigDefinition.LoggingColorRoot {
            //            Timestamp = "dim cyan",
            //            TimestampSeperator = "dim grey"
            //        }
            //    }
            //};
        }
    }
}
