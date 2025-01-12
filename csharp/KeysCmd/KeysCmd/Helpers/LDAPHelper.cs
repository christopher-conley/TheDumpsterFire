using Microsoft.Extensions.Logging;
using RosettaTools.CLI.KeysCmd.Common;
using RosettaTools.CLI.KeysCmd.Common.ExtensionMethods;
using RosettaTools.CLI.KeysCmd.Interfaces;
using RosettaTools.CLI.KeysCmd.Logging.StyleTypes;
using System.DirectoryServices.Protocols;
using System.Net;
using static RosettaTools.CLI.KeysCmd.Common.ConfigDefinition.DomainInfoRoot;
using static RosettaTools.CLI.KeysCmd.Common.Utilities;

namespace RosettaTools.CLI.KeysCmd.Helpers
{

    /// <summary>
    /// A helper class for interacting with LDAP/AD servers. Contains methods for connecting to an
    /// <br>LDAP/AD Domain, verifying that the requested user object is valid, and retrieving the</br>
    /// <br>user's SSH public keys (if they exist) from the AD attribute as defined by the <c>SSHKeyAttribute</c></br>
    /// <br>configuration option (defaults to <c>altSecurityIdentities</c>).</br>
    /// <br></br>
    /// <br>Contains the following methods:</br>
    /// <list type="bullet">
    /// <item><see cref="GetLDAPConnection" /></item>
    /// <item><see cref="GetADUser(LdapConnection)" /></item>
    /// <item><see cref="GetADUserSSHKeys(LdapConnection)" /></item>
    /// </list>
    /// </summary>

    public class LDAPHelper : ILDAPHelper
    {
        /// <summary>
        /// A DI instance-specific <see cref="ILogger"/> object.
        /// </summary>
        private readonly ILogger<LDAPHelper> _logger;

        /// <summary>
        /// A class variable containing an instantiated <see cref="ConfigDefinition.ConfigRoot"/> object via
        /// <br>a DI <see cref="IKeysCmdConfiguration"/> interface.</br>
        /// </summary>
        private IKeysCmdConfiguration _config;

        /// <summary>
        /// A <see cref="NetworkCredential"/> object containing the LDAP/AD binding credentials.
        /// <br>This may or may not be populated depending on the authentication type.</br>
        /// <br>It is always used when connecting from a Linux host.</br>
        /// </summary>
        private NetworkCredential? bindCredential;

        /// <summary>
        /// A <see langword="string"/> containing the hostname and/or Domain name of the LDAP/AD Domain to connect to.<br></br>
        /// <br>Its value will vary depending on the settings in the config file, but it will always contain the value</br>
        /// <br>of one of the following:</br>
        /// <list type="bullet">
        /// <item>A shortened NETBIOS Domain name</item>
        /// <item>A fully-qualified Domain name</item>
        /// <item>A shortened hostname of a Domain Controller</item>
        /// <item>An FQDN of a Domain Controller</item>
        /// <item>An IP address of a Domain Controller (don't do this, it breaks encryption)</item>
        /// </list>
        /// </summary>
        private string? connectHostname;

        /// <summary>
        /// A <see langword="string"/> containing the fully-qualified LDAP Distinguished Name (DN) of the user
        /// <br>to bind to the LDAP/AD Domain with. This may or may not be populated depending</br>
        /// <br>on the authentication type. It is always used when connecting from a Linux host, and</br>
        /// <br>its value is read from the <c>KEYSCMD_USER</c> environment variable.</br>
        /// </summary>
        private string? bindUser;

        /// <summary>
        /// A <see langword="string"/> containing the password for the user specified in <see cref="bindUser"/>
        /// <br>This may or may not be populated depending on the authentication type.</br>
        /// <br>It is always used when connecting from a Linux host, and its value is</br>
        /// <br>read from the <c>KEYSCMD_PASSWORD</c> environment variable.</br>
        /// </summary>
        private string? bindPassword;

        /// <summary>
        /// A <see langword="string"/> containing the NETBIOS or FQDN of the Domain to bind to.
        /// <br>This may or may not be populated depending on the authentication type.</br>
        /// <br>It is always used when connecting from a Linux host, and its value is</br>
        /// <br>read from the <c>KEYSCMD_DOMAIN</c> environment variable or the config file.</br>
        /// <br></br>
        /// <br>If the Domain is specified in both the config file and an environment variable, the</br>
        /// <br>value set in the environment variable takes precedence.</br>
        /// </summary>
        private string? bindDomain;

        /// <summary>
        /// Holds the built <see cref="LdapConnection"/> object used to connect to the LDAP/AD Domain
        /// <br>and retrieve the user's ssh public keys.</br>
        /// </summary>
        private LdapConnection? ldapConnection;

        /// <summary>
        /// An instantiated <see cref="LdapDirectoryIdentifier"/> object with connection information
        /// <br>used to construct the <see cref="ldapConnection"/> object.</br>
        /// </summary>
        protected internal LdapDirectoryIdentifier? ldapID;

        /// <summary>
        /// An <see cref="AuthType"/> enum value representing the authentication type to use when binding
        /// <br>to the LDAP/AD Domain. The default is <see cref="AuthType.Negotiate"/> on Windows hosts, and </br>
        /// <br><see cref="AuthType.Basic"/> on Linux hosts. The <c>Negotiate</c> authentication method may fail on</br>
        /// <br>a Linux host, so the default authentication method for Linux is <see cref="AuthType.Basic"/>.</br>
        /// <br>In any case, regardless of the authentication type, the connection is encrypted by default</br>
        /// <br>using implicit TLS on port <see langword="389"/> and explicit TLS on port <see langword="636"/>.</br>
        /// <br></br><br><b><i>Don't turn off encryption</i></b> if you have to change the auth type.</br>
        /// <br>Will it work over plain text on port 389? Yes. Yes, it will work. Should you do it? <b><i>Fuck no.</i></b></br>
        /// <br><i>Absolutely fucking not.</i></br> Trying port <see langword="636"/> with <c>Basic</c> authentication will <i>almost certainly</i> work
        /// <br>unless your PKI infrastructure is super fucked or you're using an IP address instead of a</br>
        /// <br>shortened hostname, FQDN, or Domain name.</br>
        /// <br><br></br>Work your shit out and <b><i>Don't turn off encryption.</i></b></br>
        /// </summary>
        protected internal AuthType authType;

        /// <summary>
        /// A convenience class property for the <see cref="_config"/> class variable.
        /// </summary>
        protected internal IKeysCmdConfiguration Config { get => _config; private set => _config = value; }

        /// <summary>
        /// A convenience property pointing to the <see cref="ConnectionInfoRoot"/> node of the running config.
        /// </summary>
        protected internal ConnectionInfoRoot connectionInfo { get => _config.RunningConfig.DomainInfo.ConnectionInfo; }

        /// <summary>
        /// A convenience property pointing to the instance-specific DI <see cref="ILogger"/>
        /// </summary>
        public ILogger<LDAPHelper> LDAPLogger { get => _logger; }

        // <summary>
        // A useless default constructor, and I forgot why I put it here. I should just remove it, but
        // <br>I just documented it, so it's going to be here to stay for at least a few commits to</br>
        // <br>justify the time I just wasted writing out this documentation block for it.</br>
        // </summary>
        //public LDAPHelper()
        //{
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="LDAPHelper"/> class with injected dependencies.
        /// <br>This class is registered as a Singleton managed by the .NET generic host and <see cref="Spectre.Console.Cli"/> app</br>
        /// <br>and isn't meant to be manually instantiated.</br>
        /// </summary>
        /// <param name="logger">The DI instance-specific <see cref="ILogger{LDAPHelper}"/> object.</param>
        /// <param name="config">A DI <see cref="IKeysCmdConfiguration"/> object which populates the <see cref="_config"/> class variable.</param>
        public LDAPHelper(ILogger<LDAPHelper> logger, IKeysCmdConfiguration config)
        {
            _logger = logger;
            _config = config;
            LDAPLogger?.CLogInformation("LDAPHelper instantiated", FormatCaller());
        }

        /// <summary>
        /// A method to establish a connection to an LDAP/AD Domain using the settings in the config file, or
        /// <br>default/best effort/best guess settings if config settings are incorrect, mangled, or <c>null</c>.</br>
        /// <br><br></br>This method takes no parameters.</br>
        /// </summary>
        /// <returns><see cref="LdapConnection"/></returns>

        public LdapConnection GetLDAPConnection()
        {
            LDAPLogger?.BeginScope(FormatCaller());

            // The Domain name is the default setting unless a Domain Controller is specified
            connectHostname = (connectionInfo.DomainController != "KeysCmdDefaultValue") ? connectionInfo.DomainController : connectionInfo.Domain;
            string encryptionVerbiage = connectionInfo.EncryptConnection ? "[green][bold]with encryption[/][/]" : "[red][bold][underline][blink]without encryption[/][/][/][/]";
            bool validAuthType = Enum.TryParse(connectionInfo.AuthType, out authType);
            bindUser = Environment.GetEnvironmentVariable("KEYSCMD_USER");
            bindPassword = Environment.GetEnvironmentVariable("KEYSCMD_PASSWORD");
            bindDomain = Environment.GetEnvironmentVariable("KEYSCMD_DOMAIN");
            bindCredential = new NetworkCredential(bindUser, bindPassword);

            if (!validAuthType)
            {
                authType = AuthType.Negotiate;
            }

            if (Config.IsLinux)
            {

                if (String.IsNullOrWhiteSpace(bindUser) || string.IsNullOrWhiteSpace(bindPassword))
                {
                    LDAPLogger?.CLogCritical("{fail}: Linux environment variables KEYSCMD_USER and/or KEYSCMD_PASSWORD are not set. "
                        + "Cannot bind to LDAP server under Linux without credentials.", FormatCaller(), FailMessage.Value);
                    LDAPLogger?.CLogCritical("{fail}: KEYSCMD_USER environment variable should be a fully-qualified LDAP DN, like: "
                        + "CN=KeysCmd ServiceAccount,OU=OpenSSH,OU=Service Accounts,DC=domain,DC=example,DC=com", FormatCaller(), FailMessage.Value);
                    throw new Exception("Linux environment variables KEYSCMD_USER and/or KEYSCMD_PASSWORD are not set. Please fill these envirnoment variables with the proper values.");
                }

                if (String.IsNullOrWhiteSpace(bindDomain))
                {
                    if (connectionInfo.Domain != "KeysCmdDefaultValue" && !string.IsNullOrWhiteSpace(connectionInfo.Domain))
                    {
                        bindDomain = connectionInfo.Domain;
                    }
                    else
                    {
                        LDAPLogger?.CLogWarning("{warn}: Linux environment variable KEYSCMD_DOMAIN is not set. "
                            + "Using config-specified domain {connectionInfo.Domain} as the domain for binding.", FormatCaller(), WarnMessage.Value, connectionInfo.Domain);
                        bindDomain = connectionInfo.Domain;
                    }
                }

                if (connectionInfo.DomainController == "KeysCmdDefaultValue")
                {
                    connectHostname = connectionInfo.Domain;
                    LDAPLogger?.CLogWarning("{warn}: In almost all cases, you must use an FQDN or IP of a Domain Controller instead "
                        + "of a NETBIOS Domain name as the primary connection method under Linux. Using \"{connectInfo.Domain}\" to attempt the connection, but don't be surprised "
                        + "if it doesn't work. It [bold][i]MAY[/][/] work if you specify the full Domain name and not a NETBIOS name.", FormatCaller(), WarnMessage.Value, connectionInfo.Domain);
                }
                else
                {
                    connectHostname = connectionInfo.DomainController;
                }

                ldapID = new(
                    server: connectHostname,
                    portNumber: connectionInfo.Port,
                    fullyQualifiedDnsHostName: false,
                    connectionless: false);

                LdapConnection ldapConnectionUserAuth = new(identifier: ldapID, credential: bindCredential, authType: authType);
                LdapConnection ldapConnectionUserBasicAuth = new(identifier: ldapID, credential: bindCredential, authType: AuthType.Basic);
                LdapConnection ldapConnectionUserNegAuth = new(identifier: ldapID, credential: bindCredential, authType: AuthType.Negotiate);

                AuthType[] authTypes = { AuthType.Basic, AuthType.Negotiate, AuthType.Kerberos };
                Dictionary<AuthType, LdapConnection> ldapConnectionDict = new()
                {
                    { authType, ldapConnectionUserAuth }
                };

                foreach (AuthType auth in authTypes)
                {
                    if (auth != authType)
                    {
                        ldapConnectionDict.Add(auth, ldapConnectionUserBasicAuth);
                    }
                }

                List<Exception> connectionExceptionsList = [];
                Exception[] connectionExceptions;

                int i = 1;
                foreach (KeyValuePair<AuthType, LdapConnection> kvp in ldapConnectionDict)
                {
                    try
                    {
                        ldapConnection = kvp.Value;
                        ldapConnection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
                        ldapConnection.SessionOptions.ProtocolVersion = 3;

                        if (connectionInfo.EncryptConnection)
                        {
                            ldapConnection.SessionOptions.SecureSocketLayer = true;
                        }

                        LDAPLogger?.CLogInformation("Attempting connection to LDAP server/domain {connectHostname} on port {connectionInfo.Port} "
                            + "using {authType} authentication ({encryptionVerbiage})", FormatCaller(), connectHostname, connectionInfo.Port, kvp.Key, encryptionVerbiage);

                        ldapConnection.Bind();

                        LDAPLogger?.CLogInformation("{success}: connection to LDAP server/domain {connectHostname} on port {connectionInfo.Port} "
                            + "using {authType} authentication ({encryptionVerbiage}), retrieving user keys...", FormatCaller(), SuccessMessage.Value, connectHostname, connectionInfo.Port, kvp.Key, encryptionVerbiage);

                        return ldapConnection;
                    }
                    catch (Exception ex)
                    {
                        connectionExceptionsList.Add(ex);
                        i++;
                        if (i == ldapConnectionDict.Count)
                        {
                            connectionExceptions = connectionExceptionsList.ToArray();
                            LDAPLogger?.CLogCritical("{fail}: Could not bind to {connectHostname} on {connectionInfo.Port} using any available authentication method. "
                                + "The errors were: {ex}", FormatCaller(), FailMessage.Value, connectHostname, connectionInfo.Port);
                            foreach (Exception storedException in connectionExceptions)
                            {
                                LDAPLogger?.CLogCritical("{error}: {storedException}", FormatCaller(), ErrorMessage.Value, storedException);
                                LDAPLogger?.CLogCritical("Inner exception: {storedException.InnerException}", FormatCaller(), storedException.InnerException);
                                LDAPLogger?.CLogCritical("Stack trace: {storedException.StackTrace}", FormatCaller(), storedException.StackTrace);
                            }
                            throw;
                        }
                        else
                        {
                            LDAPLogger?.CLogError("{error}: Could not bind to {connectHostname} on {connectionInfo.Port} using {authType} authentication. "
                                + "The error was: {ex}", FormatCaller(), ErrorMessage.Value, connectHostname, connectionInfo.Port, kvp.Key, ex);
                            LDAPLogger?.CLogWarning("Trying a different auth type", FormatCaller());
                        }
                    }
                }
            }

            LdapDirectoryIdentifier id = new(connectHostname, connectionInfo.Port);

            if (bindUser != null && bindPassword != null && bindDomain != null)
            {
                ldapID = new(
                    server: connectHostname,
                    portNumber: connectionInfo.Port,
                    fullyQualifiedDnsHostName: false,
                    connectionless: false);

                ldapConnection = new(identifier: ldapID, credential: bindCredential, authType: authType);
            }

            else
            {
                ldapConnection = new(id)
                {
                    AuthType = authType,
                    AutoBind = connectionInfo.AttemptAutoBind
                };
            }

            // Required for searching on root of ldap directory https://github.com/dotnet/runtime/issues/64900
            ldapConnection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;

            // Must be version 3 for TLS/SSL. TLS/SSL is not supported in version 2.
            ldapConnection.SessionOptions.ProtocolVersion = 3;

            // Using encryption is the default setting. You shouldn't need to turn it off unless your AD Domain is
            // wildly misconfigured, ancient, or both.
            if (connectionInfo.EncryptConnection)
            {
                try
                {
                    if (connectionInfo.Port == 636)
                    {
                        ldapConnection.SessionOptions.SecureSocketLayer = true;
                    }
                    else
                    {
                        ldapConnection.SessionOptions.StartTransportLayerSecurity(null);
                    }
                }
                catch (Exception ex)
                {
                    LDAPLogger?.CLogCritical("{fail}: The EncryptConnection config option was specified, but could not make an encrypted connection to {connectHostname} "
                        + "on {connectionInfo.Port} using TLS/SSL. The error was: {ex}", FormatCaller(), FailMessage.Value, connectHostname, connectionInfo.Port, ex);
                    throw;
                }
            }

            // Intentionally not supporting user/pass authentication via config file values, because a credential should
            // not be stored in a plaintext config file. This is a security risk. Use the KEYSCMD_USER, KEYSCMD_PASSWORD, and
            // KEYSCMD_DOMAIN environment variables if you want to pass explicit credentials.

            //NetworkCredential adCredential = new(userName, password);
            //connection.Bind(adCredential);
            try
            {
                LDAPLogger?.CLogInformation("Attempting connection to LDAP server/domain {connectHostname} on port {connectionInfo.Port} "
                + "using {authType} authentication ({encryptionVerbiage})", FormatCaller(), connectHostname, connectionInfo.Port, authType, encryptionVerbiage);
                ldapConnection.Bind();
            }
            catch (Exception ex)
            {
                LDAPLogger?.CLogCritical("{fail}: Could not bind to {connectHostname} on {connectionInfo.Port}. "
                    + "The error was: {ex}", FormatCaller(), FailMessage.Value, connectHostname, connectionInfo.Port, ex);
                throw;
            }

            return ldapConnection;
        }

        /// <summary>
        /// A method to search for a user in the LDAP/AD Domain using the connection established by <see cref="GetLDAPConnection" />
        /// <br>This method does not retrieve the user's ssh public keys; its sole purpose in life is to verify that</br>
        /// <br>the user is a valid LDAP/AD user.</br>
        /// </summary>
        /// <param name="ldapConnection">An instance of an <see cref="LdapConnection"/> object, which is constructed
        /// <br>by the <see cref="GetLDAPConnection"/> method.</br></param>
        /// <returns><see cref="SearchResponse"/></returns>

        public SearchResponse GetADUser(LdapConnection ldapConnection)
        {
            LDAPLogger?.BeginScope(FormatCaller());
            SearchResponse searchResponse;
            SearchScope adSearchScope;
            bool isValidSearchScope = Enum.TryParse(connectionInfo.SearchScope, out adSearchScope);

            if (!isValidSearchScope)
            {
                adSearchScope = SearchScope.Subtree;
            }

            SearchRequest adUserSearch = new SearchRequest(connectionInfo.BaseDN, connectionInfo.SearchFilter, adSearchScope);

            try
            {
                searchResponse = (SearchResponse)ldapConnection.SendRequest(adUserSearch);
            }
            catch (Exception ex)
            {
                LDAPLogger?.CLogCritical("{fail}: Could not search for user in {connectionInfo.BaseDN} with filter {connectionInfo.SearchFilter}. "
                    + "The error was: {ex}", FormatCaller(), FailMessage.Value, connectionInfo.BaseDN, connectionInfo.SearchFilter, ex);
                throw;
            }

            return searchResponse;
        }

        /// <summary>
        /// A method to retrieve ssh public keys from LDAP/AD for a given user, using the connection established
        /// <br>by <see cref="GetLDAPConnection" />. It returns a <see cref="SearchResponse"/> object, not the keys themselves.</br>
        /// <br></br><br>The actual keys are raw byte arrays held in an attribute of the <c>SearchResponse</c> object and are</br>
        /// <br>converted, parsed, and written to stdout by the <c>Execute</c> method of the <c>GetKeysCommand</c> class.</br>
        /// </summary>
        /// <param name="ldapConnection">An instance of an <see cref="LdapConnection"/> object, which is constructed
        /// <br>by the <see cref="GetLDAPConnection"/> method.</br></param>
        /// <returns><see cref="SearchResponse"/></returns>
        public SearchResponse GetADUserSSHKeys(LdapConnection ldapConnection)
        {
            LDAPLogger?.BeginScope(FormatCaller());
            SearchResponse searchResponse;
            SearchScope adSearchScope;
            bool isValidSearchScope = Enum.TryParse(connectionInfo.SearchScope, out adSearchScope);

            if (!isValidSearchScope)
            {
                adSearchScope = SearchScope.Subtree;
            }

            SearchRequest sshKeySearch = new SearchRequest(connectionInfo.BaseDN, connectionInfo.SearchFilter, adSearchScope, connectionInfo.SSHKeyAttribute);

            try
            {
                searchResponse = (SearchResponse)ldapConnection.SendRequest(sshKeySearch);
            }
            catch (Exception ex)
            {
                LDAPLogger?.CLogCritical("{fail}: Could not search for user in {connectionInfo.BaseDN} with filter {connectionInfo.SearchFilter}. "
                    + "The error was: {ex}", FormatCaller(), FailMessage.Value, connectionInfo.BaseDN, connectionInfo.SearchFilter, ex);
                throw;
            }

            return searchResponse;
        }
    }
}
