using System;
using Nexus.Link.Capabilities.AsyncRequestMgmt.Abstract.Services;
using Nexus.Link.Contracts.Misc.AspNet.Sdk;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.EntityAttributes;

namespace Nexus.Link.Misc.AspNet.Sdk.Inbound.Options
{
    /// <summary>
    /// Options for redirecting requests to an asynchronous request-responses capability
    /// </summary>
    public class RedirectAsynchronousRequestsOptions : Libraries.Web.AspNet.Pipe.Support.Options.Feature, IValidatable
    {
        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            if (Enabled)
            {
                FulcrumValidate.IsNotNull(RequestService, nameof(RequestService), errorLocation);
            }
        }

        /// <summary>
        /// The async request service that we should redirect requests that need to be asynchronous to.
        /// </summary>
        [Validation.NotNull(TriggerPropertyName = nameof(Enabled))]
        public IRequestService RequestService { get; set; }

        /// <summary>
        /// Set this if you would like to utilize reentry style authentication.
        /// </summary>
        public IReentryAuthenticationService ReentryAuthenticationService { get; set; }

        /// <summary>
        /// Set this to the time that a hash value should be kept. Default is 365 days.
        /// </summary>
        public TimeSpan TimeSpanBeforeDelete { get; set; } = TimeSpan.FromDays(365);

    }
}