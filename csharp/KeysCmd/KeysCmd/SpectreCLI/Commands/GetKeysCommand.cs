using RosettaTools.CLI.KeysCmd.Helpers;
using RosettaTools.CLI.KeysCmd.Interfaces;
using RosettaTools.CLI.KeysCmd.Common;
using RosettaTools.CLI.KeysCmd.Common.ExtensionMethods;
using RosettaTools.CLI.KeysCmd.Logging.StyleTypes;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using static RosettaTools.CLI.KeysCmd.Common.Utilities;
using System.DirectoryServices.Protocols;

namespace RosettaTools.CLI.KeysCmd.SpectreCLI
{
    /// <summary>
    /// Command implementation for retrieving SSH public keys from Active Directory.
    /// This command handles the actual retrieval of keys and supports both interactive and OpenSSH-compatible output modes.
    /// </summary>
    public class GetKeysCommand : Command<GetKeysSettings>
    {

        private readonly ILogger<GetKeysCommand> _logger;
        private IKeysCmdConfiguration _config;
        private ConfigDefinition.DomainInfoRoot _domainInfo;
        private ILDAPHelper _ldapHelper;
        private string? samAccountName;
        private string[]? _keysArray;
        private SearchResponse? searchResponse;
        private static LdapConnection? lDAPConnection;

        /// <summary>
        /// The LDAP helper instance used for AD operations.
        /// </summary>
        protected internal ILDAPHelper LDAPHelper
        {
            get => _ldapHelper;
            private set => _ldapHelper = value;
        }

        /// <summary>
        /// Gets or sets the configuration instance.
        /// </summary>
        protected internal IKeysCmdConfiguration Config
        {
            get => _config;
            set => _config = value;
        }

        /// <summary>
        /// Convenience property that gets the <see cref="ConfigDefinition.DomainInfoRoot"/> node from configuration.
        /// </summary>
        protected internal ConfigDefinition.DomainInfoRoot DomainInfo => _domainInfo;

        /// <summary>
        /// Gets the logger instance for this command.
        /// </summary>
        public ILogger<GetKeysCommand> GetKeysLogger => _logger;

        /// <summary>
        /// Gets or sets the LDAP connection used for AD operations.
        /// <br>This will normally be set by <see cref="LDAPHelper.GetLDAPConnection"/></br>
        /// </summary>
        protected internal static LdapConnection? LDAPConnection
        {
            get => lDAPConnection;
            set => lDAPConnection = value;
        }

        /// <summary>
        /// Gets or sets the array of retrieved SSH keys.
        /// </summary>
        protected internal string[]? KeysArray
        {
            get => _keysArray;
            set => _keysArray = value;
        }

        /// <summary>
        /// Initializes a new instance of the GetKeysCommand class with required dependencies.
        /// <br>This is handled by the .NET generic host and <see cref="Spectre.Console.Cli"/></br>
        /// </summary>
        /// <param name="logger">Logger for this command instance.</param>
        /// <param name="ldapLogger">Logger for LDAP operations.</param>
        /// <param name="config">Application configuration.</param>
        /// <param name="ldapHelper">Helper for LDAP operations.</param>
        public GetKeysCommand(ILogger<GetKeysCommand> logger,
            ILogger<LDAPHelper> ldapLogger,
            IKeysCmdConfiguration config,
            ILDAPHelper ldapHelper)
        {
            _config = config;
            _domainInfo = _config.RunningConfig.DomainInfo;
            _logger = logger;
            _ldapHelper = ldapHelper;
        }

        /// <summary>
        /// Executes the command to retrieve SSH keys for a specified user.
        /// </summary>
        /// <param name="context">The command context, provided by <see cref="Spectre.Console.Cli"/></param>
        /// <param name="settings">The command settings containing user and options, provided by <see cref="Spectre.Console.Cli"/></param>
        /// <returns>0 for success, non-zero for various error conditions.</returns>
        public override int Execute([NotNull] CommandContext context, [NotNull] GetKeysSettings settings)
        {
            GetKeysLogger?.BeginScope(FormatCaller());
            string TargetUser = settings.User ?? string.Empty;
            string usernameRegex = Config.RunningConfig.DomainInfo.UsernameRegex;
            string[] validDomains = Config.RunningConfig.DomainInfo.MatchDomains;

            // I know this looks dumb, but it's a lot harder to miss the intent
            // of this vs. just using an exclamation point to negate truthiness.
            if (Regex.IsMatch(TargetUser, usernameRegex) == false)
            {

                Console.WriteLine(NULLCHAR);
                GetKeysLogger?.CLogError("{error}: Received request with invalid username format: {TargetUser}, exiting.", FormatCaller(), ErrorMessage.Value, TargetUser);
                return 2;
            }

            Match UserMatches = Regex.Match(TargetUser, usernameRegex, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            ;

#if NET5_0_OR_GREATER
            if ((UserMatches.Groups[1].Name.ToString() == "Domain") && (UserMatches.Groups[2].Name.ToString() == "User"))
            {
#else
            if (UserMatches.Groups["Domain"].Success && UserMatches.Groups["User"].Success)
            {
#endif
                GetKeysLogger?.CLogInformation("{TargetUser} matches regex pattern", FormatCaller(), TargetUser);
                string Domain = UserMatches.Groups["Domain"].Value.Replace("\\", "").Trim();
                string User = UserMatches.Groups["User"].Value.Trim();

                if (!validDomains.Contains(Domain.ToLower()))
                {
                    GetKeysLogger?.CLogError("{error}: Username {TargetUser} matched UsernameRegex pattern, but contains an invalid domain. Exiting.", FormatCaller(), ErrorMessage.Value, TargetUser);
                    Console.WriteLine(NULLCHAR);
                    return 3;
                }

                samAccountName = User;
                Config.RunningConfig.DomainInfo.ConnectionInfo.SearchFilter = Config.RunningConfig.DomainInfo.ConnectionInfo.SearchFilter.Replace("KeysCmdUsernameHere", samAccountName);
                GetKeysLogger?.CLogInformation("Request for user {User} matched to domain: {Domain}", FormatCaller(), samAccountName, Domain);
            }

            try
            {
                LDAPConnection = LDAPHelper.GetLDAPConnection();
            }
            catch (Exception ex)
            {
                GetKeysLogger?.CLogCritical("{fail}: Unable to establish LDAP connection: {ex}", FormatCaller(), FailMessage.Value, ex.Message);
                GetKeysLogger?.CLogCritical("{fail}: Inner exception: {ex.InnerException}", FormatCaller(), FailMessage.Value, ex.InnerException);
                GetKeysLogger?.CLogCritical("{fail}: Stack trace: {ex.StackTrace}", FormatCaller(), FailMessage.Value, ex.StackTrace);
                Console.WriteLine(NULLCHAR);
                return 4;
            }

            if (LDAPConnection == null)
            {
                GetKeysLogger?.CLogCritical("{fail}: LDAP connection is null, exiting.", FormatCaller(), FailMessage.Value);
                Console.WriteLine(NULLCHAR);
                return 5;
            }

            try
            {
                searchResponse = LDAPHelper.GetADUser(LDAPConnection);
            }
            catch (Exception ex)
            {
                GetKeysLogger?.CLogCritical("{fail}: Unable to retrieve SSH keys for {samAccountName}: {ex}", FormatCaller(), FailMessage.Value, samAccountName, ex.Message);
                Console.WriteLine(NULLCHAR);
                return 6;
            }

            if (searchResponse.Entries.Count == 0)
            {
                GetKeysLogger?.CLogInformation("No results found for {samAccountName}, exiting.", FormatCaller(), samAccountName);
                Console.WriteLine(NULLCHAR);
                return 0;
            }

            try
            {
                List<string> keysList = new();
                SearchResponse keysRespone = LDAPHelper.GetADUserSSHKeys(LDAPConnection);
                SearchResultEntry keyEntries = keysRespone.Entries[0];
                var sshKeys = keyEntries.Attributes.Values;
                if (sshKeys.Count == 0)
                {
                    GetKeysLogger?.CLogInformation("No SSH keys found for {samAccountName}, exiting.", FormatCaller(), samAccountName);
                    Console.WriteLine(NULLCHAR);
                    return 0;
                }

                foreach (DirectoryAttribute keys in sshKeys)
                {

                    foreach (byte[] keyByteArray in keys)
                    {
                        keysList.Add(Encoding.UTF8.GetString(keyByteArray));

                    }
                }

                KeysArray = keysList.ToArray();
            }
            catch (Exception ex)
            {
                GetKeysLogger?.CLogCritical("{fail}: Unable to retrieve SSH keys for {samAccountName}: {ex}", FormatCaller(), FailMessage.Value, samAccountName, ex.Message);
                Console.WriteLine(NULLCHAR);
                return 7;
            }

            finally
            {
                LDAPConnection.Dispose();
            }

            if (null != KeysArray && KeysArray.Length > 0)
            {
                int numkeys = KeysArray.Length;
                int i = 1;

                GetKeysLogger?.CLogInformation("{numKeys} SSH keys found for {samAccountName}", FormatCaller(), numkeys, samAccountName);
                foreach (string key in KeysArray)
                {
                    GetKeysLogger?.CLogInformation("Writing SSH key {i} of {numkeys} to stdout: {key}", FormatCaller(), i, numkeys, key);
                    Console.WriteLine(key);
                    i++;
                }

                Console.WriteLine(NULLCHAR);
            }
            else
            {
                GetKeysLogger?.CLogInformation("No SSH keys found for {samAccountName}", FormatCaller(), samAccountName);
                Console.WriteLine(NULLCHAR);
                return 0;
            }

            return 0;
        }
    }
}
