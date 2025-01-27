namespace RosettaTools.Pwsh.Text.RevenantLogger.Interfaces
{
    public interface IRevenantConfiguration
    {
        string AppDataDir { get; }
        DateTime CreationTime { get; }
        ConfigDefinition.ConfigRoot DefaultConfig { get; }
        bool FileLoggingEnabled { get; set; }
        ConfigDefinition.ConfigRoot RunningConfig { get; set; }

        /// <summary>
        /// Gets the <see cref="ConfigDefinition.LoggingRoot"/> configuration node.
        /// </summary>
        ConfigDefinition.LoggingRoot LoggingConfig { get; }

        ConfigDefinition.LoggingColorRoot Colors { get; }

        /// <summary>
        /// Gets the path to the JSON configuration file.
        /// </summary>
        string ConfigHome { get; }

        string DefaultConfigFile { get; }

        /// <summary>
        /// Evaluates to "true" if the host operating system is Linux.
        /// </summary>
        bool IsLinux { get; }

        /// <summary>
        /// Evaluates to "true" if the host operating system is Windows.
        /// </summary>
        bool IsWindows { get; }

        /// <summary>
        /// Gets or sets the path where logfiles are stored.
        /// </summary>
        string LogPath { get; set; }

        /// <summary>
        /// A useless property that does nothing for right now.
        /// </summary>
        string OS { get; }

        void LoadConfig();
        void SaveConfig();
        void SaveConfig(ConfigDefinition.ConfigRoot _incomingConfig);
        void SaveConfig(ConfigDefinition.ConfigRoot _incomingConfig, string _savePath);
        void SaveConfig(string _savePath);

        ConfigDefinition.ConfigRoot GetDefaultConfig();
    }
}
