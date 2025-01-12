using Chainwax.CLI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainwax.CLI.Interfaces {
    public interface IConfigHelper {
        public Hashtable DefaultSettings { get; }
        public Hashtable VersionInfo { get; }
        public string UserAppDataPath { get; }
        public string ConfigPath { get; set; }
        public string ConfigDir { get; }
        public string ConfigFilename { get; set; }
        internal ConfigDefinition DefaultConfig { get; }
        public string DefaultConfigAsString { get; }
        internal ConfigDefinition RunningConfig { get; set; }
        public string RunningConfigPath { get; set; }
        internal ConfigDefinition UserSettings { get; set; }
        public dynamic GetRunningConfigValue(string setting);
        public dynamic GetRunningConfig();
        public dynamic GetDefaultSetting(string setting);
        public dynamic GetDefaultSettings();

        public ConfigDefinition ReadConfig();

        public bool WriteConfig();
        public bool SetRunningConfigValue(string setting, dynamic value);

    }
}
