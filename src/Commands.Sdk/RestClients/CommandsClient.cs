using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Rest;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Nexus.Link.Commands.Sdk.RestClients
{
    /// <summary>
    /// Implements logging by posting to a rest client.
    /// </summary>
    public class CommandsClient : BaseClient, ICommandsClient
    {
        /// <summary></summary>
        /// <param name="tenant">The tenant to get/create commands for</param>
        /// <param name="baseUri">Where the log service is located.</param>
        /// <param name="authenticationCredentials">Credentials to the log service.</param>
        public CommandsClient(Tenant tenant, string baseUri, ServiceClientCredentials authenticationCredentials)
            : base(tenant, baseUri, authenticationCredentials)
        {
            var isWelFormedUri = Uri.IsWellFormedUriString(baseUri, UriKind.Absolute);
            if (!isWelFormedUri) throw new ArgumentException($"{nameof(baseUri)} must be a well formed uri");
            if (authenticationCredentials == null) throw new ArgumentNullException(nameof(authenticationCredentials));
        }

        public async Task<IEnumerable<NexusCommand>> ReadAsync(string service, string instanceId)
        {
            var relativeUrl = $"{WebUtility.UrlEncode(service)}/{WebUtility.UrlEncode(instanceId)}";
            var commands = await RestClient.GetAsync<IEnumerable<NexusCommand>>(relativeUrl);
            return commands;
        }

        public async Task<NexusCommand> CreateAsync(string service, string command, string originator)
        {
            var relativeUrl = $"{WebUtility.UrlEncode(service)}";
            var result = await RestClient.PostAsync<NexusCommand, object>(relativeUrl, new
            {
                Service = service,
                Command = command,
                Originator = originator
            });
            return result;
        }
    }
}
