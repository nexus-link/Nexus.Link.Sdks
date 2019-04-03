﻿using System;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Authentication;
using Nexus.Link.Libraries.Web.Platform.Authentication;

namespace Nexus.Link.Authentication.Sdk
{
    /// <summary>
    /// Authentication helper for tokens used to call the Nexus services
    /// </summary>
    public class NexusAuthenticationManager : AuthenticationManager
    {
        [Obsolete("No use for serviceCredentials anymore", true)]
        public NexusAuthenticationManager(Tenant tenant, string serviceUri, AuthenticationCredentials serviceCredentials) : base(tenant, serviceUri, serviceCredentials)
        {
        }

        public NexusAuthenticationManager(Tenant tenant, string serviceBaseUrl) : base("Nexus", tenant, serviceBaseUrl, $"api/v2/Organizations/{tenant.Organization}/Environments/{tenant.Environment}/")
        {
        }

        public new static async Task<AuthenticationToken> GetJwtTokenAsync(Tenant tenant, string serviceUri, IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan, TimeSpan maximumExpirationSpan)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNull(tokenCredentials, nameof(tokenCredentials));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));
            InternalContract.RequireNotNullOrWhiteSpace(serviceUri, nameof(serviceUri));
            InternalContract.RequireNotNull(minimumExpirationSpan, nameof(minimumExpirationSpan));
            InternalContract.RequireNotNull(maximumExpirationSpan, nameof(maximumExpirationSpan));

            var authentication = new NexusAuthenticationManager(tenant, serviceUri);
            return await authentication.GetJwtTokenAsync(tokenCredentials, minimumExpirationSpan, maximumExpirationSpan);
        }

        public new static async Task<AuthenticationToken> GetJwtTokenAsync(Tenant tenant, string serviceUri, IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan)
        {
            return await GetJwtTokenAsync(tenant, serviceUri, tokenCredentials, minimumExpirationSpan, TimeSpan.FromHours(24));
        }

        public new static async Task<AuthenticationToken> GetJwtTokenAsync(Tenant tenant, string serviceUri, IAuthenticationCredentials tokenCredentials)
        {
            return await GetJwtTokenAsync(tenant, serviceUri, tokenCredentials, TimeSpan.FromHours(1), TimeSpan.FromHours(24));
        }

        public new static ITokenRefresherWithServiceClient CreateTokenRefresher(Tenant tenant, string serviceUri, IAuthenticationCredentials tokenCredentials, TimeSpan minimumExpirationSpan, TimeSpan maximumExpirationSpan)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));

            var authentication = new NexusAuthenticationManager(tenant, serviceUri);
            return authentication.CreateTokenRefresher(tokenCredentials, minimumExpirationSpan, maximumExpirationSpan);
        }

        public new static ITokenRefresherWithServiceClient CreateTokenRefresher(Tenant tenant, string serviceUri, IAuthenticationCredentials tokenCredentials)
        {
            InternalContract.RequireNotNull(tenant, nameof(tenant));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Environment, nameof(tenant.Environment));
            InternalContract.RequireNotNullOrWhiteSpace(tenant.Organization, nameof(tenant.Organization));
            InternalContract.RequireNotNullOrWhiteSpace(serviceUri, nameof(serviceUri));

            var authentication = new NexusAuthenticationManager(tenant, serviceUri);
            return authentication.CreateTokenRefresher(tokenCredentials);
        }

        public new static async Task<string> GetPublicKeyXmlAsync(Tenant tenant, string fundamentalsBaseUrl)
        {
            return await GetPublicKeyXmlAsync(tenant, fundamentalsBaseUrl, "NexusPublicKey");
        }
    }
}
