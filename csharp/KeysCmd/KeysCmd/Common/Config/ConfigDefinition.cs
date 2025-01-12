using Newtonsoft.Json;
using System.ComponentModel;

namespace RosettaTools.CLI.KeysCmd.Common
{
#pragma warning disable CS8618
    /// <summary>
    /// Superclass containing the configuration definition for the application.
    /// <br>Used by <see cref="Newtonsoft.Json"/> as a strongly-typed backbone to map the JSON config file.</br>
    /// </summary>

    public class ConfigDefinition
    {
        /// <summary>
        /// Meant to be the root node of the JSON config, but currently unused. Should probably remove it.
        /// </summary>
        [JsonProperty(nameof(Config))]
        public ConfigRoot Config = new();

        /// <summary>
        /// Container class for the configuration definition.
        /// <br>Holds a <see cref="DomainInfoRoot"/> and <see cref="LoggingRoot"/> definition.</br>
        /// </summary>
        public class ConfigRoot
        {

            /// <summary>
            /// Holds an instance of <see cref="DomainInfoRoot"/>, which contains AD Domain information and associated settings.
            /// <br></br><br>See also: <seealso cref="DomainInfoRoot"/></br>
            /// </summary>

            [Description("AD domain(s) information.")]
            [JsonProperty(nameof(DomainInfo))]
            public DomainInfoRoot DomainInfo = new();


            /// <summary>
            /// Holds an instance of <see cref="LoggingRoot"/>, which contains settings related to logging.
            /// <br></br><br>See also: <seealso cref="LoggingRoot"/></br>
            /// </summary>

            [Description("Root node for logging configuration.")]
            [JsonProperty(nameof(Logging))]
            public LoggingRoot Logging = new();
        }

        /// <summary>
        /// Class containing AD Domain information and related settings.
        /// <br>Members:</br>
        /// <list type="bullet">
        /// <item><see langword="string"/> <see cref="UsernameRegex"/></item>
        /// <item><see langword="string"/>[] <see cref="MatchDomains"/></item>
        /// <item><see cref="ConnectionInfoRoot"/> <see cref="ConnectionInfo"/></item>
        /// </list>
        /// </summary>
        public class DomainInfoRoot
        {

            /// <summary>
            /// Class property holding a <see langword="string" /> representation of the regex pattern to match usernames against.
            /// <br>The regex pattern must contain 2 explicit capture groups; one named <c>Domain</c>, and the other named <c>User</c></br>
            /// <br>Default value is:</br>
            /// <br><c>"(?&lt;Domain\&gt;^[a-zA-z0-9]+)\\(?&lt;User&gt;\w+)$"</c></br>
            /// </summary>

            [Description("Regex to match usernames against. NOTE: This MUST contain explict \"Domain\" and \"User\" capture groups.")]
            [JsonProperty(nameof(UsernameRegex))]
            public string UsernameRegex { get; set; }

            /// <summary>
            /// Class property holding a <see langword="string" />[] array containing a list of AD domains considered valid for lookups.
            /// <br>If the received input domain isn't in this array, no ssh key lookup will be made, even if the regex pattern otherwise matches.</br>
            /// </summary>
            [Description("List of AD domains considered valid for lookups.")]
            [JsonProperty(nameof(MatchDomains))]
            public string[] MatchDomains { get; set; }

            /// <summary>
            /// Holds an instance of <see cref="ConnectionInfoRoot"/>, which contains LDAP connection and bind settings.
            /// <br></br><br>See also: <seealso cref="ConnectionInfoRoot"/></br>
            /// </summary>
            [Description("Node containing connection AD/LDAP server connection info")]
            [JsonProperty(nameof(ConnectionInfo))]
            public ConnectionInfoRoot ConnectionInfo = new();

            /// <summary>
            /// Container class for LDAP connection and bind settings.
            /// <br>Members:</br>
            /// <list type="bullet">
            /// <item><see langword="bool"/> <see cref="AttemptAutoBind"/></item>
            /// <item><see langword="enum"/> <see cref="AuthType"/></item>
            /// <item><see langword="bool"/> <see cref="EncryptConnection"/></item>
            /// <item><see langword="string"/> <see cref="Domain"/></item>
            /// <item><see langword="string"/> <see cref="DomainController"/></item>
            /// <item><see langword="int"/> <see cref="Port"/></item>
            /// <item><see langword="string"/> <see cref="BaseDN"/></item>
            /// <item><see langword="string"/> <see cref="SearchFilter"/></item>
            /// <item><see langword="enum"/> <see cref="SearchScope"/></item>
            /// <item><see langword="string"/> <see cref="SSHKeyAttribute"/></item>
            /// </list>
            /// </summary>
            public class ConnectionInfoRoot
            {
                /// <summary>
                /// Class property holding a <see langword="bool" /> value indicating whether to attempt to auto-bind to the domain using the
                /// <br>current user or machine's credentials.</br>
                /// </summary>
                [Description("Attempt to auto-bind to the domain usign the current user or machine's credentials. Default is true.")]
                [JsonProperty(nameof(AttemptAutoBind))]
                public bool AttemptAutoBind { get; set; }

                /// <summary>
                /// Class property holding a <see cref="System.DirectoryServices.Protocols.AuthType"/> <see langword="enum"/> value specifying the authentication method to use.
                /// </summary>
                [Description("Authentication method to use. Currently supported are Negotiate, Basic, and Anonymous. Default is Negotiate.")]
                [JsonProperty(nameof(AuthType))]
                public string AuthType { get; set; }

                /// <summary>
                /// Class property holding a <see langword="bool" /> value indicating whether to use encryption for the LDAP connection.
                /// </summary>
                [Description("Whether to encrypt network traffic when authenticating and sending AD queries. Default is true. Please don't send your creds across the network in plain text.")]
                [JsonProperty(nameof(EncryptConnection))]
                public bool EncryptConnection { get; set; }

                /// <summary>
                /// Class property holding a <see langword="string" /> value of the AD/LDAP domain to connect to.
                /// </summary>
                [Description("The AD domain to connect to.")]
                [JsonProperty(nameof(Domain))]
                [DefaultValue("KeysCmdDefaultValue")]
                public string Domain { get; set; }

                /// <summary>
                /// Class property holding a <see langword="string" /> value of a specific Domain Controller to connect to if desired, instead of attempting
                /// <br>a connection to the domain itself using the value in <see cref="Domain"/>.</br>
                /// </summary>
                [Description("Specify a Domain Controller to connect to if desired. This shouldn't be necessary in most cases with AD, the Domain alone should suffice.")]
                [JsonProperty(nameof(DomainController))]
                [DefaultValue("KeysCmdDefaultValue")]
                public string DomainController { get; set; }

                /// <summary>
                /// Class property holding an <see langword="int" /> value of the port to use when connecting to the AD Domain/Domain Controller.
                /// </summary>
                [Description("The port to connect to the AD server on.")]
                [JsonProperty(nameof(Port))]
                [DefaultValue(389)]
                public int Port { get; set; }

                /// <summary>
                /// Class property holding a <see langword="string" /> value of the Base DN that will be searched for users.
                /// </summary>
                [Description("The base DN to search for users in.")]
                [JsonProperty(nameof(BaseDN))]
                [DefaultValue("KeysCmdDefaultValue")]
                public string BaseDN { get; set; }

                /// <summary>
                /// Class property holding a <see langword="string" /> value of the LDAP search filter to use when searching for users.
                /// <br>This value MUST contain the string <c>(sAMAccountName=KeysCmdUsernameHere)</c> somewhere within the search filter.</br>
                /// </summary>
                [Description("The search filter to use when searching for users. If you modify this value, be sure to includ the string \"(sAMAccountName=KeysCmdUsernameHere)\" somewhere "
                    + "within the query. OpenSSH sends the username in DOMAIN\\Username format, so this filter is necessary to do string replacements predictably.")]
                [JsonProperty(nameof(SearchFilter))]
                [DefaultValue("(sAMAccountName=KeysCmdUsernameHere)")]
                public string SearchFilter { get; set; }

                /// <summary>
                /// Class property holding a <see cref="System.DirectoryServices.Protocols.SearchScope"/> <see langword="enum"/> value specifying the search scope
                /// <br>relative to the Base DN value held in <see cref="BaseDN"/></br>
                /// </summary>
                [Description("The search scope to use when searching for users, with the options of OneLevel, Subtree (default), and Base. Default is Subtree.")]
                [JsonProperty(nameof(SearchScope))]
                [DefaultValue("Subtree")]
                public string SearchScope { get; set; }

                /// <summary>
                /// Class property holding a <see langword="string" /> value of the name of the AD object attribute which holds the user's ssh public key information.
                /// </summary>
                [Description("The AD attribute which holds user ssh public key information (usually altSecurityIdentities).")]
                [JsonProperty(nameof(SSHKeyAttribute))]
                [DefaultValue("altSecurityIdentities")]
                public string SSHKeyAttribute { get; set; }
            }
        }

        /// <summary>
        /// Container class for logging-related settings.
        /// <br>Members:</br>
        /// <list type="bullet">
        /// <item><see cref="Enabled"/></item>
        /// <item><see cref="LogDirectory"/></item>
        /// <item><see cref="LogFilename"/></item>
        /// <item><see cref="TimestampFormat"/></item>
        /// <item><see cref="UTC"/></item>
        /// <item><see cref="MinimumLogLevel"/></item>
        /// </list>
        /// </summary>
        public class LoggingRoot
        {
            /// <summary>
            /// Class property holding a <see langword="bool" /> value indicating whether logging is enabled.
            /// </summary>
            [Description("Whether logging to a file is enabled. Default is true.")]
            [JsonProperty(nameof(Enabled))]
            public bool Enabled { get; set; }

            /// <summary>
            /// Class property holding a <see langword="string" /> value of the directory where log files will be stored.
            /// <br><b>NOTE:</b> The value of this property is relative to the "KeysCmd" binary executable file itself.</br>
            /// </summary>
            [Description("The directory where log files will be stored.")]
            [JsonProperty(nameof(LogDirectory))]
            public string LogDirectory { get; set; }

            /// <summary>
            /// Class property holding a <see langword="string" /> value of the filename to which logs will be written.
            /// <br>This file will be stored inside the directory specified in <see cref="LogDirectory"/></br>
            /// </summary>
            [JsonProperty(nameof(LogFilename))]
            public string LogFilename { get; set; }

            /// <summary>
            /// Class property holding a <see langword="string" /> value of the timestamp format to use in the log file and console.
            /// <br>The timestamp format is a standard .NET DateTime format string.</br>
            /// </summary>
            [Description("The format of the timestamp in the log file, as defined here:" +
                "https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings")]
            [JsonProperty(nameof(TimestampFormat))]
            public string TimestampFormat { get; set; }

            /// <summary>
            /// Class property holding a <see langword="bool" /> value indicating whether to use the UTC timezone for timestamps.
            /// </summary>
            [JsonProperty(nameof(UTC))]
            public bool UTC { get; set; }

            /// <summary>
            /// Class property holding a <see cref="Microsoft.Extensions.Logging.LogLevel"/> <see langword="enum"/> value specifying the minimum
            /// <br>LogLevel that will be logged to the logfile and/or console. Log messages with a LogLevel below this</br>
            /// <br>minimum level will be discarded.</br>
            /// </summary>
            [Description("The minimum log level to write to the log file.")]
            [JsonProperty(nameof(MinimumLogLevel))]
            public string MinimumLogLevel { get; set; }
        }
    }
}
