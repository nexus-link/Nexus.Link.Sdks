using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Misc.AspNet.Sdk.Inbound.Options;

namespace Nexus.Link.Misc.AspNet.Sdk.Inbound
{
    /// <summary>
    /// Options that controls <see cref="NexusLinkMiddleware"/>
    /// </summary>
    public class NexusLinkMiddlewareOptions : IValidatable
    {
        /// <summary>
        /// The features that can be controlled.
        /// </summary>
        public MiddlewareFeatures Features = new MiddlewareFeatures();

        /// <summary>
        /// Set the default values as recommended by Nexus Link
        /// </summary>
        public NexusLinkMiddlewareOptions SetNexusLinkDefaults(
            LogSeverityLevel batchLogThreshold = LogSeverityLevel.None,
            ILeverServiceConfiguration serviceConfiguration = null)
        {
            // Save information from important headers
            Features.SaveReentryAuthentication.Enabled = true;
            Features.SaveExecutionId.Enabled = true;
            Features.SaveNexusTestContext.Enabled = true;
            Features.SaveClientTenant.Enabled = true;
            Features.SaveCorrelationId.Enabled = true;
            Features.SaveTenantConfiguration.Enabled = serviceConfiguration != null;
            Features.SaveTenantConfiguration.ServiceConfiguration = serviceConfiguration;

            // This is only used by workflow engine users
            Features.RedirectAsynchronousRequests.Enabled = false;

            // Log request and response
            Features.LogRequestAndResponse.Enabled = true;

            // Use batch logs to filter out chatty logs when successful
            Features.BatchLog.Enabled = batchLogThreshold != LogSeverityLevel.None;
            Features.BatchLog.FlushAsLateAsPossible = false;
            Features.BatchLog.Threshold = batchLogThreshold;

            // Convert exceptions to the proper HTTP status code and content
            Features.ConvertExceptionToHttpResponse.Enabled = true;
            return this;
        }

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsValidated(Features, propertyPath, nameof(Features), errorLocation);
        }
    }
}