using Chainwax.CLI;
using Chainwax.CLI.Helpers;
using Chainwax.CLI.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Chainwax.CLI.Helpers {
    public partial class ConfigHelper : IConfigHelper {
        internal Hashtable _versionInfo;
        internal string _userAppDataPath;
        internal string _configPath;
        internal string _configDir;
        internal string _configFilename;
        internal string _runningConfigPath;
        internal Hashtable _defaultSettings = AppDefaultSettings.GetDefaultSettings();
        internal readonly ConfigDefinition _defaultConfig = AppDefaultSettings.GetDefaultConfig();
        internal readonly string _defaultConfigAsString = AppDefaultSettings.GetDefaultConfigAsString();
        internal ConfigDefinition _runningConfig;
        internal ConfigDefinition _userSettings;
        internal readonly ILogger<ConfigHelper> _logger;


        public Hashtable DefaultSettings
        {
            get {
                if (null == _defaultSettings) {
                    _defaultSettings = AppDefaultSettings.GetDefaultSettings();
                    return _defaultSettings;
                }
                else {
                    return _defaultSettings;
                }
            }
        }

        public ConfigDefinition UserSettings
        {
            get {
                if (null == _userSettings) {
                    _userSettings = ReadConfig();
                    return _userSettings;
                }
                else {
                    return _userSettings;
                }
            }
            set => _userSettings = value;
        }

        public Hashtable VersionInfo
        {
            get {
                if (null == _versionInfo) {
                    _versionInfo = Utilities.GetApplicationVersionInfo();
                    return _versionInfo;
                }
                else {
                    return _versionInfo;
                }
            }
            set => Utilities.GetApplicationVersionInfo();
        }
        public string UserAppDataPath
        {
            get {
                if (null == _userAppDataPath) {
                    _userAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    return _userAppDataPath;
                }
                else {
                    return _userAppDataPath;
                }
            }
        }
        public string ConfigPath
        {
            get => _configPath;
            set => _configPath = value;
        }
        public string ConfigDir
        {
            get {
                if (null == _configDir) {
                    _configDir = AppDefaultSettings.GetDefaultSetting("ConfigDir");
                    return _configDir;
                }
                else {
                    return _configDir;
                }
            }
        }
        public string ConfigFilename
        {
            get => _configFilename;
            set => _configFilename = value;
        }

        public ConfigDefinition DefaultConfig
        {
            get => _defaultConfig;
        }
        public string DefaultConfigAsString
        {
            get => _defaultConfigAsString;
        }

        public ConfigDefinition RunningConfig
        {
            get {
                _runningConfig ??= DefaultConfig;
                return _runningConfig;
            }
            set => _runningConfig = value;
        }

        public string RunningConfigPath
        {
            get => _runningConfigPath;
            set => _runningConfigPath = value;
        }

        public ILogger<ConfigHelper> Logger
        {
            get => _logger;
        }

        public ConfigHelper(IAppTruthTable appTruthTable, ILogger<ConfigHelper> logger) {
            _logger = logger;
            // Call Init() here eventually when it's finished being written
            VersionInfo = VersionInfo;
        }

        public ConfigDefinition ReadConfig() {
            return new ConfigDefinition();
        }

        public bool WriteConfig() {
            return true;
        }

        public dynamic GetRunningConfigValue(string setting) {
            try {
                var currentSetting = RunningConfig.Config.GetType().GetProperty(setting);
                return currentSetting.GetValue(RunningConfig.Config);
            }
            catch {
                Logger.LogCritical("{setting} is not a valid setting.", setting);
                throw;
            }
        }

        public bool SetRunningConfigValue(string setting, dynamic value) {
            try {
                RunningConfig.Config
                    .GetType()
                    .GetProperty(setting)
                    .SetValue(RunningConfig.Config, value);
                return true;
            }
            catch {
                Logger.LogCritical("{setting} is not a valid setting.", setting);
                return false;
            }
        }

        public dynamic GetRunningConfig() {
            return RunningConfig;
        }

        public dynamic GetDefaultSetting(string setting) {
            return AppDefaultSettings.GetDefaultSetting(setting);
        }
        public dynamic GetDefaultSettings() {
            return AppDefaultSettings.GetDefaultSettings();
        }
    }
}
