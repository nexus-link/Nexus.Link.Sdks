using System;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.Logger.Sdk.RestClients
{
    /// <summary>
    /// Implements logging by posting to a rest client.
    /// </summary>
    public class LogClient : BaseClient, ILogClient
    {
        /// <summary>
        /// </summary>
        /// <param name="baseUri">Where the log service is located.</param>
        /// <param name="authenticationCredentials">Credentials to the log service.</param>
        public LogClient(string baseUri, ServiceClientCredentials authenticationCredentials)
            : base(baseUri, authenticationCredentials)
        {
            var isWelFormedUri = Uri.IsWellFormedUriString(baseUri, UriKind.Absolute);
            if (!isWelFormedUri) throw new ArgumentException($"{nameof(baseUri)} must be a well formed uri");
            if (authenticationCredentials == null) throw new ArgumentNullException(nameof(authenticationCredentials));
        }


        /// <inheritdoc />
        public async Task LogAsync(Tenant tenant, params LogMessage[] logs)
        {
            if (logs == null) return;
            if (logs.Length == 1)
            {
                var log = logs[0];
                var relativeUrl = $"{tenant.Organization}/{tenant.Environment}/Log";
                await RestClient.PostNoResponseContentAsync(relativeUrl, log);
            }
            else
            {
                var relativeUrl = $"{tenant.Organization}/{tenant.Environment}/Logs";
                await RestClient.PostNoResponseContentAsync(relativeUrl, logs);
            }
        }
    }
}
