using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
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
        public NexusLinkMiddlewareOptions SetNexusLinkDefaults()
        {
            // Save information from important headers
            Features.SaveReentryAuthentication.Enabled = true;
            Features.SaveExecutionId.Enabled = true;
            Features.SaveNexusTestContext.Enabled = true;
            Features.SaveClientTenant.Enabled = true;
            Features.SaveCorrelationId.Enabled = true;
            Features.SaveTenantConfiguration.Enabled = false;

            // This is only used by workflow engine users
            Features.RedirectAsynchronousRequests.Enabled = false;

            // Log request and response
            Features.LogRequestAndResponse.Enabled = true;

            // We don't yet have support for this
            Features.SaveExecutionInformation.Enabled = false;
            Features.SaveExecutionInformation.SaveBeforeExecutionAsyncDelegate = null;
            Features.SaveExecutionInformation.SaveAfterExecutionAsyncDelegate = null;

            // Use batch logs to filter out chatty logs when successful
            // The default is false, because you must set
            // FulcrumApplication.Setup.SynchronousFastLogger = new BatchLogger(<your logger>)
            // before you can enable this.
            Features.BatchLog.Enabled = false;
            Features.BatchLog.FlushAsLateAsPossible = false;
            Features.BatchLog.Threshold = LogSeverityLevel.Warning;

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