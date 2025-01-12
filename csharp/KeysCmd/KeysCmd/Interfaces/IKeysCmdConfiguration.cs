using RosettaTools.CLI.KeysCmd.Common;

namespace RosettaTools.CLI.KeysCmd.Interfaces
{
    /// <summary>
    /// Interface for Keys Command Configuration.
    /// </summary>
    public interface IKeysCmdConfiguration
    {
        /// <summary>
        /// Gets the creation time of the underlying Configuration object.
        /// </summary>
        DateTime CreationTime { get; }

        /// <summary>
        /// Gets the default configuration.
        /// </summary>
        ConfigDefinition.ConfigRoot DefaultConfig { get; }

        /// <summary>
        /// Gets or sets a value indicating whether logging is enabled.
        /// </summary>
        bool LoggingEnabled { get; set; }

        /// <summary>
        /// Gets the running configuration.
        /// </summary>
        ConfigDefinition.ConfigRoot RunningConfig { get; }

        /// <summary>
        /// Gets the <see cref="ConfigDefinition.LoggingRoot"/> configuration node.
        /// </summary>
        ConfigDefinition.LoggingRoot LoggingConfig { get; }

        /// <summary>
        /// Gets the <see cref="ConfigDefinition.DomainInfoRoot"/> configuration node.
        /// </summary>
        ConfigDefinition.DomainInfoRoot DomainInfo { get; }

        /// <summary>
        /// Gets the path to the JSON configuration file.
        /// </summary>
        string ConfigHome { get; }

        /// <summary>
        /// Evaluates to "true" if the host operating system is Linux.
        /// </summary>
        bool IsLinux { get; }

        /// <summary>
        /// Gets the <see langword="string"/> representation of the JSON configuration file.
        /// </summary>
        string DefaultConfigFile { get; }

        /// <summary>
        /// Gets the default configuration filename.
        /// </summary>
        string DefaultConfigFilename { get; }

        /// <summary>
        /// A <see cref="string"/>[] array of valid AD domains to match against.
        /// </summary>
        string[] Domains { get; }

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

        /// <summary>
        /// Gets the configured AD attribute that contains the user's SSH key.
        /// </summary>
        string SSHKeyAttribute { get; }

        /// <summary>
        /// The regex pattern to match against when an ssh public key retrieval request is received.
        /// </summary>
        string UsernameRegex { get; }

        /// <summary>
        /// Contains the name of the user running the application, used for debugging and some configuration settings.
        /// </summary>
        string? WhoAmI { get; }

        void LoadConfig();

        void SaveConfig();

        void SaveConfig(ConfigDefinition.ConfigRoot ConfigToSave);
        void SaveConfig(string SavePath);
        void SaveConfig(ConfigDefinition.ConfigRoot ConfigToSave, string SavePath);

        ConfigDefinition.ConfigRoot GetDefaultConfig();
    }
}