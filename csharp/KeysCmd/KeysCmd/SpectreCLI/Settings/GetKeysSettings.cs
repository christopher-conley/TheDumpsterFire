using Spectre.Console.Cli;
using System.ComponentModel;

namespace RosettaTools.CLI.KeysCmd.SpectreCLI
{

    /// <summary>
    /// Settings class for the key retrieval command, containing all configurable options for key retrieval operations.
    /// </summary>
    public class GetKeysSettings : CommandSettings
    {

        /// <summary>
        /// Gets the username for which SSH keys should be retrieved from Active Directory.
        /// Must be in "DOMAIN\Username" format.
        /// </summary>

        [Description("The user that will be retrieved from AD.")]
        [CommandOption("-u|--user")]
        public string? User { get; init; }

        /// <summary>
        /// Specifies the Active Directory attribute name that contains SSH public keys.
        /// Defaults to "altSecurityIdentities" if not specified.
        /// </summary>

        [Description("The AD attribute which holds the user's ssh public key information (usually altSecurityIdentities).")]
        [CommandOption("-k|--key-attribute")]
        public string? KeyAttribute { get; init; }

        /// <summary>
        /// Sets a value indicating that the command is being called by OpenSSH.
        /// When true, only SSH keys or null-terminated strings will be written to stdout.
        /// </summary>
        [Description("Used when called by OpenSSH. When this flag is used, only user SSH public keys and/or " +
            "empty null-terminated strings will be written to stdout, because that's what OpenSSH expects to receive.")]
        [CommandOption("-o|--openssh")]
        [DefaultValue(false)]
        public bool OpenSSH { get; init; }

    }
}
