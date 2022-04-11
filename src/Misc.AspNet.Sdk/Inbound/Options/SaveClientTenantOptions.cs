using System.Text.RegularExpressions;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Misc.AspNet.Sdk.Inbound.Options
{
    /// <summary>
    /// The prefix before the "/{organization}/{environment}/" part of the path. This is used to pattern match where we would find the organization and environment.
    /// Here are some common patterns: <see cref="LegacyVersion"/>, <see cref="LegacyApiVersion"/>,
    /// <see cref="ApiVersionTenant"/>
    /// </summary>
    public class SaveClientTenantOptions : Feature, IValidatable
    {
        /// <summary>
        /// The way that many XLENT Link services has prefixed tenants in their path. Not recommended. <see cref="ApiVersionTenant"/> for the recommended prefix.
        /// </summary>
        public static Regex LegacyVersion { get; } = new Regex("/v[^/]+/([^/]+)/([^/]+)/");

        /// <summary>
        /// A slightly safer way than <see cref="LegacyVersion"/>. Not recommended. <see cref="ApiVersionTenant"/> for the recommended prefix.
        /// </summary>
        public static Regex LegacyApiVersion { get; } = new Regex("api/v[^/]+/([^/]+)/([^/]+)/");

        /// <summary>
        /// The current recommended prefix for tenant in path
        /// </summary>
        public static Regex ApiVersionTenant { get; } = new Regex("api/v[^/]+/Tenant/([^/]+)/([^/]+)/");

        /// <summary>
        /// A regular expression to find organization and environment in a URL path.
        /// Here are some common patterns: <see cref="LegacyVersion"/>, <see cref="LegacyApiVersion"/>,
        /// <see cref="ApiVersionTenant"/>
        /// </summary>

        public Regex RegexForFindingTenantInUrl { get; set; } = ApiVersionTenant;

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            if (Enabled)
            {
                FulcrumValidate.IsNotNull(RegexForFindingTenantInUrl, nameof(RegexForFindingTenantInUrl), errorLocation);
            }
        }
    }
}