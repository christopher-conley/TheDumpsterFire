using Microsoft.Extensions.Logging;
using RosettaTools.CLI.KeysCmd.Helpers;
using System.DirectoryServices.Protocols;

namespace RosettaTools.CLI.KeysCmd.Interfaces
{
    /// <summary>
    /// Interface for LDAP helper functions.
    /// </summary>
    public interface ILDAPHelper
    {
        /// <summary>
        /// Gets the DI instance-specific logger for the <see cref="LDAPHelper"/> class
        /// </summary>
        ILogger<LDAPHelper> LDAPLogger { get; }

        /// <summary>
        /// A method that ensures the requested user is a valid user in Active Directory.
        /// </summary>
        /// <param name="ldapConnection">The LDAP connection to use when searching</param>
        /// <returns><see cref="SearchResponse"/></returns>
        SearchResponse GetADUser(LdapConnection ldapConnection);

        /// <summary>
        /// Retrieves the SSH public keys of an Active Directory user.
        /// </summary>
        /// <param name="ldapConnection">The LDAP connection to use when searching</param>
        /// <returns><see cref="SearchResponse"/></returns>
        SearchResponse GetADUserSSHKeys(LdapConnection ldapConnection);

        /// <summary>
        /// Establishes a connection to the LDAP/AD Domain
        /// </summary>
        /// <returns><see cref="LdapConnection"/></returns>
        LdapConnection GetLDAPConnection();
    }
}